using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryWindow : MonoBehaviour
{
    [SerializeField] private GameObject inventoryWindow;
    [SerializeField] private GameObject inventorySlot;

    public bool isOpened { get; private set; } = false;
    
    private GameObject openInventory;
    
    public static InventoryWindow Singleton;

    private void Awake()
    {
        Singleton = this;
    }

    public void OpenInventory(Inventory inventory)
    {
        var size = inventory.Size;
        const float slotSize = 80f;
        const float margin = 10f;
        var slotAmountX = inventory.Size > 10 ? 10 : inventory.Size;
        var slotAmountY = inventory.Size / 10;
        
        var obj = new GameObject("Inventory");
        var canvas = FindObjectOfType<Canvas>().transform;
        obj.transform.SetParent(canvas);
        obj.transform.SetSiblingIndex(4);
        var rt = obj.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        var windowGO = Instantiate(inventoryWindow, obj.transform);
        var windowRT = windowGO.GetComponent<RectTransform>();
        windowRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotAmountX * slotSize + margin * 2);
        windowRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotAmountY * slotSize + margin * 2);

        for (var i = 0; i < inventory.Size; i++)
        {
            var x = i % 10;
            var y = i / 10;

            var slotGO = Instantiate(inventorySlot, windowGO.transform);
            var slotRT = slotGO.GetComponent<RectTransform>();
            slotRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, margin + y * slotSize, slotSize);
            slotRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, margin + x * slotSize, slotSize);
            
            var slot = slotGO.GetComponent<ItemSlot>();
            slot.Inventory = inventory;
            slot.SlotIndex = i;
            slot.Item = inventory.GetItem(i);
        }
        
        Toolbar.Singleton.Close();
        openInventory = obj;
        isOpened = true;
    }

    public void UpdateInventoryWindow(Inventory inventory)
    {
        if (openInventory == null)
            return;
        
        var slots = openInventory.GetComponentsInChildren<ItemSlot>();
        for (var i = 0; i < inventory.Size; i++)
        {
            slots[i].Item = inventory.GetItem(i);
        }
    }
    
    public void CloseInventory()
    {
        if (isOpened)
        {
            if(GameMouse.Item != null && GameMouse.Item.ItemType != ItemType.Nothing)
            {
                for (var i = 0; i < GameMouse.Item.Amount; i++)
                    Drop.Spawn(GameManager.Player.transform.position + Vector3.up * 1.5f, GameMouse.Item.ItemType, GameManager.Player.transform.forward * 2.5f);
                GameMouse.Item = new Item(ItemType.Nothing);
            }
            Toolbar.Singleton.Open();
            Destroy(openInventory);
            isOpened = false;
        }
    }
}
