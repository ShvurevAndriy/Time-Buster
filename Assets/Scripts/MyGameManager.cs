using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum GameMode {
    play,
    playback
}

public class MyGameManager : MonoBehaviour {

    [SerializeField] Image[] coinsImages = new Image[3];
    [SerializeField] Image[] rewindsImages = new Image[20];
    [SerializeField] Image fuelForegroundImage = null;

    [SerializeField] Text replayText = null;
    [SerializeField] Button replayButton = null;

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
        UpdateCoinsIndicator();

        UpdateRewindsIndicator();

        fuelForegroundImage.transform.localScale = new Vector3(1, JetPackFuel / jetpackConfiguration.MaxJetPackFuel, 1);
    }

    private void UpdateRewindsIndicator() {
        for (int i = 0; i < rewindsImages.Length; i++) {
            rewindsImages[i].enabled = i < Replays;
        }
    }

    private void UpdateCoinsIndicator() {
        for (int i = 0; i < coinsImages.Length; i++) {
            coinsImages[i].color = i < Coins ? Color.yellow : Color.grey;
        }
    }

    public void AddCollectedItem(CollectedItem item) {
        collectedCoins.Add(item);
    }

    public void ChangeParachutes(int diff) {
        Parachutes = Mathf.Clamp(diff, 0, int.MaxValue);
    }

    public bool UseParachute() {
        if (Parachutes > 0) {
            Parachutes--;
            return true;
        }
        return false; ;
    }

    public bool HasParachute() {
        return Parachutes > 0;
    }

    public void ChangePlanners(int diff) {
        Planners = Mathf.Clamp(diff, 0, int.MaxValue);
    }

    public bool UsePlanner() {
        if (Planners > 0) {
            Planners--;
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
            UpdateRewindsIndicator();
            return true;
        }
        return false;
    }

    public float ChangeJetpackFuel(float diff) {

        float tankFreeCapacity = jetpackConfiguration.MaxJetPackFuel - JetPackFuel;

        JetPackFuel = Mathf.Clamp(JetPackFuel + diff, 0, jetpackConfiguration.MaxJetPackFuel);
        fuelForegroundImage.transform.localScale = new Vector3(1, JetPackFuel / jetpackConfiguration.MaxJetPackFuel, 1);

        if (diff < 0) {
            return JetPackFuel < Mathf.Abs(diff) ? JetPackFuel : diff;
        } else {
            return tankFreeCapacity < diff ? tankFreeCapacity : diff;
        }
    }

    public void ChangeCoins(int diff) {
        Coins = Mathf.Clamp(Coins + diff, 0, int.MaxValue);
        UpdateCoinsIndicator();
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