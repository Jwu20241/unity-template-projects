using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script handles player movement and input in a 2D platformer
// environment. A Rigidbody2D component is required, and gravity
// should be enabled. Control the speed of the player via the
// public speed variable and jump distance with jumpHeight.
// ==============================================================

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerMovement : MonoBehaviour
{
    private Rigidbody2D _rb2d;
    private bool _grounded;

    public float speed;
    public float jumpHeight;

    // Start is called before the first frame update
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    // Mod
    void Update()
    {
        _rb2d.velocity = HorizontalSpeed();

        if(Input.GetKeyDown(KeyCode.Space) && _grounded) {
            _rb2d.velocity = Jump();
        }
    }

    // Calculates the horizontal speed for bidirectional movement
    // Works with arrow keys and AD based on the 'speed' variable
    Vector2 HorizontalSpeed()
    {
        return new Vector2(Input.GetAxis("Horizontal") * speed, _rb2d.velocity.y);
    }

    // Calculates the jump velocity based on the 'jumpHeight' variable
    Vector2 Jump()
    {
        return new Vector2(_rb2d.velocity.x, jumpHeight);
    }

    // Detects when the player collides with a floor
    // Modifies the grounded variable to prevent multiple jumps
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            _grounded = true;
        }
    }

    // Detects when the player exits the floor
    // Modifies the grounded variable to prevent multiple jumps
    void OnCollisionExit2D(Collision2D collision) 
    {
        if (collision.gameObject.tag == "Floor")
        {
            _grounded = false;
        }
    }
}
