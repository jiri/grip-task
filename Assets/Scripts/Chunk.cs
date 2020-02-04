using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {
    public static readonly int Size = 16;
    public static readonly int Height = 32;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    Block[,,] data = new Block[Chunk.Size, Chunk.Height, Chunk.Size];

    int currentVertex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();


    void Start() {
        this.meshFilter = GetComponent<MeshFilter>();
        this.meshRenderer = GetComponent<MeshRenderer>();

        PopulateMap();
        GenerateMesh();
    }

    void PopulateMap() {
        for (int x = 0; x < Chunk.Size; x++) {
            for (int y = 0; y < Chunk.Height; y++) {
                for (int z = 0; z < Chunk.Size; z++) {
                    if (y > Chunk.Height / 2) {
                        this.data[x, y, z] = Block.Air;
                    }
                    else if (y > Chunk.Height / 2 - 6) {
                        this.data[x, y, z] = Block.Dirt;
                    }
                    else {
                        this.data[x, y, z] = Block.Stone;
                    }
                }
            }
        }
    }

    Block GetBlock(Vector3Int p) {
        if (p.x < 0 || p.x > Chunk.Size - 1 ||
            p.y < 0 || p.y > Chunk.Height - 1 ||
            p.z < 0 || p.z > Chunk.Size - 1) {
            return Block.Air;
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
        if (GetBlock(p) == Block.Air) {
            return;
        }

        foreach (Geometry.Face face in System.Enum.GetValues(typeof(Geometry.Face))) {
            Block neighbour = this.GetBlock(p + face.Offset());
            if (neighbour.IsTransparent()) {
                for (int i = 0; i < 6; i++) {
                    int index = Geometry.triangles[(int)face, i];
                    vertices.Add(Geometry.vertices[index] + p);
                    triangles.Add(currentVertex);
                    uvs.Add(Geometry.uvs[i] * 0.0625f + new Vector2(0.0f, 0.875f));
                    currentVertex++;
                }
            }
        }
    }
}
