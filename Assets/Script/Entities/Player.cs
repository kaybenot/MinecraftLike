using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity
{
    [SerializeField, Range(0f, 1f)] private float sensitivity = 0.3f;
    [SerializeField] private LayerMask groundMask = -1;
    
    [Header("Player body components")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerHead;
    
    [Header("Player properties")]
    [SerializeField, Min(0f)] private float speed = 2f;
    [SerializeField, Range(1f, 2f)] private float runMultiplier = 1.5f;
    [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
    [SerializeField] private float groundMaxAngle = 30f;
    [SerializeField] private float range = 4f;
    [SerializeField, Range(-90f, 90f)] private float maxLookAngle = 90f; 
    [SerializeField, Range(-90f, 90f)] private float minLookAngle = -90f; 
    
    public bool Grounded { get; private set; }
    public Action OnAttack { get; set; }
    public Action OnPlace { get; set; }
    public Vector3Int BlockPosition => Vector3Int.RoundToInt(transform.position);
    public Vector3Int ChunkPosition => GameManager.World.GetChunk(BlockPosition).WorldPosition;
    public bool BlockInput { get; set; } = false;

    private Rigidbody rb;
    private Camera cam;
    private Vector3 inputForce;
    private Animator animator;
    private Transform collitionTransform;
    private float minGroundDotProduct;
    private float headRotation;
    private static readonly int Walking = Animator.StringToHash("Walking");

    private void OnValidate()
    {
        if (maxLookAngle < minLookAngle)
            maxLookAngle = minLookAngle;
    }

    private void Awake()
    {
        OnAttack += destroyBlock;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        collitionTransform = GetComponentInChildren<BoxCollider>().transform;
        cam = Camera.main;

        minGroundDotProduct = Mathf.Cos(groundMaxAngle * Mathf.Deg2Rad);
        
        Inventory = new Inventory(10);
    }

    private void Update()
    {
        rb.velocity = inputForce.z * player.forward * speed + 
                      inputForce.x * player.right * speed + Vector3.up * rb.velocity.y;
        collitionTransform.rotation = Quaternion.identity;
    }

    private void OnCollisionExit(Collision collision)
    {
        Grounded = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if(contact.normal.y >= minGroundDotProduct)
                Grounded = true;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (BlockInput)
        {
            inputForce = Vector3.zero;
            animator.SetBool(Walking, false);
            return;
        }

        if (context.performed)
        {
            var force = context.ReadValue<Vector2>();
            inputForce = new Vector3(force.x, 0f, force.y);
            animator.SetBool(Walking, true);
        }
        else if (context.canceled)
        {
            inputForce = Vector3.zero;
            animator.SetBool(Walking, false);
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (BlockInput)
            return;
        if (context.performed)
        {
            if(Grounded && Math.Abs(rb.velocity.y) < 0.01f)
                rb.velocity += Vector3.up * Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (BlockInput)
            return;
        if (context.performed)
        {
            Vector2 look = context.ReadValue<Vector2>() * sensitivity;
            headRotation -= look.y;
            headRotation = Mathf.Clamp(headRotation, minLookAngle, maxLookAngle);
            
            player.localRotation = Quaternion.Euler(0f, look.x + player.localEulerAngles.y, 0f);
            playerHead.localRotation = Quaternion.Euler(headRotation, 0f, 0f);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (BlockInput)
            return;
        if (context.performed)
            OnAttack?.Invoke();
    }

    public void Place(InputAction.CallbackContext context)
    {
        if (BlockInput)
            return;
        if (context.performed)
            OnPlace?.Invoke();
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (context.started)
            speed *= runMultiplier;
        else if (context.canceled)
            speed /= runMultiplier;
    }

    public void GameMenu(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.GameMenuShown)
                GameManager.HideGameMenu();
            else
                GameManager.ShowGameMenu();
        }
    }

    private void destroyBlock()
    {
        Ray playerRay = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(playerRay, out RaycastHit hit, range, groundMask))
            modifyTerrain(hit);
    }

    private void modifyTerrain(RaycastHit hit)
    {
        Vector3Int pos = Vector3Int.RoundToInt(hit.point - hit.normal * 0.1f);
        var chunk = GameManager.World.GetChunk(pos);
        Drop.Spawn(pos, (ItemType) chunk.GetBlockGlobalCoord(pos).BlockType);
        GameManager.World.SetBlock(pos, BlockType.Air);

        if (chunk.IsOnEdge(pos))
        {
            IEnumerable<Chunk> neighbourChunks = chunk.GetTouchingChunks(pos);
            foreach (var neigbourChunk in neighbourChunks)
                neigbourChunk.ChunkRenderer.UpdateMesh();
        }
    }

    public void PickUpItem(ItemType itemType)
    {
        Inventory.AddItem(itemType);
    }
}
