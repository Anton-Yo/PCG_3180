using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private EnemyMovement enemyScript;

    private float startTimer;

    // Start is called before the first frame update
    void Start()
    {
        startTimer = startDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if(startTimer > 0)
        {
            startTimer -= Time.deltaTime;
            if(startTimer <= 0)
            {
                enemyScript.Move(true);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if(!other.gameObject.GetComponent<Player>().Immune)
            {
                GameManager.Instance.Damage(20); 
                other.gameObject.GetComponent<Player>().BeenHit();
            }
        }

        if(other.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if(!other.gameObject.GetComponent<Player>().Immune)
            {
                GameManager.Instance.Damage(20); 
                other.gameObject.GetComponent<Player>().BeenHit();
            }
        }
    }
}
