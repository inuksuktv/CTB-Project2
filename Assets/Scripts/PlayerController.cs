using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;

    private InputAction foldAction;
    private InputAction switchAction;
    private InputAction moveAction;

    private float speed = 10f;
    [SerializeField] private Vector2 move;
    private Rigidbody2D playerRb;
    private GameObject gameManager;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        foldAction = playerInput.actions["Fold"];
        switchAction = playerInput.actions["Switch"];
        moveAction = playerInput.actions["Move"];
    }

    private void OnEnable()
    {
        foldAction.performed += Fold;
        foldAction.Enable();
        switchAction.performed += Switch;
        switchAction.Enable();
        moveAction.Enable();
    }

    private void OnDisable()
    {
        foldAction.performed -= Fold;
        foldAction.Disable();
        switchAction.performed -= Switch;
        switchAction.Disable();
        moveAction.Disable();
    }
    

    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        playerRb = GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        move = moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (move != Vector2.zero) { Move(); }
    }

    private void Fold(InputAction.CallbackContext context)
    {
        Debug.Log("Fold");
    }

    private void Switch(InputAction.CallbackContext context)
    {
        Debug.Log("Switch " + gameObject.name);
        gameManager.GetComponent<GameManager>().SwitchHero(gameObject);
    }

    private void Move()
    {
        playerRb.transform.Translate(speed * Time.deltaTime * move);
    }
}
