using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    enum VelocityBehaviorType {
        regular,
        jetpack,
        paratroop,
        planner
    }

    [Range(45, 89)] public float jumpAngel = 77f;
    public float minJumpHeight = 5f;
    public float maxJumpHeight = 50f;
    public float gravityScale = 4;
    public float forceJumpTimeScale = 3;
    public float radius = 40;
    public float currentAngel = 0;
    public float paratrooperVelocity = -2f;
    public float jetThrust = 10f;
    public float jetMaxVelocity = float.MaxValue;

    private Rigidbody rigidBody;
    private BoxCollider bottomCollider;
    private MyGameManager gameManager;
    private PlayerStateController playerStateController;
    private float yVelocity = 0;
    private float angularSpeed;

    private Dictionary<VelocityBehaviorType, VelocityBehavior> velocityBehaviors;


    public float StartBoostYPos { get; set; }
    public float ApexYPos { get; set; }
    public float CurrentJumpHeight { get; set; }
    public float YVelocity { get => yVelocity; set => yVelocity = value; }
    public float TimeScale { get; set; } = 1;
    public float AngularSpeed { get => angularSpeed; set => angularSpeed = value; }

    public delegate void PlayerOnJumpAction();

    public event PlayerOnJumpAction OnPlayerJump = delegate { };
    public event PlayerOnJumpAction OnPlayerForeJump = delegate { };

    void Start() {
        velocityBehaviors = new Dictionary<VelocityBehaviorType, VelocityBehavior> {
            { VelocityBehaviorType.regular, new RegularMovementBehavior()},
            { VelocityBehaviorType.jetpack, new JetPackMovementBehavior(FindObjectOfType<JetpackConfiguration>())},
            { VelocityBehaviorType.paratroop, new RegularMovementBehavior()},
            { VelocityBehaviorType.planner, new RegularMovementBehavior()}
        };

        gameManager = FindObjectOfType<MyGameManager>();
        rigidBody = GetComponent<Rigidbody>();
        bottomCollider = GetComponent<BoxCollider>();
        playerStateController = GetComponent<PlayerStateController>();
    }

    void FixedUpdate() {
        if (gameManager.CurrentGameMode != GameMode.play) {
            transform.rotation = Quaternion.Euler(0, 90 - currentAngel, 0);
            return;
        }

        float time = Time.deltaTime * TimeScale;
        float gravity = gravityScale * JumpPhysics.g;

        Vector3 nextPosition = JumpPhysics.CalculatePositionAtTime(
            yVelocity,
            currentAngel,
            radius,
            time,
            gravity,
            transform.position);

        currentAngel += AngularSpeed * time;

        switch (playerStateController.CurrentJumpState) {
            case JumpState.jetpack:
                velocityBehaviors[VelocityBehaviorType.jetpack].CalculateVelocities(time, gravity, ref yVelocity, ref angularSpeed);
                break;
            default:
                velocityBehaviors[VelocityBehaviorType.regular].CalculateVelocities(time, gravity, ref yVelocity, ref angularSpeed);
                break;
        }

        rigidBody.velocity = (nextPosition - transform.position) / Time.deltaTime;
        SetAppropriateRotation();
    }

    void OnCollisionEnter(Collision collision) {
        if (gameManager.CurrentGameMode != GameMode.play) {
            return;
        }
        bool activatePlayback = false;
        if (IsInLayerMask(collision.gameObject.layer, LayerMask.GetMask("Hazard"))) {
            gameManager.StartPlaybackMode();
        } else {

            foreach (ContactPoint contact in collision.contacts) {
                switch (contact.thisCollider.name) {
                    case "Player":
                        activatePlayback = true;
                        break;
                }
            }
            if (activatePlayback) {
                gameManager.StartPlaybackMode();
            } else {
                if (IsInLayerMask(collision.gameObject.layer, LayerMask.GetMask("Level End"))) {
                    gameManager.LevelFinished();
                } else {
                    DoJump();
                }
            }
        }
    }

    public void SetAppropriateRotation() {
        transform.rotation = Quaternion.Euler(0, 90 - currentAngel, 0);
    }

    public void ForceDownJump() {
        TimeScale = forceJumpTimeScale;
        OnPlayerForeJump();
    }

    public void TakeStartBoostYPosValue(JumpState currentJumpState) {
        if (currentJumpState == JumpState.slowDown || currentJumpState == JumpState.apex) {
            StartBoostYPos = ApexYPos;
        } else if (currentJumpState == JumpState.flyDown) {
            StartBoostYPos = transform.position.y;
        }
    }

    private void DoJump() {
        TimeScale = 1;
        float forceHeight = 0;
        if (!float.IsNegativeInfinity(StartBoostYPos)) {
            forceHeight = JumpPhysics.CalculateBoostJumpHeight(StartBoostYPos, transform.position.y);
        }

        float CurrentJumpHeight = JumpPhysics.CalculateNextJumpHeight(forceHeight, minJumpHeight, maxJumpHeight);
        Vector2 velocities = JumpPhysics.CalculateNextJumpVelocities(CurrentJumpHeight, JumpPhysics.g * gravityScale, jumpAngel);
        YVelocity = velocities.y;
        AngularSpeed = velocities.x * Mathf.Rad2Deg / radius;
        ApexYPos = CurrentJumpHeight + transform.position.y;
        StartBoostYPos = float.NegativeInfinity;
        OnPlayerJump();
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask) {
        return layermask == (layermask | (1 << layer));
    }

    public interface VelocityBehavior {
        void CalculateVelocities(float time, float gravity, ref float yVelocity, ref float angularSpeed);
    }

    public class JetPackMovementBehavior : VelocityBehavior {

        private JetpackConfiguration jetpackConfiguration;

        public JetPackMovementBehavior(JetpackConfiguration jetpackConfiguration) {
            this.jetpackConfiguration = jetpackConfiguration;
        }

        public void CalculateVelocities(float time, float gravity, ref float yVelocity, ref float angularSpeed) {
            yVelocity = Mathf.Clamp(yVelocity + jetpackConfiguration.JetThrust * time, float.MinValue, jetpackConfiguration.JetMaxVelocity);
        }
    }

    public class RegularMovementBehavior : VelocityBehavior {
        public void CalculateVelocities(float time, float gravity, ref float yVelocity, ref float angularSpeed) {
            yVelocity = yVelocity - gravity * time;
        }
    }
}