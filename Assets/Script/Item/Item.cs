using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    public ItemType ItemType { get; private set; }
    public int Amount { get; set; }
    
    public bool Exists => Amount > 0 && ItemType != ItemType.Nothing;

    public Item(ItemType itemType)
    {
        ItemType = itemType;
        Amount = 1;
    }
    
    public void DecreaseAmount()
    {
        Amount--;
        if(Amount <= 0)
        {
            ItemType = ItemType.Nothing;
            Amount = 1;
        }
    }
    
    public Rect IconRect
    {
        get
        {
            var uvSide = Block.BlockDatas[(BlockType)ItemType].side;
            var tileWidth = GameManager.BlockAtlas.TileWidth;
            var tileHeight = GameManager.BlockAtlas.TileHeight;
            return new Rect(uvSide.x * tileWidth, uvSide.y * tileHeight, tileWidth, tileHeight);
        }
    }
}
