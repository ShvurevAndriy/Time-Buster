using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [Range(-20, 100)] [SerializeField] float yOffset = 0;
    [Range(-20, 100)] [SerializeField] float lookAtYOffset = 0;
    [Range(20, 150)] [SerializeField] float distanceToTarget = 20;

    [SerializeField] bool lockCameraOnPlayer = false;

    [SerializeField] float minDistance = 40;
    [SerializeField] float maxDistance = 75;

    [SerializeField] float increaseZoomTimeEasing = 2;

    [SerializeField] private float noDecreaseZoomTime = 1;
    [SerializeField] private float decreasingZoomStepMin = 0.07f;
    [SerializeField] private float decreasingZoomStepMax = 0.3f;

    [SerializeField] private float useMaxStepFromZoomDelta = 50f;
    [SerializeField] private float useMinStepFromZoomDelta = 10f;

    [SerializeField] Easings.Functions increaseCameraSizeEasing = Easings.Functions.QuinticEaseOut;

    private PlayerMovement player;
    private PlayerStateController playerState;
    private MyGameManager gameManager;
    private AccurateNextPositionFinder positionFinder;
    private BoxCollider boxCollider;

    private new Camera camera;
    private float calculatedCameraSize;

    private float previousCameraSize;

    private float startIncreaseingTime;
    private float lastChangedSize;
    private float startDecreasingTime;
    private float radius;
    private float cameraStartYpos;
    private float maxYDistanceFromCenter;
    private Vector3 collidingPoint;

    private float yMax;
    private float yMin;

    public float CameraSize { get; set; }
    public Vector3 CollidingPoint { get => collidingPoint; private set => collidingPoint = value; }

    void Start() {
        camera = GetComponent<Camera>();
        positionFinder = GetComponent<AccurateNextPositionFinder>();
        gameManager = FindObjectOfType<MyGameManager>();
        player = FindObjectOfType<PlayerMovement>();
        boxCollider = player.GetComponent<BoxCollider>();
        player.OnPlayerJump += UpdateLandingPoint;
        playerState = FindObjectOfType<PlayerStateController>();
        playerState.OnJetpackOff += UpdateLandingPoint;


        if (lockCameraOnPlayer) {
            CameraSize = minDistance;
        } else {
            yMax = GameObject.FindGameObjectWithTag("Roof").transform.position.y;
            yMin = GameObject.FindGameObjectWithTag("Floor").transform.position.y;
            cameraStartYpos = Mathf.Abs(yMax + yMin) / 2;
            maxYDistanceFromCenter = Mathf.Abs(yMax - yMin) / 2;
            CameraSize = maxDistance;
        }

        calculatedCameraSize = CameraSize;

        previousCameraSize = CameraSize;
        startIncreaseingTime = Time.time;
    }

    private void UpdateLandingPoint() {
        if (!lockCameraOnPlayer) {
            positionFinder.PredictTuchPoint(player.YVelocity,
                player.AngularSpeed,
                player.currentAngel,
                player.radius,
                player.gravityScale * JumpPhysics.g,
                JumpPhysics.layerMask,
                player.transform.position,
                out collidingPoint,
                boxCollider);
        }
    }

    void LateUpdate() {

        radius = player.radius + distanceToTarget;
        float cameraYPos;
        Vector3 lookAtPos;

        if (lockCameraOnPlayer) {
            cameraYPos = yOffset + player.transform.position.y;
            lookAtPos = player.transform.position;
            lookAtPos.y += lookAtYOffset;
        } else {
            lookAtPos = Vector3.zero;
            lookAtPos.y = cameraStartYpos;
            cameraYPos = cameraStartYpos;

        }

        transform.position = new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * player.currentAngel) * (radius + CameraSize),
            cameraYPos,
            Mathf.Sin(Mathf.Deg2Rad * player.currentAngel) * (radius + CameraSize));

        transform.LookAt(lookAtPos);

        if (gameManager.CurrentGameMode == GameMode.play) {
            SetCameraSize();
        }
    }

    private void SetCameraSize() {
        calculatedCameraSize = GetCameraZoom();
        if (Mathf.Abs(lastChangedSize - calculatedCameraSize) > Mathf.Epsilon) {
            if (lastChangedSize < calculatedCameraSize) {
                previousCameraSize = CameraSize;
                startIncreaseingTime = Time.time;
            }
            lastChangedSize = calculatedCameraSize;
        }
        if (Mathf.Abs(calculatedCameraSize - CameraSize) > Mathf.Epsilon) {
            if (CameraSize > calculatedCameraSize) {
                if (Time.time - startDecreasingTime < noDecreaseZoomTime) {
                    return;
                }
                ZoomOut();
            } else {
                ZoomIn();
            }
        } else {
            startDecreasingTime = Time.time;
        }
    }

    private void ZoomOut() {
        float deltaZoom = CameraSize - calculatedCameraSize;
        float zoomStepPerc = Mathf.InverseLerp(useMinStepFromZoomDelta, useMaxStepFromZoomDelta, Mathf.Clamp(deltaZoom, useMinStepFromZoomDelta, useMaxStepFromZoomDelta));
        float zoomStep = decreasingZoomStepMin + (decreasingZoomStepMax - decreasingZoomStepMin) * zoomStepPerc;
        CameraSize -= Mathf.Min(CameraSize - calculatedCameraSize, zoomStep);
    }

    private void ZoomIn() {
        startDecreasingTime = Time.time;
        float deltaSize = calculatedCameraSize - previousCameraSize;
        float easingTime = Mathf.Clamp01((Time.time - startIncreaseingTime) / increaseZoomTimeEasing);
        float easing = Easings.Interpolate(easingTime, increaseCameraSizeEasing);
        CameraSize = previousCameraSize + deltaSize * easing;
    }

    private float GetCameraZoom() {
        float jumpPercentage;

        if (playerState.CurrentJumpState == JumpState.jetpack) {
            jumpPercentage = 1;
        } else {
            if (lockCameraOnPlayer) {
                return player.CurrentJumpHeight / player.minJumpHeight * minDistance; ;
            } else {

                float deltaY = Mathf.Max(Mathf.Abs(camera.transform.position.y - collidingPoint.y), Mathf.Abs(camera.transform.position.y - player.CurrentJumpHeight));
                jumpPercentage = Mathf.InverseLerp(0, maxYDistanceFromCenter, Mathf.Abs(camera.transform.position.y - collidingPoint.y));
            }
        }
        return Mathf.Lerp(minDistance, maxDistance, jumpPercentage);
    }

}
