using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float speed = 3f;

    private Vector3 moveDir = new Vector3(0,0,0);

    // Update is called once per frame
    void Update()
    {
        
        if(this.gameObject.transform.position != player.transform.position)
        {
            moveDir = (player.transform.position - this.gameObject.transform.position).normalized; //calculate direction to move
            //Debug.Log(moveDir);
            transform.position += moveDir * speed * Time.deltaTime;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.Instance.Damage(20);
        }
    }
}
