using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public TMP_Dropdown playerOneType;
    public TMP_Dropdown playerTwoType;
    public TMP_Dropdown playerOneDifficulty;
    public TMP_Dropdown playerTwoDifficulty;

    private void Start()
    {
        playerOneType.onValueChanged.AddListener((i) => playerOneDifficulty.gameObject.SetActive((PlayerType) i == PlayerType.AI));
        playerTwoType.onValueChanged.AddListener((i) => playerTwoDifficulty.gameObject.SetActive((PlayerType) i == PlayerType.AI));
    }

    private void Update()
    {
        Game.playerOneType = playerOneType.value;
        Game.playerTwoType = playerTwoType.value;
        
        Game.playerOneDifficulty = playerOneDifficulty.value;
        Game.playerTwoDifficulty = playerTwoDifficulty.value;
    }

    public void OnStart()
    {
        SceneManager.LoadScene(1);
    }

    public void OnReturn()
    {
        SceneManager.LoadScene(0);
    }
}
