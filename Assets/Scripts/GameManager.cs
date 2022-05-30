using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject[] heroes = new GameObject[4];
    [SerializeField] private GameObject activeHero;
    private int activeHeroIndex;

    private void Start()
    {
        foreach (GameObject hero in heroes) {
            hero.GetComponent<PlayerInput>().enabled = false;
            hero.GetComponent<PlayerController>().enabled = false;
        }
        activeHeroIndex = 0;
        activeHero = heroes[activeHeroIndex];
        activeHero.GetComponent<PlayerInput>().enabled = true;
        activeHero.GetComponent<PlayerController>().enabled = true;
    }

    private void Update()
    {

    }

    public void SwitchHero(GameObject unit)
    {
        unit.GetComponent<PlayerInput>().enabled = false;
        unit.GetComponent<PlayerController>().enabled = false;

        activeHeroIndex++;
        activeHeroIndex %= heroes.Length;
        activeHero = heroes[activeHeroIndex];

        activeHero.GetComponent<PlayerInput>().enabled = true;
        activeHero.GetComponent<PlayerController>().enabled = true;
    }
}
