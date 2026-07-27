using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class CombatHandler : MonoBehaviour
{
    // [SerializeField] makes private variables show up in the Unity Editor!
    [SerializeField] private Combatant[] combatants;
    [SerializeField] private GameObject battleUI;
    [SerializeField] private int currentIndex;
    [SerializeField] private Combatant activeCombatant;


    private void Start()
    {
        StartFight();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("space");
            NextTurn();
        }
    }

    void StartFight()
        {
            // Find all combatants and store them in a class variable
            combatants = FindCombatants();

            NextTurn();

        }


    void NextTurn()
        {
        if (currentIndex == (combatants.Length - 1))
        {
            currentIndex = 0;
        }
        else currentIndex++;

        activeCombatant = combatants[currentIndex];

        }

    private Combatant[] FindCombatants()
        {
            // Search for all active GameObjects with the Combatant type and stores them
            // in an unsorted array
            Combatant[] foundCombatants = FindObjectsByType<Combatant>(FindObjectsSortMode.None);

            // Order them by highest to lowest speed value
            Combatant[] turnOrder = foundCombatants.OrderByDescending(combatant => combatant.speed).ToArray();

            return turnOrder;
        }


    void EndFight()
        {

        }
}
