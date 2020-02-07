using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    public BlockAtlas atlas;

    public static readonly int ViewDistanceInChunks = 5;

    Dictionary<Vector2Int, Chunk> chunkSlice = new Dictionary<Vector2Int, Chunk>();
    List<Vector2Int> activeChunks = new List<Vector2Int>();

    Queue<Vector2Int> generationQueue = new Queue<Vector2Int>();
    bool isGeneratingChunks = false;

    public Vector3 spawnPosition;

    public Transform player;
    Vector2Int playerChunkPosition;
    Vector2Int playerLastChunkPosition;

    public int seed;

    void Start() {
        this.spawnPosition = new Vector3(1.0f, Chunk.Height, 1.0f);

        Random.InitState(this.seed);
        GenerateWorld();

        this.playerLastChunkPosition = ChunkPositionFromPosition(this.player.position);
    }

    void Update() {
        this.playerChunkPosition = ChunkPositionFromPosition(this.player.position);
        
        if (!this.playerChunkPosition.Equals(this.playerLastChunkPosition)) {
            CheckViewDistance();
        }

        if (generationQueue.Count > 0 && !this.isGeneratingChunks)
            StartCoroutine("GenerateChunks");

        this.playerLastChunkPosition = this.playerChunkPosition;
    }

    IEnumerator GenerateChunks() {
        this.isGeneratingChunks = true;

        while (generationQueue.Count > 0) {
            Vector2Int chunkPosition = generationQueue.Dequeue();
            this.chunkSlice[chunkPosition].Load();
            yield return null;
        }

        this.isGeneratingChunks = false;
    }

    void GenerateWorld() {
        for (int x = -ViewDistanceInChunks / 2; x < ViewDistanceInChunks / 2; x++) {
            for (int z = -ViewDistanceInChunks / 2; z < ViewDistanceInChunks / 2; z++) {
                this.chunkSlice[new Vector2Int(x, z)] = new Chunk(this, new Vector2Int(x, z));
                this.activeChunks.Add(new Vector2Int(x, z));
            }
        }

        this.player.position = this.spawnPosition;
    }

    bool IsVoxelInWorld(Vector3 pos) {
        return pos.y >= 0 && pos.y < Chunk.Height;
    }

    Vector2Int ChunkPositionFromPosition(Vector3 position) {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / Chunk.Size),
            Mathf.FloorToInt(position.z / Chunk.Size)
        );
    }

    public Chunk ChunkFromPosition(Vector3 pos) {
        Vector2Int chunkPosition = ChunkPositionFromPosition(pos);
        return this.chunkSlice[chunkPosition];
    }

    void CheckViewDistance() {
        Vector2Int chunkCoord = ChunkPositionFromPosition(player.position);
        List<Vector2Int> previouslyActiveChunks = new List<Vector2Int>(this.activeChunks);

        for (int x = chunkCoord.x - ViewDistanceInChunks / 2; x < chunkCoord.x + ViewDistanceInChunks / 2; x++) {
            for (int z = chunkCoord.y - ViewDistanceInChunks / 2; z < chunkCoord.y + ViewDistanceInChunks / 2; z++) {
                if (!this.chunkSlice.ContainsKey(new Vector2Int(x, z))) {
                    this.chunkSlice[new Vector2Int(x, z)] = new Chunk(this, new Vector2Int(x, z), true);
                    this.generationQueue.Enqueue(new Vector2Int(x, z));
                }
                else if (!this.chunkSlice[new Vector2Int(x, z)].isActive) {
                    this.chunkSlice[new Vector2Int(x, z)].isActive = true;
                }

                this.activeChunks.Add(new Vector2Int(x, z));
                previouslyActiveChunks.RemoveAll(chunk => chunk.Equals(new Vector2Int(x, z)));
            }
        }

        foreach (Vector2Int position in previouslyActiveChunks) {
            this.chunkSlice[position].isActive = false;
        }
    }

    public byte GetVoxel(Vector3Int position) {
        if (position.y < 0 || position.y > Chunk.Height - 1) {
            return 0;
        }

        int height = Mathf.FloorToInt(Chunk.Height * 0.5f * Noise.Get2DPerlin(new Vector2(position.x, position.z), 0.0f, 0.25f));

        if (position.y > height) {
            return 0;
        }
        else if (position.y == height) {
            return 3;
        }
        else if (position.y > height - 3) {
            return 2;
        }
        else {
            return 1;
        }
    }

    public bool CheckVoxel(Vector3 pos) {
        if (!IsVoxelInWorld(pos)) {
            return false;
        }

        Vector2Int chunkPosition = ChunkPositionFromPosition(pos);

        if (this.chunkSlice.ContainsKey(chunkPosition) && this.chunkSlice[chunkPosition].IsLoaded) {
            Chunk chunk = this.chunkSlice[chunkPosition];
            Vector3Int localPosition = chunk.LocalPositionFromGlobal(pos);
            byte block = chunk.data[localPosition.x, localPosition.y, localPosition.z];
            return this.atlas.prototypes[block].isSolid;
        }
        else {
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);
            byte block = GetVoxel(new Vector3Int(x, y, z));
            return this.atlas.prototypes[block].isSolid;
        }
    }
}
