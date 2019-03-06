using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(EditorModePosition))]
public class PlayerEditor : Editor {

    private PlayerMovement player;

    void OnSceneGUI() {
        if (!Application.isPlaying) {
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0) {
                if (!player) {
                    player = FindObjectOfType<PlayerMovement>();
                }
                player.SetAppropriateRotation();
            }
        }
    }
}
