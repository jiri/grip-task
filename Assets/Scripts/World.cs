using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    public BlockPrototype[] blockPrototypes;
    public Material terrainMaterial;

    public static readonly int SizeInChunks = 100;
    public static readonly int ViewDistanceInChunks = 5;
    static int SizeInVoxels {
        get {
            return SizeInChunks * Chunk.Size;
        }
    }

    // TODO: Make the world infinite (and optionally unload far away chunks)
    Chunk[,] chunkSlice = new Chunk[SizeInChunks, SizeInChunks];
    List<Vector2Int> activeChunks = new List<Vector2Int>();

    public Vector3 spawnPosition;

    public Transform player;
    Vector2Int playerChunkPosition;
    Vector2Int playerLastChunkPosition;

    void Start() {
        this.spawnPosition = new Vector3(SizeInVoxels / 2.0f, Chunk.Height / 2.0f + 2.0f, SizeInVoxels / 2.0f);

        GenerateWorld();

        this.playerLastChunkPosition = ChunkPositionFromPosition(this.player.position);
    }

    void Update() {
        this.playerChunkPosition = ChunkPositionFromPosition(this.player.position);
        
        if (!this.playerChunkPosition.Equals(this.playerLastChunkPosition)) {
            CheckViewDistance();
        }

        this.playerLastChunkPosition = this.playerChunkPosition;
    }

    void GenerateWorld() {
        for (int x = SizeInChunks / 2 - ViewDistanceInChunks / 2; x < SizeInChunks / 2 + ViewDistanceInChunks / 2; x++) {
            for (int z = SizeInChunks / 2 - ViewDistanceInChunks / 2; z < SizeInChunks / 2 + ViewDistanceInChunks / 2; z++) {
                this.chunkSlice[x, z] = new Chunk(this, new Vector2Int(x, z));
                this.activeChunks.Add(new Vector2Int(x, z));
            }
        }

        this.player.position = this.spawnPosition;
    }

    bool IsChunkInWorld(Vector2Int chunkPosition) {
        return chunkPosition.x >= 0 && chunkPosition.x <= SizeInChunks - 1
            && chunkPosition.y >= 0 && chunkPosition.y <= SizeInChunks - 1;
    }

    Vector2Int ChunkPositionFromPosition(Vector3 position) {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / Chunk.Size),
            Mathf.FloorToInt(position.z / Chunk.Size)
        );
    }

    void CheckViewDistance() {
        Vector2Int chunkCoord = ChunkPositionFromPosition(player.position);
        List<Vector2Int> previouslyActiveChunks = new List<Vector2Int>(this.activeChunks);

        for (int x = chunkCoord.x - ViewDistanceInChunks / 2; x < chunkCoord.x + ViewDistanceInChunks / 2; x++) {
            for (int z = chunkCoord.y - ViewDistanceInChunks / 2; z < chunkCoord.y + ViewDistanceInChunks / 2; z++) {
                if (!IsChunkInWorld(new Vector2Int(x, z))) {
                    continue;
                }

                if (this.chunkSlice[x, z] == null) {
                    this.chunkSlice[x, z] = new Chunk(this, new Vector2Int(x, z));
                    this.activeChunks.Add(new Vector2Int(x, z));
                }
                else if (!this.chunkSlice[x, z].isActive) {
                    this.chunkSlice[x, z].isActive = true;
                    this.activeChunks.Add(new Vector2Int(x, z));
                }

                previouslyActiveChunks.RemoveAll(chunk => chunk.Equals(new Vector2Int(x, z)));
            }
        }

        foreach (Vector2Int position in previouslyActiveChunks) {
            this.chunkSlice[position.x, position.y].isActive = false;
        }
    }

    public byte GetVoxel(Vector3Int position) {
        if (position.x < 0 || position.x > SizeInVoxels - 1 ||
            position.y < 0 || position.y > Chunk.Height - 1 ||
            position.z < 0 || position.z > SizeInVoxels - 1) {
            return 0;
        }

        if (position.y > Chunk.Height / 2) {
            return 0;
        }
        else if (position.y == Chunk.Height / 2) {
            return 3;
        }
        else if (position.y > Chunk.Height / 2 - 3) {
            return 2;
        }
        else {
            return 1;
        }
    }
}
