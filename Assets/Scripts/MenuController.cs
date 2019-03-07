using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    [SerializeField] Dropdown levels = null;

    void Start() {
        levels.ClearOptions();
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
            if (scene.enabled && scene.path != SceneManager.GetActiveScene().path) {
                scenes.Add(System.IO.Path.GetFileNameWithoutExtension(scene.path));
            }
        }
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
