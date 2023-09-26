using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private PlayerActions playerActions;
    [SerializeField] private GameObject attackCollider;

    private float attackCooldown;
    private float attackTime;

    private float immunityTime;
    private bool immune;
    public bool Immune
    {
        get => immune;
        set => immune = value;
    }


    private void Awake()
    {
        playerActions = new PlayerActions();
        attackCollider.SetActive(false);
    }
    // Update is called once per frame

    private void OnEnable()
    {
        playerActions.Enable();
        playerActions.Attack.Attack.performed += attack => {
            DoAttack();
        };
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    void Update()
    {
        if(attackCollider.activeSelf)
        {
            attackTime -= Time.deltaTime;
            if(attackTime <= 0)
            {
                attackCollider.SetActive(false);
            }
        }
        else
        {
            if(attackCooldown > 0 && attackTime < 0)
            {
                attackCooldown -= Time.deltaTime;

                if(attackCooldown <= 0)
                {
                    attackCooldown = 0;
                }
            }
        }


        if(immunityTime > 0)
        {
            immunityTime -= Time.deltaTime;

            if(immunityTime < 0)
            {
                immunityTime = 0;
                immune = false;
                this.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }
        }
    }

    private void DoAttack()
    {
        if(attackCooldown == 0)
        {
            attackCollider.SetActive(true);
            attackTime = 0.2f;
            attackCooldown = 0.5f;
        }
    }

    public void BeenHit()
    {
        immune = true;
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        immunityTime = 0.8f;
    }
}

