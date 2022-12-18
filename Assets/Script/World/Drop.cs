using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Drop : MonoBehaviour
{
    public ItemType ItemType = ItemType.Nothing;
    [SerializeField] private float rotationSpeed = 36f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.1f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        pushUpwards();
    }

    private void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Sin(Time.time * bobSpeed) * bobHeight, transform.localPosition.z);
        transform.localRotation = Quaternion.Euler(0, Time.time * rotationSpeed, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        var parent = other.transform.parent;
        if (parent == null || !parent.TryGetComponent(out Player player))
            return;
        player.PickUpItem(ItemType);
        Destroy(gameObject);
    }

    public static void Spawn(Vector3 globalPosition, ItemType itemType)
    {
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

    private void pushUpwards()
    {
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle;
        rb.AddForce(new Vector3(randomDir.x, 0.5f, randomDir.y) * 10f, ForceMode.Impulse);
    }
}
