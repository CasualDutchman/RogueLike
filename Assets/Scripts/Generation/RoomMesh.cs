using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomMesh : MonoBehaviour {

    public Material testMaterial;

    public bool gizmoPoints;
    public Vector3 _SW, _SE, _NW, _NE;
    public float _size;

    public bool northEntrance, southEntrance, westEntrance, eastEntrance;
    public bool north, south, west, east;

    public float wallHeight;
    public float doorHeight;
    public float doorWidth;

    public bool gizmoGrid;
    public bool gizmoShapeGrid;

    MeshFilter meshFilter;

    [ContextMenu("Clear")]
    public void Clear() {
        meshFilter.mesh = null;
    }

    [ContextMenu("Generate")]
    public void Generate() {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        meshFilter = GetComponent<MeshFilter>();

        Clear();
        GenerateMesh(_SW, _SE, _NW, _NE, new WallData(north, south, west, east).Entrances(northEntrance, southEntrance, westEntrance, eastEntrance), wallHeight, doorHeight, doorWidth, _size, verts, uvs, tris);

        Mesh mesh = new Mesh();
        mesh.name = "test";
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    int amount = 0;

    public static void GenerateMesh(Vector3 SW, Vector3 SE, Vector3 NW, Vector3 NE, WallData wall, float wallH, float doorH, float doorW, float size, List<Vector3> verts, List<Vector2> uvs, List<int> tris) {

        List<Vector3> inShape = new List<Vector3>();

        float minX = 0;
        float maxX = 0;
        float minY = 0;
        float maxY = 0;

        GeneratePoints(inShape, wall, SW, SE, NW, NE, size, out minX, out maxX, out minY, out maxY);

        if (wall.hasNorth)
            AddWall(NW, NE, wall.hasNorthEntrance, size, wallH, doorH, doorW, verts, tris, uvs);
        if(wall.hasWest)
            AddWall(SW, NW, wall.hasWestEntrance, size, wallH, doorH, doorW, verts, tris, uvs);
        if (wall.hasEast)
            AddWall(NE, SE, wall.hasEastEntrance, size, wallH, doorH, doorW, verts, tris, uvs);
        if(wall.hasSouth)
            AddWall(SE, SW, wall.hasSouthEntrance, size, wallH, doorH, doorW, verts, tris, uvs);

        for (int y = Mathf.FloorToInt(minY / size); y < Mathf.CeilToInt(maxY / size); y++) {
            for (int x = Mathf.FloorToInt(minX / size); x < Mathf.CeilToInt(maxX / size); x++) {
                GetQuad(inShape, size, x, y, verts, tris, uvs);
            }
        }
    }

    #region Spawning
    static void AddWall(Vector3 v1, Vector3 v2, bool entrance, float size, float wallH, float doorH, float doorW, List<Vector3> verts, List<int> tris, List<Vector2> uvs) {
        float dis = Vector3.Distance(v1, v2);
        float disScale = dis / size;
        int randomWall = 0;
        for (int k = 0; k < 2; k++) {//layers // doorlayer // layer above door
            for (int j = 0; j < 2; j++) {// from middle to right  // from middle to left
                for (int i = 0; i < Mathf.CeilToInt(disScale / 2); i++) { // amount of stops on the line
                    float point = entrance && i == 0 && k == 0 ? (((doorW / Vector3.Distance(v1, v2)) * 0.5f) * (j == 0 ? 1 : -1)) : 0;

                    Vector3 first = Vector3.Lerp(v1, v2, 0.5f + point + ((1 / disScale) * (float)i) * (j == 0 ? 1 : -1));
                    Vector3 second = Vector3.Lerp(v1, v2, 0.5f + ((1 / disScale) * (float)(i + 1)) * (j == 0 ? 1 : -1));

                    verts.Add(first + Vector3.up * (k == 0 ? 0 : doorH));
                    verts.Add(second + Vector3.up * (k == 0 ? 0 : doorH));
                    verts.Add(first + Vector3.up * (k == 0 ? doorH : wallH));
                    verts.Add(first + Vector3.up * (k == 0 ? doorH : wallH));
                    verts.Add(second + Vector3.up * (k == 0 ? 0 : doorH));
                    verts.Add(second + Vector3.up * (k == 0 ? doorH : wallH));

                    int tricount = tris.Count;
                    tris.Add(tricount + 0);
                    tris.Add(tricount + (j == 0 ? 2 : 1));
                    tris.Add(tricount + (j == 0 ? 1 : 2));
                    tris.Add(tricount + 3);
                    tris.Add(tricount + (j == 0 ? 5 : 4));
                    tris.Add(tricount + (j == 0 ? 4 : 5));

                    //UV offset
                    float f = 0.0005f;
                    float originLeft = 0.25f * randomWall;
                    //B= Bottom // L= Left // R= Right // U= Upper
                    float B = 0 + f;
                    float L = originLeft + f;
                    float R = originLeft + 0.25F - f;
                    float U = k == 0 ? 0.375f - f : (0.375f * ((wallH - doorH) / doorH)) - f;//change top of upper layer, because of difference

                    if (j == 0) {
                        if (k == 0 && i == 0 && entrance)
                            L = originLeft + (0.25f * ((doorW * 0.5f) / size)) + f;//change the left side of the first point to match the uv for entrance

                        uvs.Add(new Vector2(L, B));
                        uvs.Add(new Vector2(R, B));
                        uvs.Add(new Vector2(L, U));
                        uvs.Add(new Vector2(L, U));
                        uvs.Add(new Vector2(R, B));
                        uvs.Add(new Vector2(R, U));
                    } else {
                        if (k == 0 && i == 0 && entrance)
                            R = originLeft + (0.25f - (0.25f * ((doorW * 0.5f) / size))) + f;//change the right side of the first point to match the uv for entrance

                        uvs.Add(new Vector2(R, B));
                        uvs.Add(new Vector2(L, B));
                        uvs.Add(new Vector2(R, U));
                        uvs.Add(new Vector2(R, U));
                        uvs.Add(new Vector2(L, B));
                        uvs.Add(new Vector2(L, U));
                    }

                    randomWall = Random.Range(0, 3);
                }
            }
        }
    }

    static void GetQuad(List<Vector3> inShape, float size, int x, int y, List<Vector3> verts, List<int> tris, List<Vector2> uvs) {
        Vector3[] v3Array = GetSquareData(inShape, size, x, y);

        int max = v3Array.Length - 2;
        for (int i = 0; i < max; i++) {
            MeshData triData = GetTriangle(v3Array[0], v3Array[i + 2], v3Array[i + 1], size, x * size, y * size);
            
            foreach (Vector3 item in triData.vertices) {
                verts.Add(item);
            }
            int triCount = tris.Count;
            foreach (int item in triData.triangles) {
                tris.Add(triCount + item);
            }
            foreach (Vector2 item in triData.uvs) {
                uvs.Add(item);
            }
        }
    }

    static MeshData GetTriangle(Vector3 p1, Vector3 p2, Vector3 p3, float size, float originX, float originY) {
        int[] iArr = new int[3];

        float dot = Vector3.Cross(p2 - p1, p3 - p1).y;
        if (dot > 0) {
            iArr[0] = 0;
            iArr[1] = 1;
            iArr[2] = 2;
        } else {
            iArr[0] = 0;
            iArr[1] = 2;
            iArr[2] = 1;
        }

        float f = 0.0005f;
        int uv = Mathf.FloorToInt(Mathf.PerlinNoise(originX / 6.5f, originY / 6.5f) * 4f);
        float offset = uv * 0.25f;

        return new MeshData(new Vector3[] { p1, p2, p3 }, iArr, new Vector2[] {
                new Vector2(offset + f + ((p1.x - originX) * (0.25f - f)) / size, 0.75f + f + ((p1.z - originY) * (0.25f - f)) / size),
                new Vector2(offset + f + ((p2.x - originX) * (0.25f - f)) / size, 0.75f + f + ((p2.z - originY) * (0.25f - f)) / size),
                new Vector2(offset + f + ((p3.x - originX) * (0.25f - f)) / size, 0.75f + f + ((p3.z - originY) * (0.25f - f)) / size)
        });
    }

    static Vector3[] GetSquareData(List<Vector3> inShape, float size, int x, int y) {
        List<Vector3> list = new List<Vector3>();

        Vector3 average = Vector3.zero;
        foreach (Vector3 v3 in inShape) {
            if (v3.x >= x * size && v3.x <= (x * size) + size) {
            if (v3.z >= y * size && v3.z <= (y * size) + size) {
                    list.Add(v3);
                    average += v3;
                }
            }
        }
        average /= list.Count;

        list.Sort((a, b) => new ClockwiseComparer(average).Compare(a, b));

        return list.ToArray();
    }
    #endregion

    #region generation
    static  void GeneratePoints(List<Vector3> inShape, WallData wall, Vector3 SW, Vector3 SE, Vector3 NW, Vector3 NE, float s, out float minX, out float maxX, out float minY, out float maxY) {
        inShape.Add(SW);
        inShape.Add(SE);
        inShape.Add(NW);
        inShape.Add(NE);

        minX = SW.x;
        maxX = SW.x;
        minY = SW.z;
        maxY = SW.z;

        if (SE.x < minX) minX = SE.x;
        if (NW.x < minX) minX = NW.x;
        if (NE.x < minX) minX = NE.x;

        if (SE.x > maxX) maxX = SE.x;
        if (NW.x > maxX) maxX = NW.x;
        if (NE.x > maxX) maxX = NE.x;

        if (SE.z < minY) minY = SE.z;
        if (NW.z < minY) minY = NW.z;
        if (NE.z < minY) minY = NE.z;

        if (SE.z > maxY) maxY = SE.z;
        if (NW.z > maxY) maxY = NW.z;
        if (NE.z > maxY) maxY = NE.z;

        for (int y = Mathf.FloorToInt(minY / s); y < Mathf.CeilToInt(maxY / s); y++) {
            for (int x = Mathf.FloorToInt(minX / s); x < Mathf.CeilToInt(maxX / s); x++) {
                if (PointInTriangle(new Vector2(x * s, y * s), new Vector2(SW.x, SW.z), new Vector2(NW.x, NW.z), new Vector2(NE.x, NE.z))) {
                    inShape.Add(new Vector3(x * s, 0, y * s));
                } else if (PointInTriangle(new Vector2(x * s, y * s), new Vector2(SW.x, SW.z), new Vector2(SE.x, SE.z), new Vector2(NE.x, NE.z))) {
                    inShape.Add(new Vector3(x * s, 0, y * s));
                }
            }
        }

        for (int i = Mathf.FloorToInt(minY / s); i < Mathf.CeilToInt(maxY / s); i++) {
            AddPoint(inShape, NW, SW, Mathf.FloorToInt(minX / s), Mathf.CeilToInt(maxX / s), i * s, i * s);
            AddPoint(inShape, NE, SE, Mathf.FloorToInt(minX / s), Mathf.CeilToInt(maxX / s), i * s, i * s);
            AddPoint(inShape, SE, SW, Mathf.FloorToInt(minX / s), Mathf.CeilToInt(maxX / s), i * s, i * s);
            AddPoint(inShape, NW, NE, Mathf.FloorToInt(minX / s), Mathf.CeilToInt(maxX / s), i * s, i * s);
        }

        for (int i = Mathf.FloorToInt(minX / s); i < Mathf.CeilToInt(maxX / s); i++) {
            AddPoint(inShape, NW, SW, i * s, i * s, Mathf.FloorToInt(minY / s), Mathf.CeilToInt(maxY / s));
            AddPoint(inShape, NE, SE, i * s, i * s, Mathf.FloorToInt(minY / s), Mathf.CeilToInt(maxY / s));
            AddPoint(inShape, SE, SW, i * s, i * s, Mathf.FloorToInt(minY / s), Mathf.CeilToInt(maxY / s));
            AddPoint(inShape, NW, NE, i * s, i * s, Mathf.FloorToInt(minY / s), Mathf.CeilToInt(maxY / s));
        }
    }

    static void AddPoint(List<Vector3> inShape, Vector3 l1, Vector3 l2, float minX, float maxX, float minY, float maxY) {
        Vector2 vec2 = GetPoint(l1, l2, minX, maxX, minY, maxY);

        float _minX = l1.x;
        float _maxX = l1.x;
        float _minY = l1.z;
        float _maxY = l1.z;

        if (l2.x < _minX) _minX = l2.x;
        if (l2.x > _maxX) _maxX = l2.x;
        if (l2.z < _minY) _minY = l2.z;
        if (l2.z > _maxY) _maxY = l2.z;

        if (!inShape.Contains(new Vector3(vec2.x, 0, vec2.y))) {
            if (vec2.x > _minX && vec2.x < _maxX && vec2.y > _minY && vec2.y < _maxY) {
                inShape.Add(new Vector3(vec2.x, 0, vec2.y));
            }
        }    
    }

    //used http://www.habrador.com/tutorials/math/5-line-line-intersection/
    static Vector2 GetPoint(Vector3 l1, Vector3 l2, float minX, float maxX, float minY, float maxY) {
        Vector2 l1_start = new Vector2(l1.x, l1.z);
        Vector2 l1_end = new Vector2(l2.x, l2.z);

        Vector2 l2_start = new Vector2(minX, minY);
        Vector2 l2_end = new Vector2(maxX, maxY);

        //Direction of the lines
        Vector2 l1_dir = (l1_end - l1_start).normalized;
        Vector2 l2_dir = (l2_end - l2_start).normalized;

        //If we know the direction we can get the normal vector to each line
        Vector2 l1_normal = new Vector2(-l1_dir.y, l1_dir.x);
        Vector2 l2_normal = new Vector2(-l2_dir.y, l2_dir.x);

        //The normal vector is the A, B
        float A = l1_normal.x;
        float B = l1_normal.y;

        float C = l2_normal.x;
        float D = l2_normal.y;

        //To get k we just use one point on the line
        float k1 = (A * l1_start.x) + (B * l1_start.y);
        float k2 = (C * l2_start.x) + (D * l2_start.y);

        float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
        float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

        return new Vector2(x_intersect, y_intersect);
    }

    static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3) {
        bool b1, b2, b3;

        b1 = Sign(pt, v1, v2) < 0.0f;
        b2 = Sign(pt, v2, v3) < 0.0f;
        b3 = Sign(pt, v3, v1) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
    #endregion

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        if (gizmoPoints) {
            Gizmos.DrawWireSphere(_SW, 0.1f);
            Gizmos.DrawWireSphere(_SE, 0.1f);
            Gizmos.DrawWireSphere(_NW, 0.1f);
            Gizmos.DrawWireSphere(_NE, 0.1f);

            Gizmos.DrawLine(_SW, _SE);
            Gizmos.DrawLine(_SE, _NE);
            Gizmos.DrawLine(_NE, _NW);
            Gizmos.DrawLine(_NW, _SW);
        }

        if (gizmoGrid) {
            for (int y = 0; y <= 10; y++) {
                for (int x = 0; x <= 10; x++) {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(new Vector3(x * _size, 0, y * _size), 0.06f);
                }
            }
        }
    }
}

public class WallData {
    public bool hasNorth;
    public bool hasWest;
    public bool hasEast;
    public bool hasSouth;
    public bool hasNorthEntrance;
    public bool hasSouthEntrance;
    public bool hasWestEntrance;
    public bool hasEastEntrance;

    public WallData(bool n, bool s, bool w, bool e) {
        hasNorth = n;
        hasWest = w;
        hasEast = e;
        hasSouth = s;
    }

    public WallData Entrances(bool n, bool s, bool w, bool e) {
        hasNorthEntrance = n;
        hasWestEntrance = w;
        hasEastEntrance = e;
        hasSouthEntrance = s;
        return this;
    }
}

public struct MeshData {
    public Vector3[] vertices;
    public Vector2[] uvs;
    public int[] triangles;

    public MeshData(Vector3[] v3, int[] t, Vector2[] v2) {
        vertices = v3;
        uvs = v2;
        triangles = t;
    }
}

//from: https://pastebin.com/1RkaP28U
//from: https://answers.unity.com/questions/877169/vector2-array-sort-clockwise.html
//
public class ClockwiseComparer : IComparer<Vector3> {
    private Vector3 m_Origin;

    //public Vector3 origin { get { return m_Origin; } set { m_Origin = value; } }

    public ClockwiseComparer(Vector3 origin) {
        m_Origin = origin;
    }

    public int Compare(Vector3 first, Vector3 second) {
        return IsClockwise(first, second, m_Origin);
    }

    public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin) {
        if (first == second)
            return 0;

        Vector3 firstOffset = first - origin;
        Vector3 secondOffset = second - origin;

        float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.z);
        float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.z);

        if (angle1 < angle2)
            return -1;

        if (angle1 > angle2)
            return 1;

        return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? -1 : 1;
    }
}