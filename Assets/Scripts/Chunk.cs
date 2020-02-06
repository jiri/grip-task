using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public static readonly int Size = 16;
    public static readonly int Height = 32;

    GameObject gameObject;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    byte[,,] data = new byte[Chunk.Size, Chunk.Height, Chunk.Size];

    int currentVertex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public World world;

    public Vector2Int chunkPosition;

    public bool isActive {
        get {
            return this.gameObject.activeSelf;
        }
        set {
            this.gameObject.SetActive(value);
        }
    }

    public Vector3Int position {
        get {
            return new Vector3Int(
                Mathf.FloorToInt(this.gameObject.transform.position.x),
                Mathf.FloorToInt(this.gameObject.transform.position.y),
                Mathf.FloorToInt(this.gameObject.transform.position.z)
            );
        }
    }

    public Chunk(World world, Vector2Int chunkPosition) {
        this.world = world;
        this.chunkPosition = chunkPosition;

        this.gameObject = new GameObject();
        this.gameObject.transform.SetParent(this.world.transform);
        this.gameObject.transform.position = new Vector3(this.chunkPosition.x * Chunk.Size, 0.0f, this.chunkPosition.y * Chunk.Size);
        this.gameObject.name = $"Chunk {chunkPosition}";

        this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
        this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        this.meshRenderer.material = this.world.atlas.material;

        PopulateMap();
        GenerateMesh();
    }

    void PopulateMap() {
        for (int x = 0; x < Chunk.Size; x++) {
            for (int y = 0; y < Chunk.Height; y++) {
                for (int z = 0; z < Chunk.Size; z++) {
                    this.data[x, y, z] = this.world.GetVoxel(new Vector3Int(x, y, z) + this.position);
                }
            }
        }
    }

    bool IsVoxelInChunk(Vector3Int p) {
        return !(p.x < 0 || p.x > Chunk.Size - 1
              || p.y < 0 || p.y > Chunk.Height - 1
              || p.z < 0 || p.z > Chunk.Size - 1);
    }

    byte GetBlock(Vector3Int position) {
        return this.data[position.x, position.y, position.z];
    }

    bool CheckVoxel(Vector3Int position) {
        if (!IsVoxelInChunk(position)) {
            return world.atlas.prototypes[world.GetVoxel(position + this.position)].isSolid;
        }
        return world.atlas.prototypes[this.data[position.x, position.y, position.z]].isSolid;
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
            if (!CheckVoxel(p + face.Offset())) {
                //byte block = GetBlock(p);

                for (int i = 0; i < 6; i++) {
                    int index = Geometry.triangles[(int)face, i];
                    vertices.Add(Geometry.vertices[index] + p);
                    triangles.Add(currentVertex);
                    AddUV(this.world.atlas.prototypes[block].GetTextureID(face), Geometry.uvs[i]);
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
