using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGUIManager : MonoBehaviour
{
    public enum HeroGUI
    {
        Idle,
        Available,
        Done
    }
    public HeroGUI heroGUI;

    public List<Portraits> portraits = new List<Portraits>();

    void Start()
    {
        heroGUI = HeroGUI.Idle;
    }

    void Update()
    {
        
    }

    private void LateUpdate()
    {
        switch (heroGUI) {
            case HeroGUI.Idle:

                break;

            case HeroGUI.Available:

                break;

            case HeroGUI.Done:

                break;
        }
    }
    public void ReceiveTurnQueue(List<GameObject> turnQueue)
    {
        heroGUI = HeroGUI.Available;

        GeneratePortraits(turnQueue);
        AddPortraitsToGUI();
    }

    // Prepare the portraits based on the turnQueue.
    private void GeneratePortraits(List<GameObject> turnQueue)
    {
        portraits.Clear();

        foreach (GameObject turn in turnQueue) {
            Portraits newPanel = new Portraits {
                unit = turn,
                sprite = turn.GetComponent<UnitStateMachine>().portrait,
                duplicate = false
            };
            foreach (Portraits portrait in portraits) {
                if (portrait.unit == turn) {
                    newPanel.duplicate = true;
                }
            }
            portraits.Add(newPanel);
        }
    }

    private void AddPortraitsToGUI()
    {
        int index = 0;
        foreach (Portraits portrait in portraits) {
            index++;
        }
    }
}
