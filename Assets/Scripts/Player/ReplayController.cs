using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityStandardAssets.CrossPlatformInput;

public class ReplayController : MonoBehaviour {
    [SerializeField] int replaySize = 100;
    [SerializeField] float pathMarkerScale = 0.2f;
    [SerializeField] Color markerColor = Color.red;
    [SerializeField] Color pathColor = Color.black;
    [SerializeField] bool markReplayForceJumpPoint = true;
    [SerializeField] float frameFactor = 100;

    private Queue<ReplayData> positionRecord;
    private List<ReplayData> playbackData;
    private BoxCollider boxCollider;
    private TrajectoryMarker trajectoryMarker;
    private MarkersPool markersPool;
    private MyGameManager gameManger;
    private PlayerMovement playerMovement;
    private PlayerStateController stateController;
    private CameraFollow cameraController;
    private Rigidbody rigidBody;
    private Dictionary<float, MarkerObject> forceMarkers = new Dictionary<float, MarkerObject>();
    private Dictionary<float, MarkerObject> forceMarkersToDelete = new Dictionary<float, MarkerObject>();
    private List<MarkerObject> path = new List<MarkerObject>();
    private bool activeTurnOffPlaybackMode = false;

    private int currentFrame;

    private ReplayData currentReplayData;

    void Start() {
        cameraController = FindObjectOfType<CameraFollow>();
        markersPool = FindObjectOfType<MarkersPool>();
        gameManger = FindObjectOfType<MyGameManager>();
        gameManger.OnPlaybackModeOn += OnPlaybackModeOn;
        gameManger.OnPlayModeOn += OnPlayModeOn;
        rigidBody = GetComponent<Rigidbody>();
        trajectoryMarker = GetComponent<TrajectoryMarker>();
        playerMovement = GetComponent<PlayerMovement>();
        stateController = GetComponent<PlayerStateController>();
        boxCollider = GetComponent<BoxCollider>();
        positionRecord = new Queue<ReplayData>();
    }

    void FixedUpdate() {
        if (gameManger.CurrentGameMode == GameMode.play) {
            DoRecord();
            gameManger.SetRecordRatio((float)positionRecord.Count / replaySize);
        }
    }

    void Update() {
        if (gameManger.CurrentGameMode == GameMode.playback) {
            ProcessUserInput();
            gameManger.SetRecordRatio((float)currentFrame / replaySize);
        }
    }

    private void ProcessUserInput() {
        int oldFrame = currentFrame;

#if (UNITY_STANDALONE || UNITY_EDITOR)
        currentFrame = Mathf.Clamp(Mathf.RoundToInt(currentFrame + CrossPlatformInputManager.GetAxis("Mouse X") * frameFactor), 0, playbackData.Count - 1);
#endif

#if !(UNITY_STANDALONE || UNITY_EDITOR)
        if (Input.touchCount > 0) {
            currentFrame = Mathf.Clamp(Mathf.RoundToInt(currentFrame + Input.touches[0].deltaPosition.x * frameFactor), 0, playbackData.Count - 1);
        }
#endif

        if (currentFrame != oldFrame) {
            DoPlayBack(currentFrame);
        }

        if (CrossPlatformInputManager.GetButtonDown("Jump") && activeTurnOffPlaybackMode) {
            gameManger.StartPlayMode();
            activeTurnOffPlaybackMode = false;
        }

        if (CrossPlatformInputManager.GetButtonUp("Jump")){
            activeTurnOffPlaybackMode = true;
        }
    }

    private void OnPlaybackModeOn() {
        activeTurnOffPlaybackMode = !CrossPlatformInputManager.GetButtonDown("Jump");
        rigidBody.detectCollisions = false;
        rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rigidBody.isKinematic = true;
        rigidBody.velocity = Vector3.zero;
        RemoveUnactualForceMarkers();

        if (markReplayForceJumpPoint) {
            foreach (float key in trajectoryMarker.ForceJumpMarkers.Keys) {
                MarkerObject markerObject = trajectoryMarker.ForceJumpMarkers[key];
                markerObject.SetColor(markerColor);
                forceMarkers.Add(key, trajectoryMarker.ForceJumpMarkers[key]);
            }
            trajectoryMarker.ForceJumpMarkers.Clear();
        } else {
            trajectoryMarker.ClearAndDestroyForceMarkers();
        }

        ReplayData replayData = positionRecord.Peek();
        while (Physics.BoxCast(replayData.position, boxCollider.bounds.extents, Vector3.zero, Quaternion.Euler(0, 90 - replayData.currentAngel, 0), 0)) {
            replayData = positionRecord.Dequeue();
        }
        playbackData = new List<ReplayData>(positionRecord);
        DrawPath();
        currentFrame = playbackData.Count - 1;
        DoPlayBack(currentFrame);
    }

    private void OnPlayModeOn() {
        rigidBody.detectCollisions = true;
        rigidBody.isKinematic = false;
        rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        positionRecord = new Queue<ReplayData>(playbackData.GetRange(0, currentFrame));
        path.ForEach(m => m.DestroyNow());
        path.Clear();
        gameManger.DeactivateCollectedItemsAfterAngel(playerMovement.CurrentAngel);
        forceMarkers.Keys.Where(k => k > playerMovement.CurrentAngel).ToList()
            .ForEach(k => {
                MarkerObject markerObject = forceMarkers[k];
                forceMarkersToDelete.Add(k, markerObject);
                forceMarkers.Remove(k);
            });
        DoRecord();
    }

    private void DrawPath() {
        foreach (ReplayData replayData in playbackData) {
            path.Add(markersPool.GetMarker(replayData.position, pathMarkerScale, pathColor, 0));
        }
    }

    private void RemoveUnactualForceMarkers() {
        forceMarkersToDelete.Values.ToList().ForEach(m => m.DestroyNow());
        forceMarkersToDelete.Clear();
    }

    private void DoRecord() {
        ReplayData replayData = ReplayData.builder()
            .setPosition(transform.position)
            .setYVelocity(playerMovement.YVelocity)
            .setTimeScale(playerMovement.TimeScale)
            .setCurrentAngel(playerMovement.CurrentAngel)
            .setAngularSpeed(playerMovement.AngularSpeed)
            .setJumpHeight(playerMovement.CurrentJumpHeight)
            .setJumpState(stateController.CurrentJumpState)
            .setStartBoostYPos(playerMovement.StartBoostYPos)
            .setApexYPos(playerMovement.ApexYPos)
            .setCameraZoom(cameraController.CameraSize)
            .setJetpackFuel(gameManger.JetPackFuel)
            .setParachutes(gameManger.Parachutes)
            .setPlanners(gameManger.Planners)
            .build();
        positionRecord.Enqueue(replayData);
        if (positionRecord.Count > replaySize) {
            replayData = positionRecord.Dequeue();
            ClearForceMarkerBehind(forceMarkers, replayData.currentAngel);
            ClearForceMarkerBehind(forceMarkersToDelete, replayData.currentAngel);
        }
    }

    private void ClearForceMarkerBehind(Dictionary<float, MarkerObject> markers, float currentAngel) {
        markers.Keys.Where(k => k < currentAngel).ToList()
            .ForEach(k => {
                MarkerObject markerObject = markers[k];
                forceMarkers.Remove(k);
                markerObject.DestroyNow();
            });
    }

    private void DoPlayBack(int frame) {
        currentReplayData = playbackData[frame];
        transform.position = currentReplayData.position;
        cameraController.CameraSize = currentReplayData.cameraZoom;
        playerMovement.CurrentAngel = currentReplayData.currentAngel;
        gameManger.JetPackFuel = currentReplayData.jetpackFuel;
        gameManger.Parachutes = currentReplayData.parachutes;
        gameManger.Planners = currentReplayData.planners;
        gameManger.ActualizeGameDataForPlayback(currentReplayData.currentAngel);

        playerMovement.CurrentJumpHeight = currentReplayData.jumpHeight;
        stateController.CurrentJumpState = currentReplayData.jumpState;
        playerMovement.StartBoostYPos = currentReplayData.startBoostYPos;
        playerMovement.ApexYPos = currentReplayData.apexYPos;
        playerMovement.YVelocity = currentReplayData.yVelocity;
        playerMovement.TimeScale = currentReplayData.timeScale;
        playerMovement.AngularSpeed = currentReplayData.angularSpeed;
    }
}
