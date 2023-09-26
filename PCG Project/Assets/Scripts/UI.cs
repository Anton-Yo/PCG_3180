using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class UI : MonoBehaviour
{

    public static UI Instance {get; private set;}
    [SerializeField] private HealthBar healthBar;

    [SerializeField] private TMP_Text gold;

    private string goldMsg = "Gold: ";

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

    public void SetupGameUI(int maxHealth, int goldVal)
    {
        healthBar.SetMaxHealth(maxHealth);
        gold.text = goldMsg + goldVal;
    }
    public void UpdateHealthBar(int health)
    {
        healthBar.SetHealth(health);
    }

    public void IncreaseGold(int goldVal)
    {
        gold.text = goldMsg + goldVal;
    }
}
