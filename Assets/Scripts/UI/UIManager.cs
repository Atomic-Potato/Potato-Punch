﻿using System;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region Inspector
    [SerializeField] TMP_Text hitPoints;
    [SerializeField] TMP_Text dashes;
    [SerializeField] TMP_Text playerState;

    [Space]
    [Header("Menus")]
    [SerializeField] GameObject menuPause;
    #endregion

    #region Global Variables
    static UIManager _instance;
    public static UIManager Instance => _instance;
    string _hitPointsText;
    string _dashesText;
    string _playerStateText;

    #endregion

    #region Execution
    void Awake() 
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _hitPointsText = hitPoints.text;  
        _dashesText = dashes.text;  
        _playerStateText = playerState.text;
    }

    void OnEnable()
    {
        Player.Instance.UpdateHitPoints += UpdateHitPoints;
        Dash.Instance.AlterDashCount += UpdateDashes;
        PlayerAnimationManager.Instance.AnimationStateAction += UpdatePlayerState;
        GameManager.Instance.PauseGame += PauseGame;
        GameManager.Instance.ResumeGame += ResumeGame;

        UpdateHitPoints();
        UpdateDashes();
        UpdatePlayerState();
    }

    void OnDisable() 
    {
        Player.Instance.UpdateHitPoints -= UpdateHitPoints;
        Dash.Instance.AlterDashCount -= UpdateDashes;
        PlayerAnimationManager.Instance.AnimationStateAction -= UpdatePlayerState;
    }

    #endregion

    #region UI Updates
    void UpdateHitPoints()
    {
        hitPoints.text = _hitPointsText + Player.Instance.HitPoints.ToString();
    }

    void UpdateDashes(int x = 0)
    {
        dashes.text = _dashesText + Dash.DashesLeft.ToString();
    }

    void UpdatePlayerState(AnimationClip x = null, bool y = false, float z = 0f)
    {
        if (PlayerAnimationManager.Instance.CurrentClip == null)
        {
            return;
        }
        playerState.text = _playerStateText + PlayerAnimationManager.Instance.CurrentClip.name;
    }
    #endregion

    #region Methods
    public void ResumeGame()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Playing)
            GameManager.Instance.ExecuteResumeGame();

        menuPause.SetActive(false);
    }

    public void PauseGame()
    {
        menuPause.SetActive(true);
    }

    public void Respawn()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
            GameManager.Instance.ResumeGame();
        Player.Instance.Respawn();
    }

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }

    #endregion
}
