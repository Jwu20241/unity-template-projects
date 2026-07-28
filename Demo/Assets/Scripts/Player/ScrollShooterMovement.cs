using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script handles player movement and input in a 2D shooter
// environment. A Rigidbody2D component is required, and gravity
// should be disabled. Control the speed of the player via the
// public speed variable.
// ==============================================================
[RequireComponent(typeof(Rigidbody2D))]
public class ScrollShooterMovement : TopDownMovement
{
    //Override the functionality of TopDownMovement to only allow horizontal movement
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            _rb2d.velocity = Vector2.left * speed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _rb2d.velocity = Vector2.right * speed;
        }
    }
}