using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block 
{
    private static Dictionary<BlockType, BlockData> BlockDatas = new Dictionary<BlockType, BlockData>();
    
    public Vector3Int GlobalPosition { get; }
    public BlockType BlockType { get; set; }
    public BlockData BlockData => BlockDatas[BlockType];
    public Chunk Chunk => GameManager.World.GetChunk(GlobalPosition);

    public Block(Vector3Int globalPosition, BlockType blockType)
    {
        GlobalPosition = globalPosition;
        BlockType = blockType;
    }
    
    public static void AddBlockData(BlockData blockData)
    {
        if (BlockDatas.ContainsKey(blockData.BlockType) == false)
            BlockDatas.Add(blockData.BlockType, blockData);
    }
}
