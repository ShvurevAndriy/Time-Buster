using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayController : MonoBehaviour {
    [SerializeField] int replaySize = 100;
    [SerializeField] Text label = null;
    [SerializeField] ProgressBar recordBar = null;
    [SerializeField] float forceMarkerScale = 1f;
    [SerializeField] Color markerColor = Color.red;
    [SerializeField] bool markReplayForceJumpPoint = true;

    private Queue<ReplayData> positionRecord;
    private Stack<ReplayData> playbackData;
    private List<Vector3> forceHeightPos = new List<Vector3>();
    private MyGameManager gameManger;
    private PlayerMovement playerMovement;
    private PlayerStateController stateController;
    private CameraFollow cameraController;
    private MarkersPool markersPool;
    private Rigidbody rigidBody;
    private List<MarkerObject> forceMarkers = new List<MarkerObject>();
    private List<MarkerObject> forceMarkersToDelete = new List<MarkerObject>();
    private bool stopTrigger;

    private ReplayData currentReplayData;


    void Start() {
        label.enabled = false;
        cameraController = FindObjectOfType<CameraFollow>();
        markersPool = FindObjectOfType<MarkersPool>();
        gameManger = FindObjectOfType<MyGameManager>();
        gameManger.OnPlaybackModeOn += OnPlaybackModeOn;
        gameManger.OnPlayModeOn += OnPlayModeOn;
        rigidBody = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        stateController = GetComponent<PlayerStateController>();
        positionRecord = new Queue<ReplayData>();
    }

    public void PutForceHeightPosition(Vector3 pos) {
        forceHeightPos.Add(pos);
    }

    void FixedUpdate() {
        foreach (MarkerObject markerObject in forceMarkers.ToList()) {
            if (markerObject.transform.position.x > transform.position.x) {
                forceMarkersToDelete.Add(markerObject);
                forceHeightPos.Remove(markerObject.transform.position);
                forceMarkers.Remove(markerObject);
            }
        }
        switch (gameManger.CurrentGameMode) {
            case GameMode.play:
                DoRecord();
                recordBar.BarValue = Mathf.RoundToInt((float)positionRecord.Count / replaySize * 100f);
                break;
            case GameMode.playback:
                DoPlayBack();
                if (Input.GetButtonDown("Jump") || playbackData.Count == 0) {
                    gameManger.StopPlaybackMode();
                    Time.timeScale = 0.0000001f;
                }
                stopTrigger = false;
                recordBar.BarValue = Mathf.RoundToInt((float)playbackData.Count / replaySize * 100f);
                break;
            case GameMode.playbackStopped:
                break;
        }
    }

    private void Update() {
        if (gameManger.CurrentGameMode != GameMode.playbackStopped)
            return;
        if (Input.GetButtonDown("Jump") && stopTrigger) {
            StartPlay();
            gameManger.StartPlayMode();
            Time.timeScale = 1f;
        }
        stopTrigger = true;
    }

    private void OnPlaybackModeOn() {
        label.enabled = true;
        rigidBody.detectCollisions = false;
        rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rigidBody.isKinematic = true;
        rigidBody.velocity = Vector3.zero;
        RemoveForceMarkers();
        if (markReplayForceJumpPoint) {
            PutForceMarkers();
        }
        playbackData = new Stack<ReplayData>(positionRecord);
        positionRecord.Clear();
    }

    private void PutForceMarkers() {
        foreach (Vector3 pos in forceHeightPos) {
            forceMarkers.Add(markersPool.GetMarker(pos, forceMarkerScale, markerColor, 0));
        }
    }

    private void OnPlayModeOn() {
        label.enabled = false;
        rigidBody.detectCollisions = true;
        rigidBody.isKinematic = false;
        rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        List<ReplayData> oldDataFromStack = new List<ReplayData>(playbackData);
        oldDataFromStack.Reverse();
        positionRecord = new Queue<ReplayData>(oldDataFromStack);
        DoRecord();
    }

    private void RemoveForceMarkers() {
        foreach (MarkerObject markerObject in forceMarkersToDelete) {
            markerObject.DestroyNow();
        }
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
            .build();
        positionRecord.Enqueue(replayData);
        if (positionRecord.Count > replaySize) {
            positionRecord.Dequeue();
        }
    }

    private void DoPlayBack() {
        currentReplayData = playbackData.Pop();
        transform.position = currentReplayData.posotion;
        cameraController.CameraSize = currentReplayData.cameraZoom;
        playerMovement.CurrentAngel = currentReplayData.currentAngel;
    }
    private void StartPlay() {
        playerMovement.CurrentJumpHeight = currentReplayData.jumpHeight;
        stateController.CurrentJumpState = currentReplayData.jumpState;
        playerMovement.StartBoostYPos = currentReplayData.startBoostYPos;
        playerMovement.ApexYPos = currentReplayData.apexYPos;
        playerMovement.YVelocity = currentReplayData.yVelocity;
        playerMovement.TimeScale = currentReplayData.timeScale;
        playerMovement.AngularSpeed = currentReplayData.angularSpeed;
    }

    class ReplayData {
        public Vector3 posotion;
        public float jumpHeight;
        public float currentAngel;
        public float angularSpeed;
        public float yVelocity;
        public float timeScale;
        public JumpState jumpState;
        public float startBoostYPos;
        public float apexYPos;
        public float cameraZoom;

        public static Builder builder() {
            return new Builder();
        }

        public class Builder {
            private Vector3 posotion;
            private float jumpHeight;
            private float currentAngel;
            private float angularSpeed;
            public float yVelocity;
            private float timeScale;
            private JumpState jumpState;
            private float startBoostYPos;
            private float apexYPos;
            private float cameraZoom;

            public Builder setPosition(Vector3 posotion) {
                this.posotion = posotion;
                return this;
            }
            public Builder setJumpHeight(float jumpHeight) {
                this.jumpHeight = jumpHeight;
                return this;
            }
            public Builder setStartBoostYPos(float startBoostYPos) {
                this.startBoostYPos = startBoostYPos;
                return this;
            }
            public Builder setApexYPos(float apexYPos) {
                this.apexYPos = apexYPos;
                return this;
            }
            public Builder setJumpState(JumpState jumpState) {
                this.jumpState = jumpState;
                return this;
            }

            public Builder setCameraZoom(float cameraZoom) {
                this.cameraZoom = cameraZoom;
                return this;
            }
            public Builder setYVelocity(float yVelocity) {
                this.yVelocity = yVelocity;
                return this;
            }
            public Builder setAngularSpeed(float angularSpeed) {
                this.angularSpeed = angularSpeed;
                return this;
            }
            public Builder setCurrentAngel(float currentAngel) {
                this.currentAngel = currentAngel;
                return this;
            }
            public Builder setTimeScale(float timeScale) {
                this.timeScale = timeScale;
                return this;
            }

            public ReplayData build() {
                ReplayData replayData = new ReplayData();
                replayData.posotion = posotion;
                replayData.yVelocity = yVelocity;
                replayData.jumpHeight = jumpHeight;
                replayData.angularSpeed = angularSpeed;
                replayData.timeScale = timeScale;
                replayData.jumpState = jumpState;
                replayData.startBoostYPos = startBoostYPos;
                replayData.apexYPos = apexYPos;
                replayData.cameraZoom = cameraZoom;
                replayData.currentAngel = currentAngel;
                return replayData;
            }
        }
    }
}
