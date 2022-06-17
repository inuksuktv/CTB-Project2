using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class BattleGUIManager : MonoBehaviour
{
    #region "Variables"
    public enum HeroGUI
    {
        Idle,
        Available,
        Targeting,
        Done
    }
    public HeroGUI heroGUI;

    // References to other objects, found in Awake() and Start().
    private GameObject gameManager;
    private BattleManager battleManager;
    private PlayerInput playerInput;
    private RectTransform canvas;

    // Input Action references.
    private InputAction upInput;
    private InputAction leftInput;
    private InputAction rightInput;
    private InputAction downInput;
    private InputAction confirmInput;
    private InputAction cancelInput;

    // Attack handling.
    private bool choosingAbility;
    private bool choosingTarget;
    private bool waitingForConfirmation;
    private GameObject activeUnit;
    private GameObject targetedUnit;
    private int targetIndex;
    public AttackCommand heroChoice;

    // GUI objects.
    private GameObject activePanel;
    private Transform turnQueueSpacer;
    [SerializeField] private GameObject inputPanel;
    [SerializeField] private GameObject turnQueuePanel;
    [SerializeField] private GameObject damagePopup;

    List<Button> buttons = new List<Button>();
    public List<Portraits> portraits = new List<Portraits>();
    private List<GameObject> inputPanels = new List<GameObject>();
    private List<GameObject> GUIObjects = new List<GameObject>();
    #endregion

    #region Awake, OnEnable, and OnDisable
    private void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        playerInput = gameManager.GetComponent<PlayerInput>();

        confirmInput = playerInput.actions["Confirm"];
        cancelInput = playerInput.actions["Cancel"];
        upInput = playerInput.actions["Up"];
        leftInput = playerInput.actions["Left"];
        rightInput = playerInput.actions["Right"];
        downInput = playerInput.actions["Down"];

    }

    private void OnEnable()
    {
        playerInput.actions.FindActionMap("Battle").Enable();

        confirmInput.performed += Confirm;
        cancelInput.performed += Cancel;
        upInput.performed += Up;
        leftInput.performed += Left;
        rightInput.performed += Right;
        downInput.performed += Down;

        confirmInput.Enable();
        cancelInput.Enable();
        upInput.Enable();
        leftInput.Enable();
        rightInput.Enable();
        downInput.Enable();
    }

    private void OnDisable()
    {
        confirmInput.performed -= Confirm;
        cancelInput.performed -= Cancel;
        upInput.performed -= Up;
        leftInput.performed -= Left;
        rightInput.performed -= Right;
        downInput.performed -= Down;

        confirmInput.Disable();
        cancelInput.Disable();
        upInput.Disable();
        leftInput.Disable();
        rightInput.Disable();
        downInput.Disable();

        foreach (GameObject element in GUIObjects) {
            Destroy(element);
        }

        if (playerInput != null) { playerInput.actions.FindActionMap("Battle").Disable(); }
    }
    #endregion

    // All these inputs should be refactored using a state machine. Setting flags to change state is garbage.
    #region InputFunctions

    private void Confirm(InputAction.CallbackContext context)
    {
        if (waitingForConfirmation) {
            choosingTarget = true;
            choosingAbility = false;
            waitingForConfirmation = false;
            // Turn on targeting GUI.
            targetIndex = 0;
            targetedUnit = battleManager.enemiesInBattle[targetIndex];
            targetedUnit.transform.Find("Selector").gameObject.SetActive(true);
        }
        else if (choosingTarget) {
            choosingTarget = false;
            // Target confirmed. Unit can collect the attack and act.
            InputTarget(targetedUnit);
            activeUnit.GetComponent<UnitStateMachine>().CollectAction(heroChoice);
            ClearInputPanels();
            targetedUnit.transform.Find("Selector").gameObject.SetActive(false);
            //Disable all selectors too?
        }
    }

    private void Cancel(InputAction.CallbackContext context)
    {
        if (waitingForConfirmation) {
            waitingForConfirmation = false;
            // Refresh input panel.
            ClearInputPanels();
            activePanel.SetActive(true);
            UpdateButtons();
        }
        else if (choosingTarget) {
            choosingTarget = false;
            choosingAbility = true;
            // Clear targeting GUI and refresh input panel. 
            ClearInputPanels();
            targetedUnit.transform.Find("Selector").gameObject.SetActive(false);
            activePanel.SetActive(true);
            UpdateButtons();
        }
    }

    private void Up(InputAction.CallbackContext context)
    {
        if (choosingAbility) {
            RectTransform buttonRT = buttons[0].GetComponent<RectTransform>();
            Attack attack = activeUnit.GetComponent<UnitStateMachine>().attackList[0];
            InputAttack(buttonRT, attack);
        }
        else if (choosingTarget) {
            TargetNextTeam(targetedUnit);
        }
    }

    private void Left(InputAction.CallbackContext context)
    {
        if (choosingAbility) {
            RectTransform buttonRT = buttons[1].GetComponent<RectTransform>();
            Attack attack = activeUnit.GetComponent<UnitStateMachine>().attackList[1];
            InputAttack(buttonRT, attack);
        }
        else if (choosingTarget) {
            TargetPreviousItem(targetedUnit);
        }
    }

    private void Right(InputAction.CallbackContext context)
    {
        if (choosingAbility) {
            RectTransform buttonRT = buttons[2].GetComponent<RectTransform>();
            Attack attack = activeUnit.GetComponent<UnitStateMachine>().attackList[2];
            InputAttack(buttonRT, attack);
        }
        else if (choosingTarget) {
            TargetNextItem(targetedUnit);
        }
    }

    private void Down(InputAction.CallbackContext context)
    {
        if (choosingAbility) {
            RectTransform buttonRT = buttons[3].GetComponent<RectTransform>();
            Attack attack = activeUnit.GetComponent<UnitStateMachine>().attackList[3];
            InputAttack(buttonRT, attack);
        }
        else if (choosingTarget) {
            TargetNextTeam(targetedUnit);
        }
    }
    #endregion

    void Start()
    {
        battleManager = gameObject.GetComponent<BattleManager>();
        canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
        turnQueueSpacer = canvas.Find("TurnQueueSpacer");
        GUIObjects.Add(turnQueueSpacer.gameObject);

        CreateInputPanels();
    }

    public void ClearInputPanels()
    {
        // Set all the buttons opaque.
        if (activePanel != null) {
            foreach (RectTransform child in activePanel.transform) {
                Image buttonImage = child.GetComponent<Image>();
                Text buttonText = child.GetComponentInChildren<Text>();
                Image arrowImage = child.Find("Arrow").GetComponent<Image>();

                buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f);
                buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 1f);
                arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, 1f);
            }
        }

        foreach (GameObject panel in inputPanels) {
            panel.SetActive(false);
        }
        //infoBox.SetActive(false);
    }

    public void ReceiveTurnQueue(List<GameObject> turnQueue)
    {
        activeUnit = turnQueue[0];

        if (activeUnit.CompareTag("Hero")) {
            activePanel = canvas.transform.Find(activeUnit.name + "Panel").gameObject;
            activePanel.SetActive(true);

            UpdateButtons();

            // Change state.
            choosingAbility = true;
            choosingTarget = false;
        }

        GeneratePortraits(turnQueue);
        SendPortraitsToGUI();
    }

    public void TextPopup(string text, Vector3 position)
    {
        if (text != null) {
            GameObject textPopup = Instantiate(damagePopup, canvas);

            Vector2 screenPoint = Camera.main.WorldToScreenPoint(position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint, null, out Vector2 canvasPoint);
            textPopup.GetComponent<RectTransform>().localPosition = canvasPoint;

            textPopup.GetComponent<TextMeshProUGUI>().text = text;
            textPopup.AddComponent<DamageTextAnimation>();
        }
    }

    private void InputAttack(Transform button, Attack attack)
    {
        heroChoice = ScriptableObject.CreateInstance<AttackCommand>();
        heroChoice.attacker = activeUnit;
        heroChoice.description = attack.description;
        heroChoice.fireTokens = attack.fireTokens;
        heroChoice.waterTokens = attack.waterTokens;
        heroChoice.earthTokens = attack.earthTokens;
        heroChoice.skyTokens = attack.skyTokens;
        heroChoice.damage = attack.damage;
        heroChoice.stateCharge = attack.stateCharge;
        heroChoice.targetMode = attack.targetMode;
        heroChoice.damageMode = attack.damageMode;
        heroChoice.setStatus = attack.setStatus;

        // Send the description to the infobox.

        Image buttonImage;
        Image arrowImage;
        Text buttonText;
        // Set transparency on all buttons.
        foreach (RectTransform child in activePanel.transform) {
            buttonImage = child.GetComponent<Image>();
            buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0.5f);
            arrowImage = child.Find("Arrow").GetComponent<Image>();
            arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, 0.5f);
            buttonText = child.GetComponentInChildren<Text>();
            buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 0.5f);
        }

        // Set the chosen button opaque again.
        buttonImage = button.GetComponent<Image>();
        buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f);
        arrowImage = button.Find("Arrow").GetComponent<Image>();
        arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, 1f);
        buttonText = button.GetComponentInChildren<Text>();
        buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 1f);

        // Change state.
        waitingForConfirmation = true;
    }

    private void InputTarget(GameObject unit)
    {
        heroChoice.target = unit;
    }

    

    private void CreateInputPanels()
    {
        foreach (GameObject hero in battleManager.heroesInBattle) {
            GameObject newPanel = Instantiate(inputPanel, canvas.transform);
            newPanel.name = hero.name + "Panel";

            // Set arrow colours based on unit name.
            if (hero.name == "Fire") {
                foreach (Transform child in newPanel.transform) {
                    Image arrow = child.Find("Arrow").GetComponent<Image>();
                    arrow.color = new Color(1, 0, 0);
                }
            }
            else if (hero.name == "Water") {
                foreach (Transform child in newPanel.transform) {
                    Image arrow = child.Find("Arrow").GetComponent<Image>();
                    arrow.color = new Color(80f / 255f, 134f / 255f, 154f / 255f);
                }
            }
            else if (hero.name == "Earth") {
                foreach (Transform child in newPanel.transform) {
                    Image arrow = child.Find("Arrow").GetComponent<Image>();
                    arrow.color = new Color(211f / 255f, 109f / 255f, 79f / 255f);
                }
            }
            else if (hero.name == "Sky") {
                foreach (Transform child in newPanel.transform) {
                    Image arrow = child.Find("Arrow").GetComponent<Image>();
                    arrow.color = new Color(112f / 255f, 159f / 255f, 69f / 255f);
                }
            }

            newPanel.SetActive(false);
            inputPanels.Add(newPanel);
            GUIObjects.Add(newPanel);

            Vector3 offset = 0.7f * Vector3.up;
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(hero.transform.position + offset);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPoint, null, out Vector2 canvasPoint);
            newPanel.GetComponent<RectTransform>().localPosition = canvasPoint;
        }
    }

    private void GeneratePortraits(List<GameObject> turnQueue)
    {
        portraits.Clear();

        foreach (GameObject turn in turnQueue) {
            Portraits newPanel = new Portraits
            {
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

    private void SendPortraitsToGUI()
    {
        foreach (Transform child in turnQueueSpacer) {
            Destroy(child.gameObject);
        }

        int index = 0;
        foreach (Portraits portrait in portraits) {
            Transform newPanel = Instantiate(turnQueuePanel, turnQueueSpacer).transform;

            Transform icon = newPanel.Find("UnitPortrait");
            Image newPortrait = icon.GetComponent<Image>();
            RectTransform newPortraitRT = icon.GetComponent<RectTransform>();

            newPortrait.sprite = portrait.sprite;

            if (index > 0) {
                newPortraitRT.anchoredPosition += 30 * Vector2.right;
            }

            double calcProgress = portrait.unit.GetComponent<UnitStateMachine>().initiative / battleManager.turnThreshold;

            if (portrait.duplicate) {
                calcProgress = 0;
                newPortraitRT.anchoredPosition += 30 * Vector2.right;
            }

            GameObject progressBar = newPanel.GetComponentInChildren<Mask>().gameObject;
            progressBar.transform.localScale = new Vector3(Mathf.Clamp((float)calcProgress, 0, 1), progressBar.transform.localScale.y, progressBar.transform.localScale.z);

            index++;
        }
    }

    private void TargetNextItem(GameObject currentTarget) 
    {
        if (currentTarget.tag == "Hero") {
            currentTarget.transform.Find("Selector").gameObject.SetActive(false);
            targetIndex++;
            targetIndex %= battleManager.heroesInBattle.Count;
            targetedUnit = battleManager.heroesInBattle[targetIndex];
            targetedUnit.transform.Find("Selector").gameObject.SetActive(true);
        }
        else if (currentTarget.tag == "Enemy") {
            currentTarget.transform.Find("Selector").gameObject.SetActive(false);
            targetIndex++;
            targetIndex %= battleManager.enemiesInBattle.Count;
            targetedUnit = battleManager.enemiesInBattle[targetIndex];
            targetedUnit.transform.Find("Selector").gameObject.SetActive(true);
        }
    }

    private void TargetNextTeam(GameObject currentTarget)
    {
        if (currentTarget.tag == "Hero") {
            currentTarget.transform.Find("Selector").gameObject.SetActive(false);
            targetIndex = 0;
            targetedUnit = battleManager.enemiesInBattle[targetIndex];
            targetedUnit.transform.Find("Selector").gameObject.SetActive(true);
        }
        else if (currentTarget.tag == "Enemy") {
            currentTarget.transform.Find("Selector").gameObject.SetActive(false);
            targetIndex = 0;
            targetedUnit = battleManager.heroesInBattle[targetIndex];
            targetedUnit.transform.Find("Selector").gameObject.SetActive(true);
        }
    }

    private void TargetPreviousItem(GameObject currentTarget)
    {
        if (currentTarget.tag == "Hero") {
            currentTarget.transform.Find("Selector").gameObject.SetActive(false);
            targetIndex--;
            if (targetIndex < 0) { targetIndex = battleManager.heroesInBattle.Count - 1; }
            targetedUnit = battleManager.heroesInBattle[targetIndex];
            targetedUnit.transform.Find("Selector").gameObject.SetActive(true);
        }
        else if (currentTarget.tag == "Enemy") {
            currentTarget.transform.Find("Selector").gameObject.SetActive(false);
            targetIndex--;
            if (targetIndex < 0) { targetIndex = battleManager.enemiesInBattle.Count - 1; }
            targetedUnit = battleManager.enemiesInBattle[targetIndex];
            targetedUnit.transform.Find("Selector").gameObject.SetActive(true);
        }
    }

    private void UpdateButtons()
    {
        buttons = new List<Button>(activePanel.GetComponentsInChildren<Button>());

        int index = 0;
        foreach (Button button in buttons) {
            Text buttonText = button.transform.Find("Text").GetComponent<Text>();
            Image buttonImage = button.GetComponent<Image>();

            Attack attack = activeUnit.GetComponent<UnitStateMachine>().attackList[index];
            buttonText.text = attack.name;
            buttonImage.sprite = attack.buttonSprite;
            index++;
        }
    }
}