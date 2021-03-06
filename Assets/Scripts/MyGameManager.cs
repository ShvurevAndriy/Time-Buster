﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum GameMode {
    play,
    playback
}

public class MyGameManager : MonoBehaviour {

    [SerializeField] Text scoreText = null;
    [SerializeField] Text parachutesText = null;
    [SerializeField] Text plannersText = null;
    [SerializeField] Text replayCountText = null;
    [SerializeField] Text replayText = null;
    [SerializeField] Button replayButton = null;
    [SerializeField] ProgressBar jetpackFuelBar = null;
    [SerializeField] ProgressBar recordBar = null;

    [SerializeField] float jetPackFuel = 0;
    [SerializeField] int parachutes = 0;
    [SerializeField] int planners = 0;
    [SerializeField] int replays = 10;

    public GameMode CurrentGameMode { get; private set; }
    public float JetPackFuel { get => jetPackFuel; set => jetPackFuel = value; }
    public int Parachutes { get => parachutes; set => parachutes = value; }
    public int Planners { get => planners; set => planners = value; }
    public int Coins { get; private set; } = 0;
    public int Replays { get => replays; set => replays = value; }

    public delegate void GameModeEvent();

    public event GameModeEvent OnPlaybackModeOn = delegate { };
    public event GameModeEvent OnPlayModeOn = delegate { };

    private JetpackConfiguration jetpackConfiguration;
    private GameMenuController gameMenuController;
    private List<CollectedItem> collectedCoins = new List<CollectedItem>();

    void Start() {
        replayText.enabled = false;
        jetpackConfiguration = GetComponent<JetpackConfiguration>();
        gameMenuController = GetComponent<GameMenuController>();
        UpdateCounters();
        if (Replays <= 0) {
            replayButton.interactable = false;
        }
    }

    private void UpdateCounters() {
        scoreText.text = Coins.ToString();
        parachutesText.text = Parachutes.ToString();
        plannersText.text = Planners.ToString();
        replayCountText.text = Replays.ToString();
        jetpackFuelBar.BarValue = Mathf.RoundToInt(JetPackFuel / jetpackConfiguration.MaxJetPackFuel * 100f);
    }

    public void AddCollectedItem(CollectedItem item) {
        collectedCoins.Add(item);
    }

    public void ChangeParachutes(int diff) {
        Parachutes = Mathf.Clamp(diff, 0, int.MaxValue);
        parachutesText.text = Parachutes.ToString();
    }

    public bool UseParachute() {
        if (Parachutes > 0) {
            Parachutes--;
            parachutesText.text = Parachutes.ToString();
            return true;
        }
        return false; ;
    }

    public void SetRecordRatio(float percentage) {
        recordBar.BarValue = Mathf.RoundToInt(percentage * 100f);
    }

    public bool HasParachute() {
        return Parachutes > 0;
    }

    public void ChangePlanners(int diff) {
        Planners = Mathf.Clamp(diff, 0, int.MaxValue);
        plannersText.text = Planners.ToString();
    }

    public bool UsePlanner() {
        if (Planners > 0) {
            Planners--;
            plannersText.text = Planners.ToString();
            return true;
        }
        return false; ;
    }

    public bool HasPlanner() {
        return Planners > 0;
    }

    public bool HasFuel() {
        return JetPackFuel > 0;
    }

    private bool UseReplay() {
        if (Replays > 0) {
            Replays--;
            replayCountText.text = Replays.ToString();
            return true;
        }
        return false;
    }

    public float ChangeJetpackFuel(float diff) {

        float tankFreeCapacity = jetpackConfiguration.MaxJetPackFuel - JetPackFuel;

        JetPackFuel = Mathf.Clamp(JetPackFuel + diff, 0, jetpackConfiguration.MaxJetPackFuel);
        jetpackFuelBar.BarValue = Mathf.RoundToInt(JetPackFuel / jetpackConfiguration.MaxJetPackFuel * 100f);

        if (diff < 0) {
            return JetPackFuel < Mathf.Abs(diff) ? JetPackFuel : diff;
        } else {
            return tankFreeCapacity < diff ? tankFreeCapacity : diff;
        }
    }

    public void ChangeCoins(int diff) {
        Coins = Mathf.Clamp(Coins + diff, 0, int.MaxValue);
        scoreText.text = Coins.ToString();
    }

    public void ActualizeGameDataForPlayback(float angel) {
        collectedCoins.ForEach(i => {
            if (i.revertOnPlayback) {
                if (i.AngelWhenCollected > angel) {
                    i.SetVisible();
                } else if (i.AngelWhenCollected <= angel) {
                    i.SetInvisible();
                }
            }
        });
        UpdateCounters();
    }

    public void DeactivateCollectedItemsAfterAngel(float angel) {
        collectedCoins.Where(i => i.revertOnPlayback && i.AngelWhenCollected > angel).ToList().ForEach(i => i.UnCollectItem());
        collectedCoins = collectedCoins.Where(i => !i.revertOnPlayback || i.AngelWhenCollected <= angel).ToList();
    }

    public void StartPlaybackMode() {
        if (CurrentGameMode != GameMode.play) {
            Debug.LogError("Call StartPlaybackMode from incorrect mode");
            return;
        }
        if (UseReplay()) {
            replayButton.interactable = false;
            replayText.enabled = true;
            CurrentGameMode = GameMode.playback;
            OnPlaybackModeOn();
        } else {
            gameMenuController.GameOver();
        }

    }

    public void StartPlayMode() {
        if (CurrentGameMode != GameMode.playback) {
            Debug.LogError("Call StartPlayMode from incorrect mode");
            return;
        }
        if (Replays > 0) {
            replayButton.interactable = true;
        }
        replayText.enabled = false;
        CurrentGameMode = GameMode.play;
        OnPlayModeOn();
    }

    public void LevelFinished() {
        gameMenuController.ShowLevelFinishedPanel();
    }

    public void ActivatePlayback() {
        if (replayButton.IsInteractable()) {
            StartPlaybackMode();
        }
    }
}