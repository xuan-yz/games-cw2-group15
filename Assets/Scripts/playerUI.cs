using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class playerUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("UI References")]
    public Slider hpSlider;
    public Slider xpSlider;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI xpText;
    
    [Header("Menus")]
    public GameObject gameOverPanel;
    public GameObject levelUpPanel;

    [Header("Player Stats")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Level Stats")]
    public int currentLevel = 1;
    public int currentXP = 0;
    int xpToNextLevel = 100;

    void Start()
    {
        currentHP = maxHP;

        hpSlider.maxValue = maxHP;
        xpSlider.maxValue = xpToNextLevel;

        hpText.text = currentHP.ToString() + " / " + maxHP.ToString();
        xpText.text = currentXP.ToString() + " / " + xpToNextLevel.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        //if (Keyboard.current.spaceKey.wasPressedThisFrame) TakeDamage(15);
        if (Keyboard.current.xKey.wasPressedThisFrame) GainXP(20);
    }


    
    public void UpdateUI()
    {
        hpSlider.value = currentHP;
        xpSlider.value = currentXP;

        hpText.text = currentHP.ToString() + " / " + maxHP.ToString();
        xpText.text = currentXP.ToString() + " / " + xpToNextLevel.ToString();

    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if(currentHP < 0)
        {
            currentHP = 0;
            TriggerGameOver();
            //end
        }

        UpdateUI();
    }

    public void GainXP(int xp)
    {
        currentXP += xp;

        if(currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel; //leftover xp will carry across
            currentLevel++;
            xpToNextLevel = (int)math.round(xpToNextLevel*1.25); //increase next level requirements

            //re-set max xp to new one and set to maxhp on ding
            xpSlider.maxValue = xpToNextLevel;
            currentHP = maxHP;
            TriggerLevelUp();
        }

        UpdateUI();
    }

    void TriggerGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    void TriggerLevelUp()
    {
        levelUpPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
