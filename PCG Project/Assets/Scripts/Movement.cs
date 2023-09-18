using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private PlayerActions playerActions;

    private Vector3 movementDirection = new Vector3(0,0, 0);

    [SerializeField] private float speed = 1f;


    private void Awake()
    {
        playerActions = new PlayerActions();

    }

    private void OnEnable()
    {
        playerActions.Enable();
        playerActions.Movement.Vertical.performed += moving => { 
            movementDirection.y = moving.ReadValue<float>();
        };

        playerActions.Movement.Horizontal.performed += moving => { 
            movementDirection.x = moving.ReadValue<float>();
        };

        playerActions.Movement.Vertical.canceled += moving => {
            movementDirection.y = 0;
        };

          playerActions.Movement.Horizontal.canceled += moving => {
            movementDirection.x = 0;
        };

    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position += movementDirection * Time.deltaTime * speed;
    }
}
