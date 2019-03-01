using UnityEngine;

public enum GameMode {
    play,
    playback,
    playbackStopped
}

public class MyGameManager : MonoBehaviour {

    public GameMode CurrentGameMode { get; private set; }

    public delegate void GameModeEvent();

    public event GameModeEvent OnPlaybackModeOn = delegate { };
    public event GameModeEvent OnPlayModeOn = delegate { };
    public event GameModeEvent OnStopPlaybackMode = delegate { };

    public void StartPlaybackMode() {
        if (CurrentGameMode != GameMode.play) {
            Debug.LogError("Call StartPlaybackMode from incorrect mode");
            return;
        }
        CurrentGameMode = GameMode.playback;
        OnPlaybackModeOn();
    }

    public void StopPlaybackMode() {
        if (CurrentGameMode != GameMode.playback) {
            Debug.LogError("Call EndPlaybackMode from incorrect mode");
            return;
        }
        CurrentGameMode = GameMode.playbackStopped;
        OnStopPlaybackMode();
    }

    public void StartPlayMode() {
        if (CurrentGameMode != GameMode.playbackStopped) {
            Debug.LogError("Call StartPlayMode from incorrect mode");
            return;
        }
        CurrentGameMode = GameMode.play;
        OnPlayModeOn();
    }
}
