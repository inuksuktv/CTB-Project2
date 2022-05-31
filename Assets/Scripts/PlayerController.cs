using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private GameObject gameManager;
    private PlayerInput playerInput;

    private InputAction foldAction;
    private InputAction switchAction;
    private InputAction moveAction;

    private float speed = 10f;
    private Vector2 move;
    private Rigidbody2D playerRb;
    private Animator animator;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        playerInput = gameManager.GetComponent<PlayerInput>();
        foldAction = playerInput.actions["Fold"];
        switchAction = playerInput.actions["Switch"];
        moveAction = playerInput.actions["Move"];
    }

    private void OnEnable()
    {
        foldAction.performed += Fold;
        switchAction.performed += Switch;

        foldAction.Enable();
        switchAction.Enable();
        moveAction.Enable();
    }

    private void OnDisable()
    {
        foldAction.performed -= Fold;
        switchAction.performed -= Switch;

        foldAction.Disable();
        switchAction.Disable();
        moveAction.Disable();
    }
    

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        animator.SetTrigger("Attack");
        Debug.Log("Fold");
    }

    private void Switch(InputAction.CallbackContext context)
    {
        GameManager.Instance.SwitchHero(gameObject);
    }

    private void Move()
    {
        playerRb.transform.Translate(speed * Time.deltaTime * move);
    }
}
