using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float sensitivity = 0.3f;
    
    [Header("Player body components")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerHead;
    
    [Header("Player movement properties")]
    [SerializeField, Min(0f)] private float speed = 2f;
    [SerializeField, Range(1f, 2f)] private float runMultiplier = 1.5f;
    [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
    [SerializeField] private float groundMaxAngle = 30f;
    
    public bool Grounded { get; private set; }
    public Action OnAttack { get; set; }
    public Action OnPlace { get; set; }
    public Vector3Int BlockPosition => Vector3Int.RoundToInt(transform.position);
    public Vector3Int ChunkPosition => GameManager.World.GetChunk(BlockPosition).WorldPosition;

    public Vector3Int ChunkCenter
    {
        get
        {
            var pos = GameManager.World.GetChunk(BlockPosition).WorldPosition;
            return new Vector3Int(
                pos.x + GameManager.World.ChunkSize / 2,
                0,
                pos.z + GameManager.World.ChunkSize / 2);
        }
    }
        

    private Rigidbody rb;
    private Vector3 inputForce;
    private Animator animator;
    private float minGroundDotProduct;
    private static readonly int Walking = Animator.StringToHash("Walking");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        minGroundDotProduct = Mathf.Cos(groundMaxAngle * Mathf.Deg2Rad);
    }

    private void Update()
    {
        rb.velocity = inputForce.z * player.forward * speed + 
                      inputForce.x * player.right * speed + Vector3.up * rb.velocity.y;
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
        if (context.performed)
        {
            if(Grounded && Math.Abs(rb.velocity.y) < 0.01f)
                rb.velocity += Vector3.up * Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 look = context.ReadValue<Vector2>() * sensitivity;
            player.Rotate(0f, look.x, 0f);
            playerHead.Rotate(-look.y, 0f, 0f);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnAttack?.Invoke();
    }

    public void Place(InputAction.CallbackContext context)
    {
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
}
