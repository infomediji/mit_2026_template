using UnityEngine;

public class FlatMeshGenerator : IScreenMeshGenerator
{
    private readonly float _width;
    private readonly float _height;
    private readonly float _distance;

    public FlatMeshGenerator(float width = 4f, float height = 2.25f, float distance = 3f)
    {
        _width = width;
        _height = height;
        _distance = distance;
    }

    public Mesh Generate()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralFlat";

        float halfW = _width / 2f;
        float halfH = _height / 2f;

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-halfW, -halfH, _distance),
            new Vector3(halfW, -halfH, _distance),
            new Vector3(-halfW, halfH, _distance),
            new Vector3(halfW, halfH, _distance)
        };

        Vector3[] normals = new Vector3[4]
        {
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back
        };

        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(1, 0),
            new Vector2(0, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        int[] triangles = new int[6]
        {
            0, 1, 2,
            2, 1, 3
        };

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        return mesh;
    }
}
