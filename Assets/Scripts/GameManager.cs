using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject[] heroes = new GameObject[4];
    [SerializeField] private GameObject activeHero;
    private int activeHeroIndex;

    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }
    }

    private void Start()
    {
        foreach (GameObject hero in heroes) {
            hero.GetComponent<PlayerController>().enabled = false;
        }
        activeHeroIndex = 0;
        activeHero = heroes[activeHeroIndex];
        activeHero.GetComponent<PlayerController>().enabled = true;
    }

    private void Update()
    {

    }

    public void SwitchHero(GameObject unit)
    {
        unit.GetComponent<PlayerController>().enabled = false;

        activeHeroIndex++;
        activeHeroIndex %= heroes.Length;
        activeHero = heroes[activeHeroIndex];

        activeHero.GetComponent<PlayerController>().enabled = true;
    }
}
