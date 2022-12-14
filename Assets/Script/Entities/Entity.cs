using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Entity : MonoBehaviour
{
    public float Health { get; set; } = 20f;
}
