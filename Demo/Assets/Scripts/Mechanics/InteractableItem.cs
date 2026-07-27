using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script handles logic for an item or object that performs
// some task when the player is within range and presses a
// button.
// ==============================================================
[RequireComponent(typeof(Collider2D))]
public class InteractableItem : MonoBehaviour
{
    //Store variable containing if player is next to object
    private bool _playerInRange;

    //Default the range boolean to false
    void Awake()
    {
        _playerInRange = false;
    }

    //Each frame, check if player has pressed E and is within range
    void Update()
    {
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Interacted with {this.gameObject}");
        }
    }
    
    //If the trigger collider is entered by a player, set range boolean
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            _playerInRange = true;
        }
    }

    //If the trigger collider is exited by a player, set range boolean
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            _playerInRange = false;
        }
    }
}