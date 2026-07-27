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

    public float speed;
    public float jumpHeight;

    // Start is called before the first frame update
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        _rb2d.velocity = HorizontalSpeed();

        if(Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("jump");
            _rb2d.velocity = Jump();
        }
    }

    Vector2 HorizontalSpeed()
    {
        return new Vector2(Input.GetAxis("Horizontal") * speed, _rb2d.velocity.y);
    }

    Vector2 Jump()
    {
        return Vector2.up * jumpHeight;
    }
}
