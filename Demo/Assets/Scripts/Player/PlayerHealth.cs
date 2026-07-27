using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script handles logic for players to allow taking damage
// and healing.
// ==============================================================
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float _health;
    [SerializeField] float _maxHealth;

    void Awake() {_health = _maxHealth;}

    //Subtract amount from health, checks for health below zero
    public void DealDamage(float amount)
    {
        _health -= amount;
        if (_health <= 0) {PlayerLose();}
    }

    //Heals amount of health, cannot overheal
    public void HealHealth(float amount)
    {
        _health += amount;
        if (_health > _maxHealth) {_health = _maxHealth;}
    }

    //Implement logic for when the player runs out of health here
    void PlayerLose()
    {
        Debug.Log("Player has lost the game");
    }
}
