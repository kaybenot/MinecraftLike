using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Drop : MonoBehaviour
{
    public ItemType ItemType = ItemType.Nothing;
    
    [SerializeField] private float pickupTime = 3f;
    [SerializeField] private float rotationSpeed = 36f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.1f;

    private Rigidbody rb;
    private bool canPickup;
    private static Vector3 force;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        push();
    }

    private void Update()
    {
        if(pickupTime > 0)
        {
            pickupTime -= Time.deltaTime;
            if (pickupTime <= 0)
                canPickup = true;
        }

        var localPos = transform.localPosition;
        transform.localPosition = new Vector3(localPos.x, Mathf.Sin(Time.time * bobSpeed) * bobHeight, localPos.z);
        transform.localRotation = Quaternion.Euler(0, Time.time * rotationSpeed, 0);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!canPickup)
            return;
        
        var parent = other.transform.parent;
        if (parent == null || !parent.TryGetComponent(out Player player))
            return;
        player.PickUpItem(ItemType);
        Destroy(gameObject.transform.parent.gameObject);
    }

    public static void Spawn(Vector3 globalPosition, ItemType itemType, Vector3 pushForce)
    {
        force = pushForce;
        
        var dropObj = Instantiate(GameManager.DropPrefab, globalPosition, quaternion.identity);
        var drop = dropObj.GetComponentInChildren<Drop>();
        drop.ItemType = itemType;

        var globPos = Vector3Int.RoundToInt(globalPosition);
        var bm = new BlockMesh();
        bm.Create((BlockType)itemType, Vector3Int.zero);

        var mr = dropObj.GetComponentInChildren<MeshFilter>();
        var m = mr.mesh;
        m.Clear();
        m.subMeshCount = 1;
        m.vertices = bm.Vertices.ToArray();
        m.SetTriangles(bm.Triangles, 0);
        m.uv = bm.UV.ToArray();
        m.RecalculateNormals();
    }

    private void push()
    {
        rb.AddForce(force * 10f, ForceMode.Impulse);
    }
}
