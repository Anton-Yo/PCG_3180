using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackUp : MonoBehaviour
{
    
    [SerializeField] private int attackIncrease; 

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.Instance.IncreaseDmg(attackIncrease);
            Destroy(this.gameObject);
        }
    }
}
