using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuController : MonoBehaviour {

    [SerializeField] GameObject gamePausePanel = null;
    [SerializeField] GameObject gameEndLevelPanel = null;
    [SerializeField] GameObject gameOverPanel = null;

    [SerializeField] Image currentCoin1 = null;
    [SerializeField] Image currentCoin2 = null;
    [SerializeField] Image currentCoin3 = null;

    [SerializeField] Image bestCoin1 = null;
    [SerializeField] Image bestCoin2 = null;
    [SerializeField] Image bestCoin3 = null;

    private MyGameManager gameManager;
    private int bestCoins;
    private int currentCoins;

    void Start() {
        gameManager = GetComponent<MyGameManager>();
    }

    private void Update() {
        if (Input.GetButtonDown("Cancel")) {
            Pause(!gamePausePanel.activeSelf);
        }
    }

    void OnApplicationPause(bool pause) {
        Pause(pause);
    }

    public void ShowLevelFinishedPanel() {
        Time.timeScale = 0;
        bestCoins = GamePrefController.LoadCoinsForLevel(SceneManager.GetActiveScene().buildIndex);
        currentCoins = gameManager.Coins;

        currentCoin1.color = currentCoins > 0 ? Color.yellow : Color.grey;
        currentCoin2.color = currentCoins > 1 ? Color.yellow : Color.grey;
        currentCoin3.color = currentCoins > 2 ? Color.yellow : Color.grey;

        bestCoin1.color = bestCoins > 0 ? Color.yellow : Color.grey;
        bestCoin2.color = bestCoins > 1 ? Color.yellow : Color.grey;
        bestCoin3.color = bestCoins > 2 ? Color.yellow : Color.grey;

        if (currentCoins > bestCoins) {
            GamePrefController.SaveCoinsForLevel(SceneManager.GetActiveScene().buildIndex, currentCoins);
        }

        gameEndLevelPanel.SetActive(true);
    }

    public void Pause(bool pause) {
        if (gameEndLevelPanel.activeSelf || gameOverPanel.activeSelf) {
            return;
        }
        Time.timeScale = pause ? 0 : 1;
        gamePausePanel.SetActive(pause);
    }

    public void LoadNextLevel() {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings) {
            LoadMainMenuScene();
        } else {
            SceneManager.LoadScene(nextSceneIndex);
        }
        Time.timeScale = 1;
    }

    public void Replay() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

    public void LoadMainMenuScene() {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }

    public void GameOver() {
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
    }
}
