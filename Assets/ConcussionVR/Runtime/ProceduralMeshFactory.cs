using UnityEngine;
using UnityEngine.Rendering;

namespace ConcussionVR
{
    internal static class ProceduralMeshFactory
    {
        public static Mesh CreateLake(string name, float width, float depth, int xSegments, int zSegments)
        {
            int columns = xSegments + 1;
            var vertices = new Vector3[columns * (zSegments + 1)];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[xSegments * zSegments * 6];

            for (int z = 0; z <= zSegments; z++)
            {
                float v = z / (float)zSegments;
                for (int x = 0; x <= xSegments; x++)
                {
                    float u = x / (float)xSegments;
                    int index = z * columns + x;
                    vertices[index] = new Vector3((u - 0.5f) * width, 0f, v * depth);
                    normals[index] = Vector3.up;
                    uv[index] = new Vector2(u, v);
                }
            }

            FillGridTriangles(triangles, xSegments, zSegments, columns);
            return Build(name, vertices, normals, uv, triangles);
        }

        public static Mesh CreateMountainRange(string name, float width, float depth, int xSegments, int zSegments)
        {
            int columns = xSegments + 1;
            var vertices = new Vector3[columns * (zSegments + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[xSegments * zSegments * 6];

            for (int z = 0; z <= zSegments; z++)
            {
                float v = z / (float)zSegments;
                for (int x = 0; x <= xSegments; x++)
                {
                    float u = x / (float)xSegments;
                    float ridge = Mathf.Sin(u * Mathf.PI * 3.1f + 0.4f) * 1.05f;
                    float detail = Mathf.PerlinNoise(u * 5.4f + 2.1f, v * 2.7f + 4.8f) * 1.4f;
                    float rise = Mathf.Pow(v, 0.72f) * 5.8f;
                    int index = z * columns + x;
                    vertices[index] = new Vector3((u - 0.5f) * width, -1.8f + rise + ridge + detail, 10f + v * depth);
                    uv[index] = new Vector2(u, v);
                }
            }

            FillGridTriangles(triangles, xSegments, zSegments, columns);
            var mesh = Build(name, vertices, null, uv, triangles);
            mesh.RecalculateNormals();
            return mesh;
        }

        public static Mesh CreateCone(string name, int segments, float radius, float height)
        {
            var vertices = new Vector3[segments + 2];
            var triangles = new int[segments * 6];
            vertices[0] = new Vector3(0f, height, 0f);
            vertices[1] = Vector3.zero;
            for (int index = 0; index < segments; index++)
            {
                float angle = index * Mathf.PI * 2f / segments;
                vertices[index + 2] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                int next = (index + 1) % segments;
                int triangle = index * 6;
                triangles[triangle] = 0;
                triangles[triangle + 1] = index + 2;
                triangles[triangle + 2] = next + 2;
                triangles[triangle + 3] = 1;
                triangles[triangle + 4] = next + 2;
                triangles[triangle + 5] = index + 2;
            }

            var mesh = Build(name, vertices, null, null, triangles);
            mesh.RecalculateNormals();
            return mesh;
        }

        public static Mesh CreateTorus(string name, int majorSegments, int minorSegments, float majorRadius, float minorRadius)
        {
            int columns = minorSegments + 1;
            var vertices = new Vector3[(majorSegments + 1) * columns];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[majorSegments * minorSegments * 6];

            for (int major = 0; major <= majorSegments; major++)
            {
                float u = major / (float)majorSegments;
                float majorAngle = u * Mathf.PI * 2f;
                Vector3 center = new Vector3(Mathf.Cos(majorAngle) * majorRadius, Mathf.Sin(majorAngle) * majorRadius, 0f);
                for (int minor = 0; minor <= minorSegments; minor++)
                {
                    float v = minor / (float)minorSegments;
                    float minorAngle = v * Mathf.PI * 2f;
                    Vector3 normal = new Vector3(
                        Mathf.Cos(majorAngle) * Mathf.Cos(minorAngle),
                        Mathf.Sin(majorAngle) * Mathf.Cos(minorAngle),
                        Mathf.Sin(minorAngle));
                    int index = major * columns + minor;
                    vertices[index] = center + normal * minorRadius;
                    normals[index] = normal;
                    uv[index] = new Vector2(u, v);
                }
            }

            FillGridTriangles(triangles, minorSegments, majorSegments, columns);
            return Build(name, vertices, normals, uv, triangles);
        }

        private static void FillGridTriangles(int[] triangles, int xSegments, int zSegments, int columns)
        {
            int cursor = 0;
            for (int z = 0; z < zSegments; z++)
            {
                for (int x = 0; x < xSegments; x++)
                {
                    int current = z * columns + x;
                    triangles[cursor++] = current;
                    triangles[cursor++] = current + columns;
                    triangles[cursor++] = current + 1;
                    triangles[cursor++] = current + 1;
                    triangles[cursor++] = current + columns;
                    triangles[cursor++] = current + columns + 1;
                }
            }
        }

        private static Mesh Build(string name, Vector3[] vertices, Vector3[] normals, Vector2[] uv, int[] triangles)
        {
            var mesh = new Mesh { name = name, indexFormat = vertices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.vertices = vertices;
            if (normals != null) mesh.normals = normals;
            if (uv != null) mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
