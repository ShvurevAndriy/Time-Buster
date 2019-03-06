using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] bool lookAtPlayer = true;
    [SerializeField] bool followPlayerByY = false;
    [Range(-20, 60)] [SerializeField] float yOffset = 0;
    [Range(-20, 60)] [SerializeField] float yTargetOffset = 0;
    [SerializeField] float distanceToTarget = 20;
    [SerializeField] float coeff = 0.5f;

    [SerializeField] float increaseZoomTimeEasing = 2;

    [SerializeField] private float noDecreaseZoomTime = 1;
    [SerializeField] private float decreasingZoomStepMin = 0.07f;
    [SerializeField] private float decreasingZoomStepMax = 0.3f;

    [SerializeField] private float useMaxStepFromZoomDelta = 50f;
    [SerializeField] private float useMinStepFromZoomDelta = 10f;

    [SerializeField] Easings.Functions increaseCameraSizeEasing = Easings.Functions.QuinticEaseOut;

    private PlayerMovement player;
    private MyGameManager gameManager;

    private new Camera camera;
    private float calculatedCameraSize;

    private float startYCameraPos;
    private float previousCameraSize;

    private float startIncreaseingTime;
    private float lastChangedSize;
    private float startDecreasingTime;
    private float radius;


    public float CameraSize { get; set; }

    void Start() {
        camera = GetComponent<Camera>();
        gameManager = FindObjectOfType<MyGameManager>();
        player = FindObjectOfType<PlayerMovement>();
        CameraSize = 0;

        calculatedCameraSize = CameraSize;
        startYCameraPos = camera.transform.position.y;

        previousCameraSize = CameraSize;
        startIncreaseingTime = Time.time;
    }

    void LateUpdate() {

        radius = player.radius + distanceToTarget;
        transform.position = new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * player.currentAngel) * (radius + CameraSize),
            yOffset,
            Mathf.Sin(Mathf.Deg2Rad * player.currentAngel) * (radius + CameraSize));

        Vector3 lookAt;
        if (lookAtPlayer) {
            lookAt = player.transform.position;
            if (!followPlayerByY) {
                lookAt.y = yTargetOffset;
            }
        } else {
            lookAt = Vector3.zero;
            lookAt.y = yTargetOffset;
        }
        transform.LookAt(lookAt);

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
                float deltaZoom = CameraSize - calculatedCameraSize;

                float zoomStepPerc = Mathf.InverseLerp(useMinStepFromZoomDelta, useMaxStepFromZoomDelta, Mathf.Clamp(deltaZoom, useMinStepFromZoomDelta, useMaxStepFromZoomDelta));
                float zoomStep = decreasingZoomStepMin + (decreasingZoomStepMax - decreasingZoomStepMin) * zoomStepPerc;

                CameraSize -= Mathf.Min(CameraSize - calculatedCameraSize, zoomStep);
            } else {
                startDecreasingTime = Time.time;
                float deltaSize = calculatedCameraSize - previousCameraSize;
                float easingTime = Mathf.Clamp01((Time.time - startIncreaseingTime) / increaseZoomTimeEasing);
                float easing = Easings.Interpolate(easingTime, increaseCameraSizeEasing);
                CameraSize = previousCameraSize + deltaSize * easing;

            }
        } else {
            startDecreasingTime = Time.time;
        }
    }

    private float GetCameraZoom() {
        return player.ApexYPos * coeff;
    }
}
