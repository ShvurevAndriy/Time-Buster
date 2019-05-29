using System.Collections.Generic;
using UnityEngine;
using Game.velocity;

public class PlayerMovement : MonoBehaviour {

    enum VelocityBehaviorType {
        regular,
        jetpack,
        parachute,
        planner
    }

    private const float collederEpsilon = 0.05f;
    private const float colliderMoveCoef = 1.0001f;

    [Range(45, 89)] public float jumpAngel = 77f;
    [SerializeField] float minJumpHeight = 5f;
    [SerializeField] float maxJumpHeight = 50f;
    [SerializeField] float gravityScale = 4;
    [SerializeField] float forceJumpTimeScale = 3;
    [SerializeField] float radius = 40;
    [SerializeField] float currentAngel = 0;
    [SerializeField] bool forceByXAxis = false;
    [SerializeField] float angularForceCoeff = 2f;

    private Rigidbody rigidBody;
    private BoxCollider boxCollider;
    private MyGameManager gameManager;
    private PlayerStateController playerStateController;
    private float yVelocity = 0;
    private float angularSpeed;
    private ContactPoint[] contactPoints = new ContactPoint[10];

    private Dictionary<VelocityBehaviorType, VelocityBehavior> velocityBehaviors;
    private Vector3 nextPosition;
    private bool isLanding;

    public float StartBoostYPos { get; set; }
    public float ApexYPos { get; set; }
    public float CurrentJumpHeight { get; set; }
    public float YVelocity { get => yVelocity; set => yVelocity = value; }
    public float TimeScale { get; set; } = 1;
    public float AngularSpeed { get => angularSpeed; set => angularSpeed = value; }
    public float Radius { get => radius; set => radius = value; }
    public float CurrentAngel { get => currentAngel; set => currentAngel = value; }
    public float MinJumpHeight { get => minJumpHeight; set => minJumpHeight = value; }
    public float MaxJumpHeight { get => maxJumpHeight; set => maxJumpHeight = value; }
    public float GravityScale { get => gravityScale; set => gravityScale = value; }

    public delegate void PlayerOnJumpAction();

    public event PlayerOnJumpAction OnPlayerJump = delegate { };
    public event PlayerOnJumpAction OnPlayerLanding = delegate { };
    public event PlayerOnJumpAction OnPlayerForeJump = delegate { };

    void Start() {

        velocityBehaviors = new Dictionary<VelocityBehaviorType, VelocityBehavior> {
            { VelocityBehaviorType.regular, new RegularMovementBehavior()},
            { VelocityBehaviorType.jetpack, new JetPackMovementBehavior(FindObjectOfType<JetpackConfiguration>())},
            { VelocityBehaviorType.parachute, new ParachuteMovementBehavior(FindObjectOfType<ParachuteConfiguration>())},
            { VelocityBehaviorType.planner, new PlannerMovementBehavior(FindObjectOfType<PlannerConfiguration>())}
        };

        gameManager = FindObjectOfType<MyGameManager>();
        gameManager.OnPlayModeOn += OnPlayMode;
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        playerStateController = GetComponent<PlayerStateController>();

        nextPosition = transform.position;
    }

    void FixedUpdate() {
        if (isLanding){
            return;
        }

        if (gameManager.CurrentGameMode != GameMode.play) {
            transform.rotation = Quaternion.Euler(0, 90 - CurrentAngel, 0);
            return;
        }

        float time = Time.deltaTime * TimeScale;
        float gravity = GravityScale * JumpPhysics.g;

        if (forceByXAxis) {
            Vector3 centerCursorPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            AngularSpeed += (Mathf.Clamp01(centerCursorPos.x) - 0.5f) * 2 * JumpPhysics.LinearToAngularVelocity(angularForceCoeff, Radius);
        }

        transform.position = nextPosition;

        float previousAngel = currentAngel;

        nextPosition = JumpPhysics.CalculatePositionAtTime(
            yVelocity,
            AngularSpeed,
            ref currentAngel,
            Radius,
            time,
            gravity,
            transform.position);

        if (ApexYPos < nextPosition.y) {
            ApexYPos = nextPosition.y;
        }

        switch (playerStateController.CurrentJumpState) {
            case JumpState.jetpack:
                velocityBehaviors[VelocityBehaviorType.jetpack].CalculateVelocities(time, gravity, Radius, ref yVelocity, ref angularSpeed);
                break;
            case JumpState.patachute:
                velocityBehaviors[VelocityBehaviorType.parachute].CalculateVelocities(time, gravity, Radius, ref yVelocity, ref angularSpeed);
                break;
            case JumpState.deltaplan:
                velocityBehaviors[VelocityBehaviorType.planner].CalculateVelocities(time, gravity, Radius, ref yVelocity, ref angularSpeed);
                break;
            default:
                velocityBehaviors[VelocityBehaviorType.regular].CalculateVelocities(time, gravity, Radius, ref yVelocity, ref angularSpeed);
                break;
        }

        RaycastHit mHit;

        Vector3 direction = nextPosition - transform.position;

        if (yVelocity < 0 && Physics.BoxCast(boxCollider.bounds.center, boxCollider.bounds.extents, direction.normalized, out mHit, Quaternion.Euler(transform.rotation.eulerAngles.x, 90 + transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z), direction.magnitude, JumpPhysics.layerMask)) {
            nextPosition = transform.position + (direction.normalized * mHit.distance);
            rigidBody.velocity = new Vector3(0, ((nextPosition - transform.position) * colliderMoveCoef / Time.deltaTime).y, 0);

            CurrentAngel = previousAngel;
        } else {
            rigidBody.velocity = direction / Time.deltaTime;
        }
        SetAppropriateRotation();
    }

    void OnCollisionEnter(Collision collision) {
        if (gameManager.CurrentGameMode != GameMode.play || isLanding) {
            return;
        }

        if (IsInLayerMask(collision.gameObject.layer, LayerMask.GetMask("Hazard"))) {
            gameManager.StartPlaybackMode();
            return;
        }

        if (collision.contactCount > contactPoints.Length) {
            contactPoints = new ContactPoint[collision.contactCount];
        }
        collision.GetContacts(contactPoints);
        for (int i = 0; i < collision.contactCount; i++) {
            ContactPoint contactPoint = contactPoints[i];
            if (contactPoint.point.y > contactPoint.thisCollider.bounds.min.y + collederEpsilon) {
                gameManager.StartPlaybackMode();
                return;
            }
        }

        if (IsInLayerMask(collision.gameObject.layer, LayerMask.GetMask("Level End"))) {
            gameManager.LevelFinished();
        } else {
            DoLanding();
        }

    }

    private void OnPlayMode() {
        isLanding = false;
        nextPosition = transform.position;
    }

    public void SetAppropriateRotation() {
        transform.rotation = Quaternion.Euler(0, -CurrentAngel - 90, 0);
    }

    public void ForceDownJump() {
        TimeScale = forceJumpTimeScale;
        OnPlayerForeJump();
    }

    public void TakeStartBoostYPosValue(bool apexWasReached) {
        if (!apexWasReached && (playerStateController.CurrentJumpState == JumpState.slowDown || playerStateController.CurrentJumpState == JumpState.apex)) {
            StartBoostYPos = ApexYPos;
        } else {
            StartBoostYPos = transform.position.y;
        }
    }

    public void DoNextJump() {
        isLanding = false;
        rigidBody.isKinematic = false;
        OnPlayerJump();
    }

    private void DoLanding() {
        isLanding = true;
        YVelocity = 0;
        AngularSpeed = 0;
        rigidBody.velocity = Vector3.zero;
        nextPosition = transform.position;
        TimeScale = 1;
        float forceHeight = 0;
        if (!float.IsNegativeInfinity(StartBoostYPos))
        {
            forceHeight = JumpPhysics.CalculateBoostJumpHeight(StartBoostYPos, transform.position.y);
        }

        CurrentJumpHeight = JumpPhysics.CalculateNextJumpHeight(forceHeight, MinJumpHeight, MaxJumpHeight);
        Vector2 velocities = JumpPhysics.CalculateNextJumpVelocities(CurrentJumpHeight, JumpPhysics.g * GravityScale, jumpAngel);
        YVelocity = velocities.y;
        AngularSpeed = JumpPhysics.LinearToAngularVelocity(velocities.x, Radius);
        ApexYPos = CurrentJumpHeight + transform.position.y;
        StartBoostYPos = float.NegativeInfinity;
        OnPlayerLanding();
    }


    public static bool IsInLayerMask(int layer, LayerMask layermask) {
        return layermask == (layermask | (1 << layer));
    }
}