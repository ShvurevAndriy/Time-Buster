using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ReplayController : MonoBehaviour {
    [SerializeField] int replaySize = 100;
    [SerializeField] Text label = null;
    [SerializeField] ProgressBar recordBar = null;
    [SerializeField] float pathMarkerScale = 0.2f;
    [SerializeField] Color markerColor = Color.red;
    [SerializeField] Color pathColor = Color.black;
    [SerializeField] bool markReplayForceJumpPoint = true;
    [SerializeField] int frameFactor = 100;
    [SerializeField] bool mobileStylePlayback = false;

    private Queue<ReplayData> positionRecord;
    private List<ReplayData> playbackData;
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

    private float mouseStartXPos;
    private int lastSelected;
    private int currentFrame;

    private ReplayData currentReplayData;

    public bool MobileStylePlayback { get => mobileStylePlayback; private set => mobileStylePlayback = value; }

    void Start() {
        label.enabled = false;
        cameraController = FindObjectOfType<CameraFollow>();
        markersPool = FindObjectOfType<MarkersPool>();
        gameManger = FindObjectOfType<MyGameManager>();
        gameManger.OnPlaybackModeOn += OnPlaybackModeOn;
        gameManger.OnPlayModeOn += OnPlayModeOn;
        rigidBody = GetComponent<Rigidbody>();
        trajectoryMarker = GetComponent<TrajectoryMarker>();
        playerMovement = GetComponent<PlayerMovement>();
        stateController = GetComponent<PlayerStateController>();
        positionRecord = new Queue<ReplayData>();
    }

    void FixedUpdate() {
        if (gameManger.CurrentGameMode == GameMode.play) {
            DoRecord();
            recordBar.BarValue = Mathf.RoundToInt((float)positionRecord.Count / replaySize * 100f);
        }
    }

    void Update() {
        if (gameManger.CurrentGameMode == GameMode.playback) {
            ProcessUserInput();
            recordBar.BarValue = Mathf.RoundToInt((float)currentFrame / replaySize * 100f);
        }
    }

    private void ProcessUserInput() {
        if (mobileStylePlayback) {
            if (Input.GetMouseButtonUp(1)) {
                mouseStartXPos = float.NegativeInfinity;
                lastSelected = currentFrame;
            }
            if (Input.GetMouseButtonDown(1)) {
                mouseStartXPos = Camera.main.ScreenToViewportPoint(Input.mousePosition).x;
            }
            if (Input.GetMouseButton(1)) {
                if (!float.IsInfinity(mouseStartXPos)) {
                    float currentX = Camera.main.ScreenToViewportPoint(Input.mousePosition).x;
                    int frameDiff = Mathf.RoundToInt((currentX - mouseStartXPos) * frameFactor);
                    currentFrame = Mathf.Clamp(lastSelected - frameDiff, 0, playbackData.Count - 1);
                    DoPlayBack(currentFrame);
                }
            }
        } else {
            mouseStartXPos = 0;
            float frameFactor = playbackData.Count;
            float currentX = Camera.main.ScreenToViewportPoint(Input.mousePosition).x;
            int frameDiff = Mathf.RoundToInt((currentX - mouseStartXPos) * frameFactor);
            currentFrame = Mathf.Clamp(lastSelected - frameDiff, 0, playbackData.Count - 1);
            DoPlayBack(currentFrame);
        }

        if (Input.GetMouseButtonDown(0)) {
            PreparePlayMode();
            gameManger.StartPlayMode();
        }
    }

    private void OnPlaybackModeOn() {
        mouseStartXPos = float.NegativeInfinity;
        label.enabled = true;
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
        positionRecord.Dequeue();
        playbackData = new List<ReplayData>(positionRecord);
        DrawPath();
        lastSelected = playbackData.Count - 1;
        currentFrame = lastSelected;
        DoPlayBack(lastSelected);
    }

    private void OnPlayModeOn() {
        lastSelected = currentFrame;
        label.enabled = false;
        rigidBody.detectCollisions = true;
        rigidBody.isKinematic = false;
        rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        positionRecord = new Queue<ReplayData>(playbackData.GetRange(0, lastSelected));
        path.ForEach(m => m.DestroyNow());
        path.Clear();
        gameManger.DeactivateCollectedItemsAfterAngel(playerMovement.currentAngel);
        forceMarkers.Keys.Where(k => k > playerMovement.currentAngel).ToList()
            .ForEach(k => {
                MarkerObject markerObject = forceMarkers[k];
                forceMarkersToDelete.Add(k, markerObject);
                forceMarkers.Remove(k);
            });
        DoRecord();
    }

    private void DrawPath() {
        foreach (ReplayData replayData in playbackData) {
            path.Add(markersPool.GetMarker(replayData.posotion, pathMarkerScale, pathColor, 0));
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
            .setCurrentAngel(playerMovement.currentAngel)
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
        transform.position = currentReplayData.posotion;
        cameraController.CameraSize = currentReplayData.cameraZoom;
        playerMovement.currentAngel = currentReplayData.currentAngel;
        gameManger.JetPackFuel = currentReplayData.jetpackFuel;
        gameManger.Parachutes = currentReplayData.parachutes;
        gameManger.Planners = currentReplayData.planners;
        gameManger.ActualizeGameDataForPlayback(currentReplayData.currentAngel);
    }

    private void PreparePlayMode() {
        playerMovement.CurrentJumpHeight = currentReplayData.jumpHeight;
        stateController.CurrentJumpState = currentReplayData.jumpState;
        playerMovement.StartBoostYPos = currentReplayData.startBoostYPos;
        playerMovement.ApexYPos = currentReplayData.apexYPos;
        playerMovement.YVelocity = currentReplayData.yVelocity;
        playerMovement.TimeScale = currentReplayData.timeScale;
        playerMovement.AngularSpeed = currentReplayData.angularSpeed;
    }
}
