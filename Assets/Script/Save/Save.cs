using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public struct SerializableVector3Int
{
    public int x;
    public int y;
    public int z;
    
    public SerializableVector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public override string ToString()
    {
        return $"[{x}, {y}, {z}]";
    }
    
    public static implicit operator Vector3Int(SerializableVector3Int val)
    {
        return new Vector3Int(val.x, val.y, val.z);
    }

    public static implicit operator SerializableVector3Int(Vector3Int val)
    {
        return new SerializableVector3Int(val.x, val.y, val.z);
    }
}

[Serializable]
public class ChunkSaveData
{
    public Vector2Int MapSeed;
    public List<SerializableVector3Int> ModifiedChunks { get; }
    public Dictionary<SerializableVector3Int, Dictionary<SerializableVector3Int, BlockType>> ModifiedBlocks { get; }

    public ChunkSaveData(Vector2Int mapSeed)
    {
        MapSeed = mapSeed;
        ModifiedChunks = new List<SerializableVector3Int>();
        ModifiedBlocks = new Dictionary<SerializableVector3Int, Dictionary<SerializableVector3Int, BlockType>>();
    }
}

public static class Save
{
    public static ChunkSaveData SaveData;
    public static Queue<Block> BlocksToSave = new Queue<Block>();
    public static bool IsSaving = false;

    public static void InitSave(Vector2Int mapSeed)
    {
        SaveData = new ChunkSaveData(mapSeed);
        BlocksToSave.Clear();
    }
    
    public static void SaveWorld(int slot)
    {
        if (!Directory.Exists("Saves/Save" + slot))
            Directory.CreateDirectory("Saves/Save" + slot);
        string path = "Saves/Save" + slot + "/World";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream chunkListFile = new FileStream(path, FileMode.OpenOrCreate);
        formatter.Serialize(chunkListFile, SaveData);
        chunkListFile.Close();
    }
    
    public static void LoadWorld(int slot)
    {
        string path = "Saves/Save" + slot + "/World";
        if (!File.Exists(path))
        {
            Debug.Log("Save file does not exist. Creating new world...");
            return;
        }
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream chunkListFile = new FileStream(path, FileMode.OpenOrCreate);
        SaveData = (ChunkSaveData)formatter.Deserialize(chunkListFile);
        GameManager.World.MapSeed = SaveData.MapSeed;
        chunkListFile.Close();
    }

    public static void RunSaveLoop()
    {
        IsSaving = true;
        while (IsSaving)
        {
            if (BlocksToSave.Count > 0)
            {
                var block = BlocksToSave.Dequeue();
                if (!SaveData.ModifiedChunks.Contains(block.Chunk.WorldPosition))
                {
                    SaveData.ModifiedChunks.Add(block.Chunk.WorldPosition);
                    SaveData.ModifiedBlocks[block.Chunk.WorldPosition] = new Dictionary<SerializableVector3Int, BlockType>();
                }

                SaveData.ModifiedBlocks[block.Chunk.WorldPosition][block.GlobalPosition] = block.BlockType;
            }
            else
                System.Threading.Thread.Sleep(1000);
        }
    }

    public static bool[] CheckIfSavesExist()
    {
        bool[] saves = new bool[3];
        for (int slot = 0; slot < 3; slot++)
        {
            string path = "Saves/Save" + slot + "/World";
            if (File.Exists(path))
                saves[slot] = true;
            else
                saves[slot] = false;
        }

        return saves;
    }
}
