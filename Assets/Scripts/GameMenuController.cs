using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuController : MonoBehaviour {

    [SerializeField] GameObject gamePausePanel = null;
    [SerializeField] GameObject gameEndLevelPanel = null;
    [SerializeField] Text endLevelScoreText = null;

    private MyGameManager gameManager;

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
        endLevelScoreText.text = gameManager.Scores.ToString();
        gameEndLevelPanel.SetActive(true);
    }

    public void Pause(bool pause) {
        if (gameEndLevelPanel.activeSelf) {
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
}
