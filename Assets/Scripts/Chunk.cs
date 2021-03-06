﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public static readonly int Size = 16;
    public static readonly int Height = 32;

    public GameObject gameObject;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public byte[,,] data = new byte[Chunk.Size, Chunk.Height, Chunk.Size];

    int currentVertex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public World world;
    public Vector2Int chunkPosition;

    public bool IsLoaded { get; private set; } = false;
    public bool IsVoxelMapPopulated { get; private set; } = false;

    private bool _isActive = true;

    public bool isActive {
        get {
            return this._isActive;
        }
        set {
            _isActive = value;
            if (this.gameObject) {
                this.gameObject.SetActive(this._isActive);
            }
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

    public Chunk(World world, Vector2Int chunkPosition, ChunkData chunkData) : this(world, chunkPosition, true) {
        this.data = chunkData.data;
        this.IsVoxelMapPopulated = true;
    }

    public Chunk(World world, Vector2Int chunkPosition, bool delayedLoad = false) {
        this.world = world;
        this.chunkPosition = chunkPosition;

        if (!delayedLoad) {
            this.Load();
        }
    }

    public void Load() {
        this.gameObject = new GameObject();
        this.gameObject.SetActive(this.isActive);
        this.gameObject.transform.SetParent(this.world.transform);
        this.gameObject.transform.position = new Vector3(this.chunkPosition.x * Chunk.Size, 0.0f, this.chunkPosition.y * Chunk.Size);
        this.gameObject.name = $"Chunk {chunkPosition}";

        this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
        this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        this.meshRenderer.material = this.world.atlas.material;

        if (!this.IsVoxelMapPopulated) {
            PopulateMap();
        }
        GenerateMesh();

        this.IsLoaded = true;
    }

    void PopulateMap() {
        for (int x = 0; x < Chunk.Size; x++) {
            for (int y = 0; y < Chunk.Height; y++) {
                for (int z = 0; z < Chunk.Size; z++) {
                    this.data[x, y, z] = this.world.GetVoxel(new Vector3Int(x, y, z) + this.position);
                }
            }
        }

        this.IsVoxelMapPopulated = true;
    }

    bool IsVoxelInChunk(Vector3Int p) {
        return !(p.x < 0 || p.x > Chunk.Size - 1
              || p.y < 0 || p.y > Chunk.Height - 1
              || p.z < 0 || p.z > Chunk.Size - 1);
    }

    byte GetBlock(Vector3Int pos) {
        return this.data[pos.x, pos.y, pos.z];
    }

    bool CheckVoxel(Vector3Int pos) {
        if (!IsVoxelInChunk(pos)) {
            return this.world.CheckVoxel(this.position + pos);
        }
        return world.atlas.prototypes[this.data[pos.x, pos.y, pos.z]].isSolid;
    }

    void GenerateMesh() {
        ClearMesh();

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

    void ClearMesh() {
        this.currentVertex = 0;
        this.vertices.Clear();
        this.triangles.Clear();
        this.uvs.Clear();
    }

    void AddBlockGeometry(Vector3Int p) {
        byte block = GetBlock(p);
        if (block == 0) {
            return;
        }

        foreach (Geometry.Face face in System.Enum.GetValues(typeof(Geometry.Face))) {
            if (CheckVoxel(p + face.Offset())) {
                continue;
            }

            for (int i = 0; i < 6; i++) {
                int index = Geometry.triangles[(int)face, i];
                vertices.Add(Geometry.vertices[index] + p);
                triangles.Add(currentVertex);
                AddUV(this.world.atlas.prototypes[block].GetTextureID(face), Geometry.uvs[i]);
                currentVertex++;
            }
        }
    }

    void AddUV(int textureID, Vector2 uv) {
        int y = textureID / 16;
        int x = textureID % 16;
        float unit = 16.0f / 256.0f;

        uvs.Add(new Vector2((x + uv.x) * unit, 1.0f - (y - uv.y + 1) * unit));
    }

    public Vector3Int LocalPositionFromGlobal(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        x -= Mathf.FloorToInt(this.position.x);
        z -= Mathf.FloorToInt(this.position.z);

        return new Vector3Int(x, y, z);
    }

    public void EditVoxel(Vector3Int pos, byte newBlock) {
        Vector3Int localPos = LocalPositionFromGlobal(pos);

        this.data[localPos.x, localPos.y, localPos.z] = newBlock;

        UpdateNeighbours(localPos);
        GenerateMesh();
    }

    void UpdateNeighbours(Vector3Int pos) {
        foreach (Geometry.Face face in System.Enum.GetValues(typeof(Geometry.Face))) {
            if (!IsVoxelInChunk(pos + face.Offset())) {
                this.world.ChunkFromPosition(this.position + pos + face.Offset()).GenerateMesh();
            }
        }
    }
}
