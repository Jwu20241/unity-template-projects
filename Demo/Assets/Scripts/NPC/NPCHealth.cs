using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script handles logic for an NPC that can take damage and
// perish; useful for passive and enemy NPCs.
// ==============================================================
public class NPCHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float _health;
    [SerializeField] float _maxHealth;

    void Awake() {_health = _maxHealth;}

    //Subtract amount from health, checks for health below zero
    public void DealDamage(float amount)
    {
        _health -= amount;
        StartCoroutine(FlashRed());
        if (_health <= 0) {HandleDeath();}
    }

    //Heals amount of health, cannot overheal
    public void HealHealth(float amount)
    {
        _health += amount;
        if (_health > _maxHealth) {_health = _maxHealth;}
    }

    //Destroys the object upon reaching zero hp
    void HandleDeath()
    {
        Destroy(this.gameObject);
    }

    //Creates a visual red flash
    IEnumerator FlashRed()
    {
        SpriteRenderer sp = this.gameObject.GetComponent<SpriteRenderer>();
        Color original = sp.color;
        sp.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sp.color = original;
    }
}
