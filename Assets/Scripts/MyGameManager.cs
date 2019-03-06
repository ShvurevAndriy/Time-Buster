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

    public GameMode CurrentGameMode { get; private set; }

    public delegate void GameModeEvent();

    public event GameModeEvent OnPlaybackModeOn = delegate { };
    public event GameModeEvent OnPlayModeOn = delegate { };

    private int scores = 0;
    private List<CollectedItem> collectedCoins = new List<CollectedItem>();

    private void Start() {
        ChangeScore(-scores);
    }

    public void AddCollectedItem(CollectedItem item) {
        collectedCoins.Add(item);
    }

    public int ChangeScore(int diff) {
        scores = Mathf.Clamp(scores + diff, 0, int.MaxValue);
        scoreText.text = scores.ToString();
        return scores;
    }

    public void UpdateVisabilityCollectedItems(float angel) {
        collectedCoins.ForEach(i => {
            if (i.revertOnPlayback) {
                if (i.AngelWhenCollected > angel) {
                    i.SetVisible();
                } else if (i.AngelWhenCollected <= angel) {
                    i.SetInvisible();
                }
            }
        });
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
        CurrentGameMode = GameMode.playback;
        OnPlaybackModeOn();
    }

    public void StartPlayMode() {
        if (CurrentGameMode != GameMode.playback) {
            Debug.LogError("Call StartPlayMode from incorrect mode");
            return;
        }
        CurrentGameMode = GameMode.play;
        OnPlayModeOn();
    }
}
