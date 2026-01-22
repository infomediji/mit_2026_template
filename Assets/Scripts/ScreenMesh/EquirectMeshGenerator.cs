using UnityEngine;

public class EquirectMeshGenerator : IScreenMeshGenerator
{
    private readonly int _segments;
    private readonly float _radius;

    public EquirectMeshGenerator(int segments = 64, float radius = 100f)
    {
        _segments = segments;
        _radius = radius;
    }

    public Mesh Generate()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralEquirect180";

        int segments = _segments;
        int rings = segments; // 1:1 ratio for 180x180 equirect

        int vertexCount = (segments + 1) * (rings + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];

        int index = 0;

        for (int ring = 0; ring <= rings; ring++)
        {
            // Vertical angle: 0 to PI (top to bottom)
            float phi = Mathf.PI * ring / rings;
            float sinPhi = Mathf.Sin(phi);
            float cosPhi = Mathf.Cos(phi);

            for (int seg = 0; seg <= segments; seg++)
            {
                // Horizontal angle: PI/2 to 3*PI/2 (180 degrees, front hemisphere)
                float theta = Mathf.PI * (0.5f + (float)seg / segments);
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                float x = sinPhi * cosTheta;
                float y = cosPhi;
                float z = sinPhi * sinTheta;

                vertices[index] = new Vector3(x, y, z) * _radius;
                normals[index] = new Vector3(-x, -y, -z);

                // Equirectangular UV mapping (linear)
                float u = (float)seg / segments;
                float v = 1f - (float)ring / rings;
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
