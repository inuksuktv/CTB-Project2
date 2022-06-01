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

    public List<Portraits> turnQueue = new List<Portraits>();

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
    public void ReceiveTurnQueue(List<CachedInitiative> turnList)
    {
        heroGUI = HeroGUI.Available;

        GeneratePortraits(turnList);
        AddPortraitsToGUI();
    }

    private void GeneratePortraits(List<CachedInitiative> turnList)
    {
        turnQueue.Clear();

        foreach (CachedInitiative cache in turnList) {
            Portraits newPanel = new Portraits {
                unit = cache.unit,
                sprite = cache.unit.GetComponent<UnitStateMachine>().portrait,
                duplicate = false
            };
            foreach (Portraits portrait in turnQueue) {
                if (portrait.unit == cache.unit) {
                    newPanel.duplicate = true;
                }
            }
            turnQueue.Add(newPanel);
        }
    }

    private void AddPortraitsToGUI()
    {
        int index = 0;
        foreach (Portraits portrait in turnQueue) {
            index++;
        }
    }
}
