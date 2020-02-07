using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ChunkData {
    public byte[,,] data;

    public ChunkData(Chunk chunk) {
        this.data = chunk.data;
    }
}

[System.Serializable]
public class PlayerData {
    public float[] position = new float[3];
    public float[] orientation = new float[3];
    public int selectedBlock;

    public PlayerData(FPSController player) {
        this.position[0] = player.transform.position.x;
        this.position[1] = player.transform.position.y;
        this.position[2] = player.transform.position.z;

        this.orientation[0] = player.transform.rotation.x;
        this.orientation[1] = player.transform.rotation.y;
        this.orientation[2] = player.transform.rotation.z;

        this.selectedBlock = player.selectedBlock;
    }
}

[System.Serializable]
public class WorldData {
    public List<int[]> positions = new List<int[]>();
    public List<ChunkData> chunks = new List<ChunkData>();
    public PlayerData playerData;
    public int seed;

    public WorldData(World world) {
        foreach (var pair in world.chunkSlice) {
            this.positions.Add(new int[] { pair.Key.x, pair.Key.y });
            this.chunks.Add(new ChunkData(pair.Value));
        }

        this.playerData = new PlayerData(world.player.GetComponent<FPSController>());

        this.seed = world.seed;
    }
}

[RequireComponent(typeof(World))]
public class WorldIO : MonoBehaviour {
    public Button saveButton;
    public Button loadButton;

    private World world;
    private string savePath;

    void Start() {
        this.world = GetComponent<World>();

        this.savePath = Application.persistentDataPath + "/save.dat";

        this.saveButton.onClick.AddListener(SaveWorld);
        this.loadButton.onClick.AddListener(LoadWorld);
    }

    public bool SaveFileExists() {
        return File.Exists(savePath);
    }

    void SaveWorld() {
        Debug.Log($"Saving game to '{savePath}'...");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        WorldData data = new WorldData(this.world);
        formatter.Serialize(stream, data);

        stream.Close();
    }

    void LoadWorld() {
        if (!SaveFileExists()) {
            Debug.LogError($"Save file '{savePath}' not found");
            return;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Open);

        WorldData data = formatter.Deserialize(stream) as WorldData;

        stream.Close();

        this.world.LoadFromData(data);
    }
}
