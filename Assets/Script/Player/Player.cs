using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 2f;
    
    private Rigidbody rb;
    private Vector3 inputForce;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (inputForce != Vector3.zero)
            rb.velocity = inputForce * speed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var force = context.ReadValue<Vector2>();
            inputForce = new Vector3(force.x, 0f, force.y);
        }
        else if(context.canceled)
            inputForce = Vector3.zero;
    }

    public void Look(InputAction.CallbackContext context)
    {
    }
}
