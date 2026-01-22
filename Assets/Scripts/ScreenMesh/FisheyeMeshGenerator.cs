using UnityEngine;

public class FisheyeMeshGenerator : IScreenMeshGenerator
{
    private readonly int _segments;
    private readonly float _radius;
    private readonly float _coverage;

    public FisheyeMeshGenerator(int segments = 64, float radius = 100f, float coverage = 0.5f)
    {
        _segments = segments;
        _radius = radius;
        _coverage = Mathf.Clamp01(coverage);
    }

    public Mesh Generate()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralFisheye";

        int segments = _segments;
        int rings = Mathf.Max(1, (int)(segments * _coverage));

        int vertexCount = (segments + 1) * (rings + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];

        int index = 0;
        float maxPhi = Mathf.PI * _coverage;

        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = maxPhi * ring / rings;
            float sinPhi = Mathf.Sin(phi);
            float cosPhi = Mathf.Cos(phi);

            for (int seg = 0; seg <= segments; seg++)
            {
                float theta = 2f * Mathf.PI * seg / segments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                float x = sinPhi * cosTheta;
                float y = cosPhi;
                float z = sinPhi * sinTheta;

                vertices[index] = new Vector3(x, y, z) * _radius;
                normals[index] = new Vector3(-x, -y, -z);

                // Fisheye UV mapping (radial)
                float u = 0.5f + (sinPhi / (2f * Mathf.Sin(maxPhi))) * cosTheta;
                float v = 0.5f + (sinPhi / (2f * Mathf.Sin(maxPhi))) * sinTheta;
                uvs[index] = new Vector2(u, v);

                index++;
            }
        }

        int triangleCount = segments * rings * 6;
        int[] triangles = new int[triangleCount];
        int triIndex = 0;

        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                int current = ring * (segments + 1) + seg;
                int next = current + segments + 1;

                triangles[triIndex++] = current;
                triangles[triIndex++] = current + 1;
                triangles[triIndex++] = next;

                triangles[triIndex++] = next;
                triangles[triIndex++] = current + 1;
                triangles[triIndex++] = next + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        return mesh;
    }
}
