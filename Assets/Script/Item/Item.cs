using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public ItemType ItemType { get; }
    public int Amount { get; set; }

    public Item(ItemType itemType)
    {
        ItemType = itemType;
        Amount = 1;
    }
}
