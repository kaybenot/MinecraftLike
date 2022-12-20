using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    public List<GameObject> Slots;

    public int Selected
    {
        get => selected;
        set
        {
            if (value is < 0 or > 9)
                return;
            
            var slot = Slots[selected].GetComponent<ItemSlot>();
            slot.Deselect();
            
            selected = value;
            
            var selSlot = Slots[selected].GetComponent<ItemSlot>();
            selSlot.Select();
        }
    }

    public static Toolbar Singleton;

    private int selected = 0;

    private void Start()
    {
        Singleton = this;
        
        for (var i = 0; i < 10; i++)
        {
            Slots[i].GetComponent<ItemSlot>().Inventory = GameManager.Player.Inventory;
            Slots[i].GetComponent<ItemSlot>().SlotIndex = i;
        }
        RefreshSlots();
    }
    
    public void RefreshSlots()
    {
        if (GameManager.Player == null)
            return;
        
        for (var i = 0; i < 10; i++)
            Slots[i].GetComponent<ItemSlot>().Item = GameManager.Player.Inventory.GetItem(i);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        RefreshSlots();
    }
}
