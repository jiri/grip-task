using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {
    public static readonly int Size = 16;
    public static readonly int Height = 32;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    byte[,,] data = new byte[Chunk.Size, Chunk.Height, Chunk.Size];

    int currentVertex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public World world;

    void Start() {
        this.meshFilter = GetComponent<MeshFilter>();
        this.meshRenderer = GetComponent<MeshRenderer>();

        this.world = GameObject.Find("World").GetComponent<World>();

        PopulateMap();
        GenerateMesh();
    }

    void PopulateMap() {
        for (int x = 0; x < Chunk.Size; x++) {
            for (int y = 0; y < Chunk.Height; y++) {
                for (int z = 0; z < Chunk.Size; z++) {
                    if (y > Chunk.Height / 2) {
                        this.data[x, y, z] = 0;
                    }
                    else if (y == Chunk.Height / 2) {
                        this.data[x, y, z] = 3;
                    }
                    else if (y > Chunk.Height / 2 - 3) {
                        this.data[x, y, z] = 2;
                    }
                    else {
                        this.data[x, y, z] = 1;
                    }
                }
            }
        }
    }

    byte GetBlock(Vector3Int p) {
        if (p.x < 0 || p.x > Chunk.Size - 1 ||
            p.y < 0 || p.y > Chunk.Height - 1 ||
            p.z < 0 || p.z > Chunk.Size - 1) {
            return 0;
        }
        return this.data[p.x, p.y, p.z];
    }

    void GenerateMesh() {
        for (int x = 0; x < Chunk.Size; x++) {
            for (int y = 0; y < Chunk.Height; y++) {
                for (int z = 0; z < Chunk.Size; z++) {
                    AddBlockGeometry(new Vector3Int(x, y, z));
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = this.vertices.ToArray();
        mesh.triangles = this.triangles.ToArray();
        mesh.uv = this.uvs.ToArray();
        mesh.RecalculateNormals();

        this.meshFilter.mesh = mesh;
    }

    void AddBlockGeometry(Vector3Int p) {
        byte block = GetBlock(p);
        if (block == 0) {
            return;
        }

        foreach (Geometry.Face face in System.Enum.GetValues(typeof(Geometry.Face))) {
            byte neighbour = this.GetBlock(p + face.Offset());
            if (!world.blockPrototypes[neighbour].isSolid) {
                for (int i = 0; i < 6; i++) {
                    int index = Geometry.triangles[(int)face, i];
                    vertices.Add(Geometry.vertices[index] + p);
                    triangles.Add(currentVertex);
                    AddUV(world.blockPrototypes[block].GetTextureID(face), Geometry.uvs[i]);
                    currentVertex++;
                }
            }
        }
    }

    void AddUV(int textureID, Vector2 uv) {
        int y = textureID / 16;
        int x = textureID % 16;
        float unit = 16.0f / 256.0f;

        uvs.Add(new Vector2((x + uv.x) * unit, 1.0f - (y - uv.y + 1) * unit));
    }
}
