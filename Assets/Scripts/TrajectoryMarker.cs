using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryMarker : MonoBehaviour {
    public bool traceTragectory = true;
    public float traceMarkerRadius = 0.2f;
    public float traceTime = 3f;

    private MyGameManager gameManager;
    private MarkersPool markersPool;

    void Start() {
        gameManager = FindObjectOfType<MyGameManager>();
        markersPool = FindObjectOfType<MarkersPool>();
    }

    void FixedUpdate() {
        TraceTrajectory();
    }
    private void TraceTrajectory() {
        if (traceTragectory && gameManager.CurrentGameMode != GameMode.playbackStopped) {
            DrawSphere(transform.position, traceMarkerRadius, Color.black, traceTime);
        }
    }

    private MarkerObject DrawSphere(Vector3 position, float radius, Color color, float timeToLive = 0) {
        return markersPool.GetMarker(position, radius, color, timeToLive);
    }
}
