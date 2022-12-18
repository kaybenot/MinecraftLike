using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Chunk
{
    public BlockType[,,] Blocks { get; }
    public int Size { get; }
    public int Height { get; }
    public World World => GameManager.World;
    public Vector3Int WorldPosition { get; }
    public ChunkRenderer ChunkRenderer { get; private set; }
    public ChunkGenerator ChunkGenerator { get; }
    public ChunkMesh ChunkMesh { get; private set; }
    
    public static Dictionary<Vector3Int, Chunk> Chunks = new Dictionary<Vector3Int, Chunk>();

    public Chunk(int size, int height, Vector3Int worldPosition)
    {
        ChunkGenerator = new ChunkGenerator(this);
        Size = size;
        Height = height;
        WorldPosition = worldPosition;
        Blocks = new BlockType[size, height, size];
    }

    public void GenerateChunkMesh()
    {
        ChunkMesh = new ChunkMesh(true);
        for(int x = 0; x < Blocks.GetLength(0); x++)
            for(int y = 0; y < Blocks.GetLength(1); y++)
                for(int z = 0; z < Blocks.GetLength(2); z++)
                    ChunkMesh.TryAddBlock(this, Blocks[x, y, z], new Vector3Int(x, y, z));
    }
    
    public void SetBlock(Vector3Int localPosition, BlockType blockType, bool generating = false)
    {
        if (inRange(localPosition))
        {
            Blocks[localPosition.x, localPosition.y, localPosition.z] = blockType;

            if (!generating)
            {
                Save.BlocksToSave.Enqueue(new Block(WorldPosition + localPosition, blockType, this));
                GenerateChunkMesh();
                ChunkRenderer.UpdateMesh();
            }
            
            if (!generating && IsOnEdge(WorldPosition + localPosition))
            {
                foreach (var touching in GetTouchingChunks(WorldPosition + localPosition))
                {
                    touching.GenerateChunkMesh();
                    touching.ChunkRenderer.UpdateMesh();
                }
            }
        }
        else
            World.SetBlock(WorldPosition + localPosition, blockType, generating);
    }
    
    public Block GetBlock(Vector3Int localPosition)
    {
        var globalPos = WorldPosition + localPosition;
        if (inRange(localPosition))
        {
            var blockType = Blocks[localPosition.x, localPosition.y, localPosition.z];
            return new Block(globalPos, blockType, this);
        }
        return World.GetBlock(globalPos);
    }

    public Block GetBlockGlobalCoord(Vector3Int globalPosition)
    {
        return GetBlock(globalPosition - WorldPosition);
    }
    
    public Vector3Int GetLocalPosition(Vector3Int globalPosition)
    {
        return new Vector3Int(
            globalPosition.x - WorldPosition.x,
            globalPosition.y - WorldPosition.y,
            globalPosition.z - WorldPosition.z);
    }
    
    public bool IsOnEdge(Vector3Int globalPosition)
    {
        Vector3Int localPos = GetLocalPosition(globalPosition);
        return localPos.x == 0 || localPos.x == Size - 1 || localPos.y == 0 || localPos.y == Height - 1 ||
               localPos.z == 0 || localPos.z == Size - 1;
    }
    
    public IEnumerable<Chunk> GetTouchingChunks(Vector3Int globalPosition)
    {
        List<Chunk> neighboursToUpdate = new List<Chunk>();
        var localPos = GetLocalPosition(globalPosition);
        
        if(localPos.x == 0)
            neighboursToUpdate.Add(World.GetChunk(globalPosition + Vector3Int.left));
        else if(localPos.x == Size - 1)
            neighboursToUpdate.Add(World.GetChunk(globalPosition + Vector3Int.right));
        if(localPos.z == 0)
            neighboursToUpdate.Add(World.GetChunk(globalPosition + Vector3Int.back));
        else if(localPos.z == Size - 1)
            neighboursToUpdate.Add(World.GetChunk(globalPosition + Vector3Int.forward));
        
        return neighboursToUpdate;
    }
    
    public void RenderChunk()
    {
        if (GameManager.World.WorldGenerator.ChunkPool.Count > 0)
        {
            ChunkRenderer = GameManager.World.WorldGenerator.ChunkPool.Dequeue();
            ChunkRenderer.transform.position = WorldPosition;
        }
        else
        {
            GameObject chunkObj = Object.Instantiate(GameManager.World.ChunkPrefab, WorldPosition, Quaternion.identity);
            chunkObj.layer = LayerMask.NameToLayer("Ground");
            chunkObj.transform.parent = GameManager.WorldObj.transform;
            ChunkRenderer = chunkObj.GetComponent<ChunkRenderer>();
        }

        ChunkRenderer.Chunk = this;
        ChunkRenderer.UpdateMesh();
        ChunkRenderer.gameObject.SetActive(true);
    }
    
    private bool inRange(Vector3Int localPosition)
    {
        return localPosition.x >= 0 && localPosition.x < Size && localPosition.z >= 0 && localPosition.z < Size &&
               localPosition.y >= 0 && localPosition.y < Height;
    }
}
