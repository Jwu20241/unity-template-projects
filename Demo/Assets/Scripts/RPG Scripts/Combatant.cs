using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Combatant : MonoBehaviour
{
    public int speed = 10;
    public int strength = 10;
    public int mp = 10;
    public int maxHealth = 10;
    public int currentHealth;
    public bool isPartyMember;

    void Start()
    {
        currentHealth = maxHealth;

        if (isPartyMember) GetComponent<SpriteRenderer>().color = Color.green;
        else GetComponent<SpriteRenderer>().color = Color.red;
    }

}
