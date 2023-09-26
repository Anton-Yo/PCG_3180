using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    private int goldAmount;
    [SerializeField] private int maxHealth;
    private int playerHealth;

    [SerializeField] private Player player;

    private int playerDmg = 1;
    public int PlayerDmg
    {
        get => playerDmg;
    }

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
        if(player == null)
        {
            player = GameObject.Find("Player").GetComponent<Player>();
        }

        playerHealth = maxHealth;
        UI.Instance.SetupGameUI(maxHealth, goldAmount);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(goldAmount);
    }

    public void AddGold(int amount)
    {
        goldAmount += amount;
        UI.Instance.IncreaseGold(goldAmount);
    }

    public void Damage(int amount)
    {
        playerHealth -= amount;
        UI.Instance.UpdateHealthBar(playerHealth);
    }

    public void IncreaseDmg(int amount)
    {
        playerDmg += amount;
    }

    public void IncreaseHealth(int amount)
    {
        playerHealth += amount;
        UI.Instance.UpdateHealthBar(playerHealth);
    }
}
