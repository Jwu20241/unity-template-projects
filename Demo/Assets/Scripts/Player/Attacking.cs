using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script handles logic for players to allow attacking
// any entity with a health script.
// ==============================================================
public class Attacking : MonoBehaviour
{
    [SerializeField] float _attackDamage;
    [SerializeField] float _attackRadius;

    //Each frame, if the key is pressed, start an attack
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleAttack();
        }
    }

    //Performs calculations to deal damage.
    void HandleAttack()
    {
        RaycastHit2D hit = Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), _attackRadius, Vector2.zero);
        if (!hit) {return;}
        IDamageable i = hit.collider.gameObject.GetComponent<IDamageable>();
        i.DealDamage(_attackDamage);
    }
}