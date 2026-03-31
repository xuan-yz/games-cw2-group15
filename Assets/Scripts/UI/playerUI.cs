using TMPro;
using Unity.Mathematics;
using UnityEditor.Rendering.Universal;
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

    public GameObject levelUpPanel;
    public GameObject gameOverPanel;
    
    [Header("Data Reference")]
    public playerStats stats; 
    void Start()
    {
        stats.ResetStats();
        UpdateUI(); 
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) TakeDamage(15);
        if (Keyboard.current.xKey.wasPressedThisFrame) GainXP(20);
    }


    
public void UpdateUI()
    {

        hpSlider.maxValue = stats.maxHP; 
        xpSlider.maxValue = stats.xpToNextLevel;

        hpSlider.value = stats.currentHP;
        xpSlider.value = stats.currentXP;

        hpText.text = stats.currentHP + " / " + stats.maxHP;
        xpText.text = stats.currentXP + " / " + stats.xpToNextLevel;
    }

    public void TakeDamage(int damage)
    {
        stats.currentHP -= damage;
        if(stats.currentHP < 0)
        {
            stats.currentHP = 0;
            TriggerGameOver();
        }

        UpdateUI();
    }

    public void GainXP(int xp)
    {
        stats.currentXP += xp;

        if(stats.currentXP >= stats.xpToNextLevel)
        {
            stats.currentXP -= stats.xpToNextLevel; //leftover xp will carry across
            stats.currentLevel++;
            stats.xpToNextLevel = (int)math.round(stats.xpToNextLevel*1.25); //increase next level requirements

            //re-set max xp to new one and set to maxhp on ding
            xpSlider.maxValue = stats.xpToNextLevel;
            stats.currentHP = stats.maxHP;
            TriggerLevelUp();
        }

        UpdateUI();
    }

    void TriggerGameOver()
    {
        gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
    }

    void TriggerLevelUp()
    {
        levelUpPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
    }

    public void IncreaseDamage(){
        stats.damage += 5;
        Time.timeScale = 1;
        levelUpPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        UpdateUI();
    }

    public void IncreaseHP(){
        stats.maxHP += 20;
        stats.currentHP += 20;
        Time.timeScale = 1;
        levelUpPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        UpdateUI();
    }

    public void IncreaseMS(){
        stats.moveSpeed += 5;
        Time.timeScale = 1;
        levelUpPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        UpdateUI();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
