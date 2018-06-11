using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

//spawn the hub room
public class HubRoom : MonoBehaviour {

    public ThemeSettings themeSettings;

    public float wallHeight, doorHeight, doorWidth, tileSize;

    public Material stencilMaterial;

    System.Random rng;
    int seed;

    Vector3 _SW, _SE, _NW, _NE;

    Room room;

    void Start () {
        seed = Random.Range(0, int.MaxValue);
        rng = new System.Random(seed);

        float away = 12;
        Vector2 ran = Random.insideUnitCircle;
        _SW = new Vector3(-away, 0, -away) + new Vector3(ran.x * 5, 0, ran.y * 5);
        ran = Random.insideUnitCircle;
        _SE = new Vector3(away, 0, -away) + new Vector3(ran.x * 5, 0, ran.y * 5);
        ran = Random.insideUnitCircle;
        _NW = new Vector3(-away, 0, away) + new Vector3(ran.x * 5, 0, ran.y * 5);
        ran = Random.insideUnitCircle;
        _NE = new Vector3(away, 0, away) + new Vector3(ran.x * 5, 0, ran.y * 5);

        StartCoroutine(Generate());
    }

    IEnumerator Generate() {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        GenerateRooms();
        yield return new WaitForEndOfFrame();

        SpawnRoom();
        yield return new WaitForEndOfFrame();

        SpawnStencilRoom();
        yield return new WaitForEndOfFrame();

        SpawnGameplay();
        yield return new WaitForEndOfFrame();

        Decorate();
        yield return new WaitForEndOfFrame();

        Destroy(room);

        sw.Stop();
        Debug.Log("Generated in " + sw.ElapsedMilliseconds + " ms/ " + sw.ElapsedTicks + " ticks");
    }

    //generate info to spawn a room
    void GenerateRooms() {
        Node node = new Node(0, 0);
        node.room = true;
        RoomData roomData = new RoomData(_SW, _SE, _NW, _NE, node );
        roomData.SetupRoom(2.2f);
        Part part = roomData.parts[0];
        part.hasNorthEnt = true;
        roomData.parts[0] = part;

        GameObject child = new GameObject("Room " + node.posX + "x" + node.posY);
        child.transform.parent = transform;
        room = child.AddComponent<Room>();
        room.roomData = roomData;

        GameObject go = new GameObject("content");
        go.transform.parent = child.transform;
    }

    //spawn the mesh for the room
    void SpawnRoom() {
        GameObject child = room.gameObject;
        GameObject go = child.transform.GetChild(0).gameObject;

        go.layer = LayerMask.NameToLayer("Room");

        RoomData roomdata = room.roomData;

        MeshCollider meshCollider = child.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = GetCollision(roomdata);
        meshCollider.sharedMesh.name = seed.ToString() + " collision";
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshCollider = go.AddComponent<MeshCollider>();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        List<Vector3> roomData = new List<Vector3>();

        foreach (Part part in roomdata.parts) {
            RoomMesh.GenerateMesh(part.SW, part.SE, part.NW, part.NE,
                    new WallData(part.hasNorth, part.hasSouth, part.hasWest, part.hasEast).Entrances(part.hasNorthEnt, part.hasSouthEnt, part.hasWestEnt, part.hasEastEnt),
                    wallHeight, doorHeight, doorWidth, tileSize, verts, uvs, tris);
        }

        Mesh mesh = new Mesh {
            name = "terrain " + seed.ToString(),
            vertices = verts.ToArray(),
            triangles = tris.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshRenderer.material = themeSettings.roomMaterial;

        meshCollider.sharedMesh = mesh;
    }

    //add the mesh used by the stencil shader
    void SpawnStencilRoom() {
        GameObject go = new GameObject("Stencil");
        go.transform.parent = room.transform.GetChild(0);
        go.transform.localPosition += Vector3.up * 0.0003f;

        MeshFilter filter = go.AddComponent<MeshFilter>();
        MeshRenderer render = go.AddComponent<MeshRenderer>();

        //filter.mesh = room.roomData.GetStencilMesh(wallheight);
        filter.mesh = room.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
        render.material = stencilMaterial;
    }

    //can contain shops items, etc
    //wip
    void SpawnGameplay() {

    }

    //to decorate the room
    void Decorate() {
        SpawnDoors();
        SpawnDecoration();
        SpawnFog();
    }

    void SpawnDoors() {
        foreach (KeyValuePair<Vector3, float> doorPos in room.roomData.doorLocations) {
            GameObject go = Instantiate(themeSettings.doorObject, doorPos.Key, Quaternion.Euler(0, doorPos.Value, 0));
            go.transform.parent = room.transform.GetChild(0).transform;

        }
    }

    void SpawnDecoration() {
        int[] arr = new int[] { 0, 2, 2, 1, 1, 3, 3, 0 };
        for (int i = 0; i < room.roomData.parts.Count; i++) {
            Part currentPart = room.roomData.parts[i];

            Vector3 insideSW = Vector3.Lerp(currentPart.SW, currentPart.NE, 0.05f);
            Vector3 insideNE = Vector3.Lerp(currentPart.SW, currentPart.NE, 0.95f);
            Vector3 insideSE = Vector3.Lerp(currentPart.SE, currentPart.NW, 0.05f);
            Vector3 insideNW = Vector3.Lerp(currentPart.SE, currentPart.NW, 0.95f);

            Vector3[] insideArr = new Vector3[] { insideSW, insideNE, insideSE, insideNW };

            List<Vector3> list = new List<Vector3>();


            for (int p = 0; p < themeSettings.decorateObjects.Length; p++) {
                ObjectCategory cat = themeSettings.decorateObjects[p];
                int amount = room.roomData.roomType == RoomType.Room ? rng.Next(cat.roomAmountDecorate.x, cat.roomAmountDecorate.y) : rng.Next(cat.roomAmountDecorate.x / room.roomData.parts.Count, cat.roomAmountDecorate.y / room.roomData.parts.Count);

                list.Clear();

                while (list.Count < amount) {
                    //for (int k = 0; k < amount / room.roomData.parts.Count; k++) {
                    bool add = false;
                    Vector3 addVec3 = Vector3.zero;
                    for (int h = 0; h < (list.Count == 0 ? 1 : list.Count); h++) {
                        //foreach (Vector3 item in list) {
                        if (cat.place == Place.Wall) {
                            int ran = rng.Next(0, 4);
                            int perc1 = rng.Next(10, 190);

                            Vector3 place = new Vector3();
                            float rotY = 0; ;
                            if (ran == 0 && currentPart.hasNorth) {
                                if (currentPart.hasNorthEnt && perc1 >= 80 && perc1 <= 120)
                                    perc1 += 40;

                                place = Vector3.Lerp(currentPart.NW, currentPart.NE, perc1 * 0.005f);
                                rotY = Quaternion.LookRotation(currentPart.NW - currentPart.NE).eulerAngles.y;
                            } else if (ran == 1 && currentPart.hasEast) {
                                if (currentPart.hasEastEnt && perc1 >= 80 && perc1 <= 120)
                                    perc1 += 40;

                                place = Vector3.Lerp(currentPart.NE, currentPart.SE, perc1 * 0.005f);
                                rotY = Quaternion.LookRotation(currentPart.NE - currentPart.SE).eulerAngles.y;
                            } else if (ran == 2 && currentPart.hasSouth) {
                                if (currentPart.hasSouthEnt && perc1 >= 80 && perc1 <= 120)
                                    perc1 += 40;

                                place = Vector3.Lerp(currentPart.SE, currentPart.SW, perc1 * 0.005f);
                                rotY = Quaternion.LookRotation(currentPart.SE - currentPart.SW).eulerAngles.y;
                            } else if (ran == 3 && currentPart.hasWest) {
                                if (currentPart.hasWestEnt && perc1 >= 80 && perc1 <= 120)
                                    perc1 += 40;

                                place = Vector3.Lerp(currentPart.SW, currentPart.NW, perc1 * 0.005f);
                                rotY = Quaternion.LookRotation(currentPart.SW - currentPart.NW).eulerAngles.y;
                            }

                            if (list.Count == 0 ? true : Vector3.Distance(list[h], place) > 2f) {
                                add = true;
                                addVec3 = place;

                                GameObject go = Instantiate(cat.objects[rng.Next(0, cat.objects.Length)], room.transform.GetChild(0));
                                go.transform.position = place + Vector3.up * cat.positionGridPlace;
                                go.transform.eulerAngles = new Vector3(0, rotY, 0);
                                break;
                            }
                        } else {
                            int index1 = rng.Next(0, 4), index2 = rng.Next(0, 4);
                            int perc1 = rng.Next(0, 200), perc2 = rng.Next(0, 200), perc3 = rng.Next(0, 200);
                            Vector3 pos1 = Vector3.Lerp(insideArr[arr[index1 * 2]], insideArr[arr[(index1 * 2) + 1]], perc1 * 0.005f);
                            Vector3 pos2 = Vector3.Lerp(insideArr[arr[index2 * 2]], insideArr[arr[(index2 * 2) + 1]], perc2 * 0.005f);
                            Vector3 pos3 = Vector3.Lerp(pos1, pos2, perc3 * 0.005f);

                            if (list.Count == 0 ? true : Vector3.Distance(list[h], pos3) > 2f) {
                                add = true;
                                addVec3 = pos3;

                                GameObject go = Instantiate(cat.objects[rng.Next(0, cat.objects.Length)], room.transform.GetChild(0));

                                if (cat.positionGridPlace > float.Epsilon) {
                                    float point = cat.positionGridPlace;
                                    pos3.x = (int)(pos3.x / point + 0.5f) * point;
                                    pos3.z = (int)(pos3.z / point + 0.5f) * point;
                                }
                                go.transform.position = pos3 + Vector3.up * 0.0001f;

                                float y = Mathf.Clamp(cat.rotationMinMax.x + rng.Next(0, (int)cat.rotationMultiplier) * cat.rotationIncrements, cat.rotationMinMax.x, cat.rotationMinMax.y);

                                go.transform.eulerAngles = new Vector3(0, y, 0);
                                break;
                            }
                        }
                    }
                    if (add) {
                        list.Add(addVec3);
                    }
                }
            }
        }
    }

    void SpawnFog() {
        if (themeSettings.fogObject != null) {
            GameObject go = Instantiate(themeSettings.fogObject, room.transform.GetChild(0));
            go.transform.position = room.roomData.node.worldPosition + Vector3.up * 0.2f;

            Vector3[] arr = new Vector3[] { room.roomData.SW, room.roomData.NW, room.roomData.NW, room.roomData.NE };
            int indexClosest = 0;
            for (int i = 0; i < arr.Length; i++) {
                if (Vector3.Distance(room.roomData.node.worldPosition, arr[i]) > Vector3.Distance(room.roomData.node.worldPosition, arr[indexClosest])) {
                    indexClosest = i;
                }
            }
            Vector3 v = room.roomData.node.worldPosition - arr[indexClosest];
            Vector3 v1 = new Vector3(Mathf.Abs(v.x * 2), Mathf.Abs(v.z * 2), 0.3f);

            ParticleSystem system = go.GetComponent<ParticleSystem>();
            var shape = system.shape;
            shape.scale = v1;
        }
    }

    Mesh GetCollision(RoomData room) {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        List<Vector3> roomData = new List<Vector3>();
        foreach (Vector3 roomvector in room.GetTriggerData(wallHeight)) {
            roomData.Add(roomvector);
        }

        for (int i = 0; i < roomData.Count / 4; i++) {
            int triCount = tris.Count;
            verts.Add(roomData[(i * 4) + 0]);
            verts.Add(roomData[(i * 4) + 1]);
            verts.Add(roomData[(i * 4) + 2]);
            tris.Add(triCount + 0);
            tris.Add(triCount + 2);
            tris.Add(triCount + 1);

            triCount = tris.Count;
            verts.Add(roomData[(i * 4) + 1]);
            verts.Add(roomData[(i * 4) + 2]);
            verts.Add(roomData[(i * 4) + 3]);
            tris.Add(triCount + 0);
            tris.Add(triCount + 1);
            tris.Add(triCount + 2);

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
        }

        Mesh mesh = new Mesh {
            name = "terrain " + seed.ToString(),
            vertices = verts.ToArray(),
            triangles = tris.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();

        return mesh;
    }
}
