using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryMarker : MonoBehaviour {
    public bool traceTragectory = true;
    public float traceMarkerRadius = 0.2f;
    public float traceTime = 3f;
    public bool markForceJumpPoint = true;
    public bool putForceJumpMarker;
    public Color forceMarkerColor = Color.grey;
    public float forceMarkerScale = 1;
    public float angelToDeletForceMarker = 180;

    private MyGameManager gameManager;
    private MarkersPool markersPool;
    private ReplayController replayController;
    private PlayerMovement playerMovement;

    public Dictionary<float, MarkerObject> ForceJumpMarkers { get; set; } = new Dictionary<float, MarkerObject>();

    void Start() {
        gameManager = FindObjectOfType<MyGameManager>();
        markersPool = FindObjectOfType<MarkersPool>();
        replayController = FindObjectOfType<ReplayController>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerMovement.OnPlayerJump += OnJump;
    }

    void FixedUpdate() {
        TraceTrajectory();

        if (markForceJumpPoint && !putForceJumpMarker && IsReachStartBoostY()) {
            Vector3 markerPos = transform.position;
            ForceJumpMarkers.Add(playerMovement.currentAngel, DrawSphere(markerPos, forceMarkerScale, forceMarkerColor));
            putForceJumpMarker = true;
        }

        ForceJumpMarkers.Keys.Where(k => k < playerMovement.currentAngel - angelToDeletForceMarker).ToList()
            .ForEach(k => {
                ForceJumpMarkers[k].DestroyNow();
                ForceJumpMarkers.Remove(k);
            });
    }

    public void ClearAndDestroyForceMarkers() {
        foreach (MarkerObject marker in ForceJumpMarkers.Values) {
            marker.DestroyNow();
        }
        ForceJumpMarkers.Clear();
    }

    private void OnJump() {
        putForceJumpMarker = false;
    }

    private bool IsReachStartBoostY() {
        return !float.IsInfinity(playerMovement.StartBoostYPos) && transform.position.y <= playerMovement.StartBoostYPos && playerMovement.YVelocity <= 0;
    }

    private void TraceTrajectory() {
        if (traceTragectory && gameManager.CurrentGameMode != GameMode.playback) {
            DrawSphere(transform.position, traceMarkerRadius, Color.black, traceTime);
        }
    }

    private MarkerObject DrawSphere(Vector3 position, float radius, Color color, float timeToLive = 0) {
        return markersPool.GetMarker(position, radius, color, timeToLive);
    }
}
