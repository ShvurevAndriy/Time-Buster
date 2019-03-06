using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditorModePosition : MonoBehaviour {
    private PlayerMovement player;

    private void Update() {
        if (!Application.isPlaying) {
            if (!player) {
                player = GetComponent<PlayerMovement>();
            }
            Vector2 position2d = new Vector2(player.transform.position.x, player.transform.position.z);
            player.radius = position2d.magnitude;
            player.currentAngel = Mathf.Acos(position2d.normalized.x) * Mathf.Rad2Deg;
            if (position2d.normalized.y < 0) {
                player.currentAngel = 360 - player.currentAngel;
            }
        }
    }
}
