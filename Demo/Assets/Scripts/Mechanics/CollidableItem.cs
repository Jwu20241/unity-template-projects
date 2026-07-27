using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script handles logic for an item or object that performs
// some task upon being collided with.
// Both OnTriggerEnter2D and OnCollisionEnter2D are provided. The
// present debug statements should be replaced with your own
// logic.
// ==============================================================
[RequireComponent(typeof(Collider2D))]
public class CollidableItem : MonoBehaviour
{
    // Activates when the game object containing a trigger collider collides with another collider object
    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col);
    }
    // Activates when the game object containing a collider collides with another collider object
    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log(col);
    }
}