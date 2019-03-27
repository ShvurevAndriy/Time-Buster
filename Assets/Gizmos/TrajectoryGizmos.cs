using System;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TrajectoryGizmos : MonoBehaviour {
    public float step = 0.2f;
    public Color lineColor = new Color(1, 0, 0, 0.2f);
    public Color circleColor = Color.red;

    private PlayerMovement player;

    void OnDrawGizmos() {
        if (!Application.isPlaying) {
            if (!player) {
                player = FindObjectOfType<PlayerMovement>();
                if (!player) {
                    return;
                }
            }
            Vector3 lastPos = new Vector3(Mathf.Cos(0) * player.Radius, player.transform.position.y, Mathf.Sin(0) * player.Radius);
            for (float angel = step; angel < 360.0f; angel += step) {
                Vector3 currentVector = DrawGizmos(lastPos, angel);
                lastPos = currentVector;
            }
            DrawGizmos(lastPos, 0);

        }
    }

    private Vector3 DrawGizmos(Vector3 lastPos, float angel) {
        Vector3 currentVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angel) * player.Radius, player.transform.position.y, Mathf.Sin(Mathf.Deg2Rad * angel) * player.Radius);
        Gizmos.color = circleColor;
        Gizmos.DrawLine(lastPos, currentVector);
        Gizmos.color = lineColor;
        Gizmos.DrawLine(lastPos, new Vector3(lastPos.x, 0, lastPos.z));
        return currentVector;
    }
}
