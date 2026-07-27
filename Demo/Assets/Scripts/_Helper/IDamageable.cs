using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INFORMATION ==================================================
// This script is used to maintain inheritance and allow damage
// functionality to be shared across entities.
// You should not need to modify this.
// ==============================================================
public interface IDamageable
{
    public void DealDamage(float amount);
    public void HealHealth(float amount);
}
