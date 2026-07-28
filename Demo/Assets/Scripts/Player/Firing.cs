using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script handles logic allowing a player to fire
// projectiles.
// Projectile is customizable through the projectile Serialized
// Field.
// ==============================================================
public class Firing : MonoBehaviour
{
    //Store a reference to the prefab for the projectile
    [SerializeField] GameObject _projectile;

    //Each frame, spawn a projectile if the space key is held down
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(_projectile, transform.position + Vector3.up, Quaternion.identity);
        }
    }

    //If the projectile hits a collider object, run logic
    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"Projectile hit object {col.gameObject}");
    }
}