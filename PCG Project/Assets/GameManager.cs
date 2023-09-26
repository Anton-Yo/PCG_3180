using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    private int goldAmount;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            Debug.Log("Too many game managers in the scene");
        }
        else
        {
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(goldAmount);
    }

    public void AddGold(int amount)
    {
        goldAmount += amount;
    }
}
