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

    private new Camera camera;
    private float calculatedCameraSize;

    private float previousCameraSize;

    private float startIncreaseingTime;
    private float lastChangedSize;
    private float startDecreasingTime;
    private float radius;
    private float cameraStartYpos;

    public float CameraSize { get; set; }

    void Start() {
        camera = GetComponent<Camera>();
        gameManager = FindObjectOfType<MyGameManager>();
        player = FindObjectOfType<PlayerMovement>();
        playerState = FindObjectOfType<PlayerStateController>();
        CameraSize = minDistance;

        float? yMax = GameObject.FindGameObjectWithTag("Roof")?.transform.position.y;
        float? yMin = GameObject.FindGameObjectWithTag("Floor")?.transform.position.y;
        if (yMax.HasValue && yMin.HasValue) {
            cameraStartYpos = Mathf.Abs(yMax.Value + yMin.Value) / 2;
        } else {
            cameraStartYpos = camera.transform.position.y;
        }

        calculatedCameraSize = CameraSize;

        previousCameraSize = CameraSize;
        startIncreaseingTime = Time.time;
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
        if (lockCameraOnPlayer) {
            float jumpPercentage = 1;
            if (playerState.CurrentJumpState != JumpState.jetpack) {
                jumpPercentage = Mathf.InverseLerp(player.minJumpHeight, player.maxJumpHeight, player.CurrentJumpHeight);
            }
            return Mathf.Lerp(minDistance, maxDistance, jumpPercentage);
        } else {
            return Mathf.Abs(camera.transform.position.y - player.transform.position.y) * minDistance;
        }
    }
}
