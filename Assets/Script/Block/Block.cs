using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block 
{
    public static Dictionary<BlockType, BlockData> BlockDatas = new Dictionary<BlockType, BlockData>();
    
    public Vector3Int GlobalPosition { get; }
    public BlockType BlockType
    {
        get => blockType;
        set
        {
            GameManager.World.SetBlock(GlobalPosition, value);
            blockType = value;
        }
    }
    public BlockData BlockData => BlockDatas[BlockType];
    public Chunk Chunk { get; }

    private BlockType blockType;

    public Block(Vector3Int globalPosition, BlockType blockType, Chunk chunk)
    {
        GlobalPosition = globalPosition;
        this.blockType = blockType;
        Chunk = chunk;
    }
    
    public static void AddBlockData(BlockData blockData)
    {
        if (BlockDatas.ContainsKey(blockData.BlockType) == false)
            BlockDatas.Add(blockData.BlockType, blockData);
    }
}
