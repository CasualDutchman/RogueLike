using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LevelGeneration))]
public class LevelGenerationEditor : Editor {

    LevelGeneration gen;

    private void OnEnable() {
        gen = (LevelGeneration)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate")) {
            gen.ClearLevel();
            gen.GenerateLevel(gen.settings, true);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        if (GUILayout.Button("+Stage")) {
            gen.genStages += 1;
            gen.ClearLevel();
            gen.GenerateLevel(gen.settings, true);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        if (GUILayout.Button("Random Gen")) {
            //gen.settings.seed = Random.Range(int.MinValue, int.MaxValue);
            gen.ClearLevel();
            gen.GenerateLevel(gen.settings, true);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        GUILayout.EndHorizontal();
    }
}
#endif

public class LevelGeneration : MonoBehaviour {

    public bool useLoadingScreen;
    public GameObject loadingScreen;

    //public Material testMaterial;
    public Transform playerTransform;
    public Transform cameraRig;

    //public GameObject doorObj;
    //public GameObject[] gameplayObjects;
    //public GameObject[] decorationObjects;

    public ThemeSettings themeSettings;

    public LevelManager levelManager;

    public LevelSettings settings;

    public int genStages;

    public Material stencilMaterial;

    Node spawnNode, endNode;
    Node prevNode;
    List<Node> nodeArray = new List<Node>();
    int mainRoadSize;

    System.Random rng;
    int seed = 0;

    public bool debugRoads;
    public bool debugNodes;
    public bool debugOffsetMap;
    public bool debugRoomBorders;
    public bool debugRooms;

    //public Vector2Int mainRoadRange;
    //public int branchWeight;
    //public int maxBranchOut;

    //public Vector2 offsetSpacing;
    //public Vector2 offsetLocal;
    Vector3[,] offsetMap;
    
    List<Room> roomLayout = new List<Room>();

    public float minHallWidth;
    public float wallheight;

    public float doorWidth;
    public float doorHeight;

    public float tileSize;

    [ContextMenu("Clear Level")]
    public void ClearLevel() {
        spawnNode = null;
        endNode = null;
        prevNode = null;
        nodeArray.Clear();
        offsetMap = null;
        roomLayout.Clear();

        GetComponent<NavMeshSurface>().RemoveData();

        int maxChild = transform.childCount;
        for (int i = 0; i < maxChild; i++) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public void GenerateLevel(LevelSettings lvlset, bool spawn) {
        settings = lvlset;
        seed = lvlset.seed;
        rng = new System.Random(seed);

        Node node = new Node(0, 0);
        nodeArray.Add(node);
        spawnNode = node;

        prevNode = AddNode(node, Direction.North, false);

        if (useLoadingScreen)
            loadingScreen.SetActive(true);

        StartCoroutine(Generate(spawn));
    }

    IEnumerator Generate(bool spawn) {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        GenerateMainRoad();
        yield return new WaitForEndOfFrame();
        if (genStages >= 1) {
            GenerateBranchNodes();
            yield return new WaitForEndOfFrame();
        }
        if (genStages >= 2) {
            GenerateOffsets();
            yield return new WaitForEndOfFrame();
        }
        if (genStages >= 3) {
            GenerateRooms();
            yield return new WaitForEndOfFrame();
        }
        if (genStages >= 4) {
            //SpawnRooms();
            int index = 0;
            while (index < roomLayout.Count) {
                SpawnRoom(roomLayout[index]);
                SpawnStencilRoom(roomLayout[index]);
                index++;
                yield return new WaitForEndOfFrame();
            }
        }
        if (genStages >= 5) {
            SpawnGameplay();
            yield return new WaitForEndOfFrame();
        }
        if (genStages >= 6) {
            GetComponent<NavMeshSurface>().BuildNavMesh();
            yield return new WaitForEndOfFrame();
        }
        if (genStages >= 7) {
            Decorate();
            yield return new WaitForEndOfFrame();
        }
        if (genStages >= 8) {
            SpawnEnemies();
            yield return new WaitForEndOfFrame();
        }

        playerTransform.position = spawnNode.worldPosition;
        playerTransform.GetComponent<PlayerMovement>().SetCharacter();
        cameraRig.position = spawnNode.worldPosition;

        if (!spawn) {
            for (int i = 1; i < levelManager.rooms.Count; i++) {
                levelManager.rooms[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        sw.Stop();
        Debug.Log("Generated in " + sw.ElapsedMilliseconds + " ms/ " + sw.ElapsedTicks + " ticks");

        loadingScreen.SetActive(false);

        if (!spawn) {
            Destroy(this);
        }
    }

    void GenerateMainRoad() {
        int mainRoadMax = rng.Next(settings.mainRoadLengthMin, settings.mainRoadLengthMax);
        for (int i = 0; i <= mainRoadMax; i++) {

            Direction newDir;
            bool canSpawn = GetAvailableDirection(prevNode, out newDir);
            if (canSpawn) {
                Node node = AddNode(prevNode, newDir, false);
                prevNode = node;
                if (i == mainRoadMax) {
                    endNode = node;
                }
            } else {
                endNode = prevNode;
                break;
            }
        }

        mainRoadSize = nodeArray.Count;
    }

    void GenerateBranchNodes() {
        for (int b = 0; b < settings.maxBranchOut; b++) {
            int max = nodeArray.Count - 2;
            for (int i = 0; i < max; i++) {
                Node workingNode = nodeArray[i + 1];
                if (workingNode == endNode)
                    continue;

                if (rng.Next(settings.branchWeight) == 0) {
                    Direction newDir;
                    bool canSpawn = GetAvailableDirection(workingNode, out newDir, true);
                    if (canSpawn) {
                        Node node = AddNode(workingNode, newDir, true);
                    }
                }
            }
        }
    }

    void GenerateOffsets() {
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        for (int i = 1; i < nodeArray.Count; i++) {
            Node node = nodeArray[i];
            if (node.posX < minX)
                minX = node.posX;
            if (node.posX > maxX)
                maxX = node.posX;
            if (node.posY < minY)
                minY = node.posY;
            if (node.posY > maxY)
                maxY = node.posY;
        }

        int difX = Mathf.Abs(minX) + maxX + 1 + 1;
        int difY = Mathf.Abs(minY) + maxY + 1 + 1;

        offsetMap = new Vector3[difX, difY];

        int currentX = 0;
        int currentY = 0;

        int addX = rng.Next((int)(settings.offsetSpacingMin * 100), (int)(settings.offsetSpacingMax * 100));
        int addY = rng.Next((int)(settings.offsetSpacingMin * 100), (int)(settings.offsetSpacingMax * 100));

        for (int y = 0; y < difY; y++) {
            for (int x = 0; x < difX; x++) {
                currentX = (addX * x) + rng.Next((int)(settings.offsetLocalMin * 100), (int)(settings.offsetLocalMax * 100));
                currentY = (addY * y) + rng.Next((int)(settings.offsetLocalMin * 100), (int)(settings.offsetLocalMax * 100));

                offsetMap[x, y] = new Vector3(currentX * 0.01f, 0, currentY * 0.01f);
            }
        }

        for (int i = 0; i < nodeArray.Count; i++) {
            Node node = nodeArray[i];

            int x = Mathf.Abs(minX) + node.posX;
            int y = Mathf.Abs(minY) + node.posY;

            Vector3 a1 = offsetMap[x, y];
            Vector3 a2 = offsetMap[x + 1, y];
            Vector3 a3 = offsetMap[x, y + 1];
            Vector3 a4 = offsetMap[x + 1, y + 1];
            Vector3 b = (a1 + a2 + a3 + a4) / 4;

            node.worldPosition = b;
            node.posX = x;
            node.posY = y;
        }
        
    }

    void GenerateRooms() {
        for (int i = 0; i < mainRoadSize; i++) {
            nodeArray[i].room = i % 2 == 0 || i == mainRoadSize - 1 || i == 0 || rng.Next(10) > 7;
        }
        for (int i = mainRoadSize; i < nodeArray.Count; i++) {
            nodeArray[i].room = rng.Next(10) >= 4;
        }

        for (int i = 0; i < nodeArray.Count; i++) {
            Node node = nodeArray[i];
            RoomData roomData = new RoomData(
                offsetMap[node.posX, node.posY],
                offsetMap[node.posX + 1, node.posY],
                offsetMap[node.posX, node.posY + 1],
                offsetMap[node.posX + 1, node.posY + 1],
                node
            );
            roomData.SetupRoom(minHallWidth);

            GameObject child = new GameObject("Room " + node.posX + "x" + node.posY);
            child.transform.parent = transform;
            Room room = child.AddComponent<Room>();
            room.roomData = roomData;
            room.endRoom = endNode == node;

            roomLayout.Add(room);

            GameObject go = new GameObject("content");
            go.transform.parent = child.transform;

            if(levelManager != null)
                levelManager.rooms.Add(room);
        }
    }

    void SpawnGameplay() {
        foreach (Room room in roomLayout) {
            if (room.roomData.node == spawnNode || room.roomData.node == endNode)
                continue;

            if (room.roomData.roomType == RoomType.Room) {
                List<Vector3> list = new List<Vector3>();
                Vector3 insideSW = Vector3.Lerp(room.roomData.SW, room.roomData.NE, 0.2f);
                Vector3 insideNE = Vector3.Lerp(room.roomData.SW, room.roomData.NE, 0.8f);
                Vector3 insideSE = Vector3.Lerp(room.roomData.SE, room.roomData.NW, 0.2f);
                Vector3 insideNW = Vector3.Lerp(room.roomData.SE, room.roomData.NW, 0.8f);
                Vector3[] insideArr = new Vector3[] { insideSW, insideNE, insideSE, insideNW };
                int[] arr = new int[] { 0, 2, 2, 1, 1, 3, 3, 0 };
                int amount = rng.Next(themeSettings.roomAmountGameplay.x, themeSettings.roomAmountGameplay.y);
                while (list.Count < amount) {
                    int index1 = rng.Next(0, 4), index2 = rng.Next(0, 4);
                    int perc1 = rng.Next(0, 200), perc2 = rng.Next(0, 200), perc3 = rng.Next(0, 200);
                    Vector3 pos1 = Vector3.Lerp(insideArr[arr[index1 * 2]], insideArr[arr[(index1 * 2) + 1]], perc1 * 0.005f);
                    Vector3 pos2 = Vector3.Lerp(insideArr[arr[index2 * 2]], insideArr[arr[(index2 * 2) + 1]], perc2 * 0.005f);
                    Vector3 pos3 = Vector3.Lerp(pos1, pos2, perc3 * 0.005f);
                    if (list.Count <= 0) {
                        ObjectCategory cat = themeSettings.gameplayObjects[rng.Next(themeSettings.gameplayObjects.Length)];

                        GameObject go = Instantiate(cat.objects[rng.Next(0, cat.objects.Length)], room.transform.GetChild(0));

                        go.layer = LayerMask.NameToLayer("Room");

                        if (cat.positionGridPlace > float.Epsilon) {
                            float point = cat.positionGridPlace;
                            pos3.x = (int)(pos3.x / point + 0.5f) * point;
                            pos3.z = (int)(pos3.z / point + 0.5f) * point;
                        }
                        list.Add(pos3);
                        go.transform.position = pos3 + Vector3.up * 0.0001f;

                        float y = Mathf.Clamp(cat.rotationMinMax.x + rng.Next(0, (int)cat.rotationMultiplier) * cat.rotationIncrements, cat.rotationMinMax.x, cat.rotationMinMax.y);

                        go.transform.eulerAngles = new Vector3(0, y, 0);
                    } else {
                        bool add = false;
                        Vector3 addVec3 = Vector3.zero;
                        foreach(Vector3 vec3 in list) {
                            if (Vector3.Distance(vec3, pos3) > 5.5f) {
                                add = true;
                                ObjectCategory cat = themeSettings.gameplayObjects[rng.Next(themeSettings.gameplayObjects.Length)];

                                GameObject go = Instantiate(cat.objects[rng.Next(0, cat.objects.Length)], room.transform.GetChild(0));

                                go.layer = LayerMask.NameToLayer("Room");

                                if (cat.positionGridPlace > float.Epsilon) {
                                    float point = cat.positionGridPlace;
                                    pos3.x = (int)(pos3.x / point + 0.5f) * point;
                                    pos3.z = (int)(pos3.z / point + 0.5f) * point;
                                }
                                addVec3 = pos3;
                                go.transform.position = pos3 + Vector3.up * 0.0001f;

                                float y = Mathf.Clamp(cat.rotationMinMax.x + rng.Next(0, (int)cat.rotationMultiplier) * cat.rotationIncrements, cat.rotationMinMax.x, cat.rotationMinMax.y);

                                go.transform.eulerAngles = new Vector3(0, y, 0);
                                break;
                            } else {
                                continue;
                            }
                        }
                        if (add) {
                            list.Add(addVec3);
                        }
                    }
                }
            } else {
                continue;
            }
        }
    }

    void Decorate() {
        SpawnDoors();
        SpawnDecoration();
        SpawnFog();
    }

    void SpawnDoors() {
        foreach (Room room in roomLayout) {
            foreach (KeyValuePair<Vector3, float> doorPos in room.roomData.doorLocations) {
                GameObject go = Instantiate(themeSettings.doorObject, doorPos.Key, Quaternion.Euler(0, doorPos.Value, 0));
                go.transform.parent = room.transform.GetChild(0).transform;

            }
        }
    }

    void SpawnDecoration() {
        foreach (Room room in roomLayout) {
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
    }

    void SpawnFog() {
        if (themeSettings.fogObject != null) {
            foreach (Room room in roomLayout) {
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
    }

    void SpawnEnemies() {
        foreach (Room room in roomLayout) {
            if (room.roomData.node == spawnNode || room.roomData.node == endNode)
                continue;

            if (room.roomData.roomType != RoomType.Room)
                continue;

            Vector3 insideSW = Vector3.Lerp(room.roomData.SW, room.roomData.NE, 0.2f);
            Vector3 insideNE = Vector3.Lerp(room.roomData.SW, room.roomData.NE, 0.8f);
            Vector3 insideSE = Vector3.Lerp(room.roomData.SE, room.roomData.NW, 0.2f);
            Vector3 insideNW = Vector3.Lerp(room.roomData.SE, room.roomData.NW, 0.8f);
            Vector3[] insideArr = new Vector3[] { insideSW, insideNE, insideSE, insideNW };
            int[] arr = new int[] { 0, 2, 2, 1, 1, 3, 3, 0 };
            int amount = rng.Next((int)themeSettings.randomPerRoom.x, (int)themeSettings.randomPerRoom.y);
            for (int i = 0; i < amount; i++) {
                int index1 = rng.Next(0, 4), index2 = rng.Next(0, 4);
                int perc1 = rng.Next(0, 200), perc2 = rng.Next(0, 200), perc3 = rng.Next(0, 200);
                Vector3 pos1 = Vector3.Lerp(insideArr[arr[index1 * 2]], insideArr[arr[(index1 * 2) + 1]], perc1 * 0.005f);
                Vector3 pos2 = Vector3.Lerp(insideArr[arr[index2 * 2]], insideArr[arr[(index2 * 2) + 1]], perc2 * 0.005f);
                Vector3 pos3 = Vector3.Lerp(pos1, pos2, perc3 * 0.005f);

                GameObject go = Instantiate(themeSettings.enemies[Random.Range(0, themeSettings.enemies.Length)], room.transform.GetChild(0));
                go.transform.position = pos3;
                go.GetComponent<Enemy>().target = playerTransform;
                //go.SetActive(false);
            }
        }
    }

    void SpawnStencilRoom(Room room) {
        GameObject go = new GameObject("Stencil");
        go.transform.parent = room.transform.GetChild(0);
        go.transform.localPosition += Vector3.up * 0.0003f;

        MeshFilter filter = go.AddComponent<MeshFilter>();
        MeshRenderer render = go.AddComponent<MeshRenderer>();

        //filter.mesh = room.roomData.GetStencilMesh(wallheight);
        filter.mesh = room.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
        render.material = stencilMaterial;
    }

    void SpawnRoom(Room room) {
        GameObject child = room.gameObject;
        
        GameObject go = child.transform.GetChild(0).gameObject;
        //go.SetActive(false);

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
                    wallheight, doorHeight, doorWidth, tileSize, verts, uvs, tris);
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

    Mesh GetCollision(RoomData room) {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        List<Vector3> roomData = new List<Vector3>();
        foreach (Vector3 roomvector in room.GetTriggerData(wallheight)) {
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

    bool GetAvailableDirection(Node node, out Direction dir, bool plusSouth = false) {
        List<Direction> avail = new List<Direction>(node.GetAvailableDirections(plusSouth));

        dir = Direction.North;

        if (avail.Count <= 0) {
            return false;
        }

        bool searching = true;
        bool found = false;
        bool same = false;
        while (searching) {
            same = false;
            Direction newDir = avail[rng.Next(0, avail.Count)];
            int posX = newDir == Direction.East ? 1 : (newDir == Direction.West ? -1 : 0);
            int posY = newDir == Direction.North ? 1 : (newDir == Direction.South ? -1 : 0);

            foreach (Node n in nodeArray) {
                if(n.posX == (node.posX + posX) && n.posY == (node.posY + posY)) {
                    same = true;
                    break;
                }
            }

            if (same) {
                avail.Remove(newDir);
                if(avail.Count <= 0){
                    searching = false;
                    break;
                }
                same = false;
            } else {
                dir = newDir;
                found = true;
                searching = false;
                break;
            }
        }
        return found;
    }

    Node AddNode(Node parentNode, Direction dir, bool br) {
        Node node = new Node(parentNode, dir);
        if(br) {
            if (parentNode.branchIndex == 0) {
                node.endbranch = true;
            } else {
                if (parentNode.endbranch) {
                    node.endbranch = true;
                    parentNode.endbranch = false;
                }
            }
            node.branchIndex = parentNode.branchIndex + 1;
        }
        node.AddNeighborNode(parentNode, dir, true);
        parentNode.AddNeighborNode(node, dir);
        nodeArray.Add(node);
        return node;
    }

    void OnDrawGizmosSelected() {
        foreach (Node node in nodeArray) {
            Gizmos.color = new Color(node == endNode || node == spawnNode ? 1 : 0, node.branchIndex == 0 ? 1 : 0, node.endbranch ? 1 : 0);

            float multiplier = genStages >= 2 ? 10f : 1f;
            Vector3 pos = node.worldPosition;
            pos.y = 0.1f;

            if (debugNodes) {
                if (node.room) {
                    Gizmos.DrawWireCube(pos, new Vector3(0.3f, 0.1f, 0.3f) * ((node == endNode || node == spawnNode ? 1.2f : 1f) * multiplier));
                } else {
                    Gizmos.DrawWireSphere(pos, (node == endNode || node == spawnNode ? 0.2f : 0.1f) * multiplier);
                }
            }
            if (debugRoads) {
                if (node.northNode != null) Gizmos.DrawLine(node.worldPosition, node.northNode.worldPosition);
                if (node.southNode != null) Gizmos.DrawLine(node.worldPosition, node.southNode.worldPosition);
                if (node.eastNode != null) Gizmos.DrawLine(node.worldPosition, node.eastNode.worldPosition);
                if (node.westNode != null) Gizmos.DrawLine(node.worldPosition, node.westNode.worldPosition);
            }
        }

        if (debugOffsetMap && offsetMap != null) {
            Gizmos.color = Color.yellow;
            for (int y = 0; y < offsetMap.GetLength(1); y++) {
                for (int x = 0; x < offsetMap.GetLength(0); x++) {
                    Gizmos.DrawWireSphere(offsetMap[x, y], 0.5f);
                }
            }
        }
        
        if (debugRoomBorders && roomLayout.Count > 0) {
            Gizmos.color = Color.white;
            for (int i = 0; i < roomLayout.Count; i++) {
                RoomData room = roomLayout[i].GetComponent<Room>().roomData;

                Gizmos.DrawWireSphere(room.SW, 0.1f);
                Gizmos.DrawWireSphere(room.SE, 0.1f);
                Gizmos.DrawWireSphere(room.NW, 0.1f);
                Gizmos.DrawWireSphere(room.NE, 0.1f);

                Gizmos.DrawLine(room.SW, room.SE);
                Gizmos.DrawLine(room.SE, room.NE);
                Gizmos.DrawLine(room.NE, room.NW);
                Gizmos.DrawLine(room.NW, room.SW);
            }
        }
    }
}

public enum Direction { North, East, South, West }

[System.Serializable]
public class Node {
    public int posX;
    public int posY;
    public Vector3 worldPosition;
    public bool endbranch;
    public int branchIndex = 0;
    public Node northNode;
    public Node eastNode;
    public Node southNode;
    public Node westNode;

    public bool room = false;

    public Node(int x, int y) {
        posX = x;
        posY = y;
        worldPosition = new Vector3(x, 0, y);
    }

    public Node(Node n, Direction dir) {
        posX = n.posX;
        posY = n.posY;
        posX += dir == Direction.East ? 1 : (dir == Direction.West ? -1 : 0);
        posY += dir == Direction.North ? 1 : (dir == Direction.South ? -1 : 0);
        worldPosition = new Vector3(posX, 0, posY);
    }

    public Direction[] GetAvailableDirections(bool plusSouth = false) {
        List<Direction> dirs = new List<Direction>();
        if (northNode == null) dirs.Add(Direction.North);
        if (southNode == null && plusSouth) dirs.Add(Direction.South);
        if (eastNode == null) dirs.Add(Direction.East);
        if (westNode == null) dirs.Add(Direction.West);
        return dirs.ToArray();
    }

    public void AddNeighborNode(Node node, Direction dir, bool reversed = false) {
        switch (dir) {
            case Direction.North: if (reversed) { southNode = node; } else { northNode = node; } break;
            case Direction.East: if (reversed) { westNode = node; } else { eastNode = node; } break;
            case Direction.South: if (reversed) { northNode = node; } else { southNode = node; } break;
            case Direction.West: if (reversed) { eastNode = node; } else { westNode = node; } break;
        }
    }
}

public enum RoomType { Hallway, Room }

[System.Serializable]
public class RoomData {
    public Vector3 SW, SE, NW, NE;
    public RoomType roomType;
    public Node node;
    public GameObject roomObject;

    public Dictionary<Vector3, float> doorLocations = new Dictionary<Vector3, float>();
    public List<Part> parts = new List<Part>();

    public bool hasNorth = true;
    public bool hasWest = true;
    public bool hasEast = true;
    public bool hasNorthEntrance;
    public bool hasSouthEntrance;
    public bool hasWestEntrance;
    public bool hasEastEntrance;

    public RoomData(Vector3 sw, Vector3 se, Vector3 nw, Vector3 ne, Node n) {
        SW = sw;
        SE = se;
        NW = nw;
        NE = ne;
        node = n;
    }

    public List<Vector3> GetTriggerData(float wallH) {
        List<Vector3> list = new List<Vector3>();

        list.Add(SW);
        list.Add(SE);
        list.Add(NW);
        list.Add(NE);

        list.Add(SE);
        list.Add(SW);
        list.Add(NE);
        list.Add(NW);

        foreach (Vector3 v in GetWall(SW, SE, false, 0, 0, wallH)) {
            list.Add(v);
        }
        foreach (Vector3 v in GetWall(SE, NE, false, 0, 0, wallH)) {
            list.Add(v);
        }
        foreach (Vector3 v in GetWall(NE, NW, false, 0, 0, wallH)) {
            list.Add(v);
        }
        foreach (Vector3 v in GetWall(NW, SW, false, 0, 0, wallH)) {
            list.Add(v);
        }

        return list;
    }
    
    public Mesh GetStencilMesh(float height) {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        foreach(Part part in parts) {
            Quad(part.SW, part.SE, part.NW, part.NE, verts, tris, uvs);

            Quad(part.NW, part.NE, part.NW + Vector3.up * height, part.NE + Vector3.up * height, verts, tris, uvs);
            Quad(part.NE, part.SE, part.NE + Vector3.up * height, part.SE + Vector3.up * height, verts, tris, uvs);
            Quad(part.SE, part.SW, part.SE + Vector3.up * height, part.SW + Vector3.up * height, verts, tris, uvs);
            Quad(part.SW, part.NW, part.SW + Vector3.up * height, part.NW + Vector3.up * height, verts, tris, uvs);

            Quad(part.SE + Vector3.up * height, part.SW + Vector3.up * height, part.NE + Vector3.up * height, part.NW + Vector3.up * height, verts, tris, uvs);
        }

        Mesh mesh = new Mesh();
        mesh.name = "Show me";
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    void Quad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, List<Vector3> verts, List<int> tris, List<Vector2> uvs) {
        verts.Add(v2);
        verts.Add(v3);
        verts.Add(v1);

        verts.Add(v4);
        verts.Add(v3);
        verts.Add(v2);

        int triCount = tris.Count;
        tris.Add(triCount + 0);
        tris.Add(triCount + 1);
        tris.Add(triCount + 2);

        tris.Add(triCount + 3);
        tris.Add(triCount + 4);
        tris.Add(triCount + 5);

        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);

        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
    }

    List<Vector3> GetWall(Vector3 v1, Vector3 v2, bool hasDoor, float doorH, float doorW, float wallH) {
        List<Vector3> wall = new List<Vector3>();

        if (hasDoor) {
            float point = (doorW / Vector3.Distance(v1, v2)) * 0.5f;
            Vector3 d1 = Vector3.Lerp(v1, v2, 0.5f - point);
            Vector3 d2 = Vector3.Lerp(v1, v2, 0.5f + point);

            wall.Add(v1);
            wall.Add(d1);
            wall.Add(v1 + Vector3.up * wallH);
            wall.Add(d1 + Vector3.up * doorH);

            wall.Add(d1 + Vector3.up * doorH);
            wall.Add(d2 + Vector3.up * doorH);
            wall.Add(v1 + Vector3.up * wallH);
            wall.Add(v2 + Vector3.up * wallH);

            wall.Add(d2);
            wall.Add(v2);
            wall.Add(d2 + Vector3.up * doorH);
            wall.Add(v2 + Vector3.up * wallH);
        } 
        else {
            wall.Add(v1);
            wall.Add(v2);
            wall.Add(v1 + Vector3.up * wallH);
            wall.Add(v2 + Vector3.up * wallH);
        }

        return wall;
    }

    public void SetupRoom(float minHallWidth) {
        roomType = node.room ? RoomType.Room : RoomType.Hallway;

        float left = 0.33f;
        float right = 0.67f;

        if (node.room) {
            parts.Add(new Part() {
                SW = this.SW,
                SE = this.SE,
                NW = this.NW,
                NE = this.NE,
                hasEast = true,
                hasNorth = true,
                hasWest = true,
                hasSouth = true,
                hasNorthEnt = node.northNode != null,
                hasSouthEnt = node.southNode != null,
                hasWestEnt = node.westNode != null,
                hasEastEnt = node.eastNode != null
            });

            if (node.northNode != null) { AddDoorData(NW, NE); }
            if (node.southNode != null) { AddDoorData(SE, SW); }
            if (node.westNode != null) { AddDoorData(SW, NW); }
            if (node.eastNode != null) { AddDoorData(NE, SE); }
        } else {
            float off = 0;
            GetOffSet(NW, NE, minHallWidth, out off);
            Vector3 n1 = Vector3.Lerp(NW, NE, left - off);
            Vector3 n2 = Vector3.Lerp(NW, NE, right + off);

            GetOffSet(SW, SE, minHallWidth, out off);
            Vector3 s1 = Vector3.Lerp(SW, SE, left - off);
            Vector3 s2 = Vector3.Lerp(SW, SE, right + off);

            GetOffSet(NW, SW, minHallWidth, out off);
            Vector3 w1 = Vector3.Lerp(NW, SW, left - off);
            Vector3 w2 = Vector3.Lerp(NW, SW, right + off);

            GetOffSet(NE, SE, minHallWidth, out off);
            Vector3 e1 = Vector3.Lerp(NE, SE, left - off);
            Vector3 e2 = Vector3.Lerp(NE, SE, right + off);

            Vector3 msw = (Vector3.Lerp(n1, s1, right) + Vector3.Lerp(w2, e2, left)) / 2f;//Middle South West
            Vector3 mse = (Vector3.Lerp(n2, s2, right) + Vector3.Lerp(w2, e2, right)) / 2f;//Middle South East
            Vector3 mnw = (Vector3.Lerp(n1, s1, left) + Vector3.Lerp(w1, e1, left)) / 2f;//Middle North West
            Vector3 mne = (Vector3.Lerp(n2, s2, left) + Vector3.Lerp(w1, e1, right)) / 2f;//Middle North East

            parts.Add(new Part() {
                SW = msw,
                SE = mse,
                NW = mnw,
                NE = mne,
                hasEast = node.eastNode == null,
                hasNorth = node.northNode == null,
                hasWest = node.westNode == null,
                hasSouth = node.southNode == null,
                hasNorthEnt = node.northNode != null,
                hasSouthEnt = node.southNode != null,
                hasWestEnt = node.westNode != null,
                hasEastEnt = node.eastNode != null
            });

            if (node.northNode != null) {
                parts.Add(new Part() {
                    SW = mnw,
                    SE = mne,
                    NW = n1,
                    NE = n2,
                    hasEast = true,
                    hasNorth = true,
                    hasWest = true,
                    hasSouth = false,
                    hasNorthEnt = node.northNode != null,
                    hasSouthEnt = false,
                    hasWestEnt = false,
                    hasEastEnt = false
                });
                AddDoorData(n1, n2);
            }
            if (node.southNode != null) {
                parts.Add(new Part() {
                    SW = s1,
                    SE = s2,
                    NW = msw,
                    NE = mse,
                    hasEast = true,
                    hasNorth = false,
                    hasWest = true,
                    hasSouth = true,
                    hasNorthEnt = false,
                    hasSouthEnt = node.southNode != null,
                    hasWestEnt = false,
                    hasEastEnt = false
                });
                AddDoorData(s2, s1);
            }
            if (node.westNode != null) {
                parts.Add(new Part() {
                    SW = w2,
                    SE = msw,
                    NW = w1,
                    NE = mnw,
                    hasEast = false,
                    hasNorth = true,
                    hasWest = true,
                    hasSouth = true,
                    hasNorthEnt = false,
                    hasSouthEnt = false,
                    hasWestEnt = node.westNode != null,
                    hasEastEnt = false
                });
                AddDoorData(w2, w1);
            }
            if (node.eastNode != null) {
                parts.Add(new Part() {
                    SW = mse,
                    SE = e2,
                    NW = mne,
                    NE = e1,
                    hasEast = true,
                    hasNorth = true,
                    hasWest = false,
                    hasSouth = true,
                    hasNorthEnt = false,
                    hasSouthEnt = false,
                    hasWestEnt = false,
                    hasEastEnt = node.eastNode != null
                });
                AddDoorData(e1, e2);
            }
        }
    }

    void GetOffSet(Vector3 v1, Vector3 v2, float minDis, out float off) {
        off = 0;
        float dis = Vector3.Distance(v1, v2);
        if (dis * 0.33f < minDis) {
            float k = minDis - (dis * 0.33f);
            float j = k / dis;
            off = j * 0.5f;
        }
    }

    void AddDoorData(Vector3 v1, Vector3 v2) {
        Vector3 center = Vector3.Lerp(v1, v2, 0.5f);
        float angle = Quaternion.LookRotation(v2 - v1).eulerAngles.y;
        //float angle = Vector3.SignedAngle(v1, v2, Vector3.up);
        doorLocations.Add(center, angle);
    }
}

public struct Part {
    public Vector3 SW, SE, NW, NE;
    public bool hasNorth;
    public bool hasWest;
    public bool hasEast;
    public bool hasSouth;
    public bool hasNorthEnt;
    public bool hasWestEnt;
    public bool hasEastEnt;
    public bool hasSouthEnt;
}
