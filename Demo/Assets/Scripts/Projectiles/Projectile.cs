using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script drives projectile movement.
// projectiles.
// Projectile and projectile speed are customizable.
// Projectiles automatically self destruct after 10 seconds;
// ==============================================================
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] Vector2 _direction;
    [SerializeField] float _speed;

    void Start() {StartCoroutine(ProjectileLifetimeTimer());}

    //Change the flight direction of the projectile
    void SetFlightDirection(Vector2 direction)
    {
        _direction = direction;
    }

    //Change the flight speed of the projectile
    void SetFlightSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }

    //Each frame, move the projectile further in the set direction
    void Update()
    {
        transform.position = new Vector3(transform.position.x + _direction.x * _speed, transform.position.y + _direction.y * _speed, transform.position.z);
    }

    IEnumerator ProjectileLifetimeTimer() {yield return new WaitForSeconds(10f); Destroy(this.gameObject);}
}