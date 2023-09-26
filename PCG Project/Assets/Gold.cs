using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : MonoBehaviour
{
    [SerializeField] private int val = 1;

    private void OnTriggerEnter2D(Collider2D other) {
  
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log(val + " added to gold total");
            GameManager.Instance.AddGold(val);
            Destroy(this.gameObject);
        }
        else
        {
            Debug.Log("Something apart from player picked up the gold");
        }
    }
}
