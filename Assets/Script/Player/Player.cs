using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerHead;
    [SerializeField, Range(0f, 1f)] private float sensitivity = 0.3f;
    [SerializeField, Min(0f)] private float speed = 2f;
    [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
    
    public bool Grounded { get; private set; }
    
    private Rigidbody rb;
    private Vector3 inputForce;
    private Animator animator;
    private static readonly int Walking = Animator.StringToHash("Walking");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        rb.velocity = inputForce.z * player.forward + inputForce.x * player.right + Vector3.up * rb.velocity.y;
    }

    private void OnCollisionExit(Collision collision)
    {
        Grounded = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        Grounded = true;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var force = context.ReadValue<Vector2>();
            inputForce = new Vector3(force.x, 0f, force.y) * speed;
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
            if(Grounded)
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
}
