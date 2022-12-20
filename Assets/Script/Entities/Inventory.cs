using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory
{
    public int Size { get; }
    public Item[] Items { get; set; }

    public Inventory(int size)
    {
        Size = size;
        Items = new Item[size];
        for(var i = 0; i < size; i++)
            Items[i] = new Item(ItemType.Nothing);
    }

    public void AddItem(ItemType itemType)
    {
        var slot = ContainsItem(itemType);
        if (slot == -1)
        {
            var emptySlot = GetFirstEmptySlot();
            if (emptySlot == -1)
                return;
            Items[emptySlot] = new Item(itemType);
        }
        else
        {
            Items[slot].Amount++;
        }
    }
    
    public Item GetItem(int slot)
    {
        return Items[slot];
    }
    
    public void RemoveItem(int slot)
    {
        Items[slot] = new Item(ItemType.Nothing);
    }
    
    public int ContainsItem(ItemType itemType)
    {
        for(var i = 0; i < Size; i++)
            if(Items[i].ItemType == itemType)
                return i;
        return -1;
    }
    
    public int GetFirstEmptySlot()
    {
        for(var i = 0; i < Size; i++)
            if(Items[i].ItemType == ItemType.Nothing)
                return i;
        return -1;
    }
}
