using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float speed = 3f;

    private bool canMove = false;

    private Vector3 moveDir = new Vector3(0,0,0);

    public void Move(bool val)
    {
        canMove = val;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(canMove && this.gameObject.transform.position != player.transform.position)
        {
            moveDir = (player.transform.position - this.gameObject.transform.position).normalized; //calculate direction to move
            //Debug.Log(moveDir);
            transform.position += moveDir * speed * Time.deltaTime;
        }

    }
}
