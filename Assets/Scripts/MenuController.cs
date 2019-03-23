using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    [SerializeField] Dropdown levels = null;
    [SerializeField] Image coin1 = null;
    [SerializeField] Image coin2 = null;
    [SerializeField] Image coin3 = null;
    [SerializeField] Button playButton = null;

    [SerializeField] Text totalCoinsText = null;
    [SerializeField] Text neededCoinsText = null;

    [SerializeField] int[] levelsRequrement = null;

    private List<string> scenes;
    private int totalCoins;

    void Start() {
        levels.ClearOptions();
        scenes = new List<string>();

#if UNITY_EDITOR
        foreach (UnityEditor.EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes) {
            if (scene.enabled && scene.path != SceneManager.GetActiveScene().path) {
                scenes.Add(System.IO.Path.GetFileNameWithoutExtension(scene.path));
            }
        }
#endif

#if !UNITY_EDITOR
        scenes.Add("Level 1");
        scenes.Add("Level 2");
        scenes.Add("Level 3");
        scenes.Add("Level 4");
        scenes.Add("Jetpack level 1");
        scenes.Add("Jetpack level 2");
        scenes.Add("Bonus level");
        scenes.Add("Jetpack bounus level");
        scenes.Add("Secret level");
#endif

        levels.AddOptions(scenes);
        levels.value = 0;
        UpdateCollectedCoinsIcons(levels.value);
        UpdateCoinsStatsForLevel(levels.value);
        totalCoins = GamePrefController.GetTotalCoinsForLevels(levels.options.Count);
        totalCoinsText.text = totalCoins.ToString();

        if (levelsRequrement != null) {
            for (int i = 0; i < levelsRequrement.Length; i++) {
                GamePrefController.SaveLevelRequrement(i + 1, levelsRequrement[i]);
            }
        }
    }

    public void OnDropDownChange(int option) {
        UpdateCollectedCoinsIcons(option);
        UpdateCoinsStatsForLevel(option);

    }

    public void ResetProgress() {
        GamePrefController.ClearCoinsForLevels(levels.options.Count);
        UpdateCollectedCoinsIcons(levels.value);
        totalCoins = GamePrefController.GetTotalCoinsForLevels(levels.options.Count);
        totalCoinsText.text = totalCoins.ToString();
    }

    public void Play() {
        SceneManager.LoadScene(levels.options[levels.value].text);
    }

    public void Quit() {
        Application.Quit();
    }

    private void UpdateCollectedCoinsIcons(int level) {
        int coins = GamePrefController.LoadCoinsForLevel(level + 1);
        coin1.color = coins > 0 ? Color.yellow : Color.grey;
        coin2.color = coins > 1 ? Color.yellow : Color.grey;
        coin3.color = coins > 2 ? Color.yellow : Color.grey;
    }

    private void UpdateCoinsStatsForLevel(int level) {
        int neededCoins = GamePrefController.LoadLevelRequrement(level + 1);
        neededCoinsText.text = neededCoins.ToString();
        playButton.interactable = neededCoins <= totalCoins;
    }
}
