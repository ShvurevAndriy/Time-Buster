using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    [SerializeField] Dropdown levels = null;
    public List<string> scenes;

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
        scenes.Add("Main Menu");
        scenes.Add("Test Level");
        #endif

        levels.AddOptions(scenes);
        levels.value = 0;
    }

    public void Play() {
        SceneManager.LoadScene(levels.options[levels.value].text);
    }

    public void Quit() {
        Application.Quit();
    }
}
