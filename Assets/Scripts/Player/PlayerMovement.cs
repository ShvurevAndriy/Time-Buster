using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    enum VelocityBehaviorType {
        regular,
        jetpack,
        parachute,
        planner
    }

    private const float collederEpsilon = 0.05f;

    [Range(45, 89)] public float jumpAngel = 77f;
    public float minJumpHeight = 5f;
    public float maxJumpHeight = 50f;
    public float gravityScale = 4;
    public float forceJumpTimeScale = 3;
    public float radius = 40;
    public float currentAngel = 0;
    public bool forceByXAxis = false;
    public float angularForceCoeff = 2f;

    private Rigidbody rigidBody;
    private BoxCollider bottomCollider;
    private MyGameManager gameManager;
    private PlayerStateController playerStateController;
    private float yVelocity = 0;
    private float angularSpeed;
    private ContactPoint[] contactPoints = new ContactPoint[10];

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
            { VelocityBehaviorType.parachute, new ParachuteMovementBehavior(FindObjectOfType<ParachuteConfiguration>())},
            { VelocityBehaviorType.planner, new PlannerMovementBehavior(FindObjectOfType<PlannerConfiguration>())}
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

        if (forceByXAxis) {
            Vector3 centerCursorPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            AngularSpeed += (Mathf.Clamp01(centerCursorPos.x) - 0.5f) * 2 * JumpPhysics.LinearToAngularVelocity(angularForceCoeff, radius);
        }

        currentAngel += AngularSpeed * time;

        Vector3 nextPosition = JumpPhysics.CalculatePositionAtTime(
            yVelocity,
            currentAngel,
            radius,
            time,
            gravity,
            transform.position);

        if (ApexYPos < nextPosition.y) {
            ApexYPos = nextPosition.y;
        }

        switch (playerStateController.CurrentJumpState) {
            case JumpState.jetpack:
                velocityBehaviors[VelocityBehaviorType.jetpack].CalculateVelocities(time, gravity, radius, ref yVelocity, ref angularSpeed);
                break;
            case JumpState.patachute:
                velocityBehaviors[VelocityBehaviorType.parachute].CalculateVelocities(time, gravity, radius, ref yVelocity, ref angularSpeed);
                break;
            case JumpState.deltaplan:
                velocityBehaviors[VelocityBehaviorType.planner].CalculateVelocities(time, gravity, radius, ref yVelocity, ref angularSpeed);
                break;
            default:
                velocityBehaviors[VelocityBehaviorType.regular].CalculateVelocities(time, gravity, radius, ref yVelocity, ref angularSpeed);
                break;
        }

        rigidBody.velocity = (nextPosition - transform.position) / Time.deltaTime;
        SetAppropriateRotation();
    }

    void OnCollisionEnter(Collision collision) {
        if (gameManager.CurrentGameMode != GameMode.play) {
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
            DoJump();
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

        CurrentJumpHeight = JumpPhysics.CalculateNextJumpHeight(forceHeight, minJumpHeight, maxJumpHeight);
        Vector2 velocities = JumpPhysics.CalculateNextJumpVelocities(CurrentJumpHeight, JumpPhysics.g * gravityScale, jumpAngel);
        YVelocity = velocities.y;
        AngularSpeed = JumpPhysics.LinearToAngularVelocity(velocities.x, radius);
        ApexYPos = CurrentJumpHeight + transform.position.y;
        StartBoostYPos = float.NegativeInfinity;
        OnPlayerJump();
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask) {
        return layermask == (layermask | (1 << layer));
    }

    public interface VelocityBehavior {
        void CalculateVelocities(float time, float gravity, float radius, ref float yVelocity, ref float angularSpeed);
    }

    public class ParachuteMovementBehavior : VelocityBehavior {

        private ParachuteConfiguration parachuteConfiguration;

        public ParachuteMovementBehavior(ParachuteConfiguration parachuteConfiguration) {
            this.parachuteConfiguration = parachuteConfiguration;
        }

        public void CalculateVelocities(float time, float gravity, float radius, ref float yVelocity, ref float angularSpeed) {
            if (yVelocity > 0) {
                yVelocity = Mathf.Clamp(yVelocity - parachuteConfiguration.SlowdownUp * time, 0, float.PositiveInfinity); ;
            } else if (yVelocity >= parachuteConfiguration.YVelocity && yVelocity <= 0) {
                yVelocity = Mathf.Clamp(yVelocity + parachuteConfiguration.FreeFall * time, float.NegativeInfinity, parachuteConfiguration.YVelocity);
                angularSpeed = ChangeAngularSpeed(angularSpeed, radius, time);

            } else if (yVelocity < parachuteConfiguration.YVelocity) {
                angularSpeed = ChangeAngularSpeed(angularSpeed, radius, time);
                yVelocity = Mathf.Clamp(yVelocity + parachuteConfiguration.SlowdownDown * time, float.NegativeInfinity, parachuteConfiguration.YVelocity);
            }
        }

        private float ChangeAngularSpeed(float angularSpeed, float radius, float time) {
            float parachuteAngularSpeed = JumpPhysics.LinearToAngularVelocity(parachuteConfiguration.XVelocity, radius);
            if (angularSpeed > parachuteAngularSpeed) {
                angularSpeed = Mathf.Clamp(angularSpeed - JumpPhysics.LinearToAngularVelocity(parachuteConfiguration.XAccel * time, radius), parachuteAngularSpeed, float.PositiveInfinity);
            }
            return angularSpeed;
        }
    }

    public class PlannerMovementBehavior : VelocityBehavior {

        private PlannerConfiguration plannerConfiguration;

        public PlannerMovementBehavior(PlannerConfiguration plannerConfiguration) {
            this.plannerConfiguration = plannerConfiguration;
        }

        public void CalculateVelocities(float time, float gravity, float radius, ref float yVelocity, ref float angularSpeed) {
            if (yVelocity >= plannerConfiguration.YVelocity && yVelocity <= 0) {
                yVelocity = Mathf.Clamp(yVelocity + plannerConfiguration.FreeFall * time, float.NegativeInfinity, plannerConfiguration.YVelocity);

            } else if (yVelocity < plannerConfiguration.YVelocity) {
                yVelocity = Mathf.Clamp(yVelocity + plannerConfiguration.SlowdownDown * time, float.NegativeInfinity, plannerConfiguration.YVelocity);
            }
        }
    }


    public class JetPackMovementBehavior : VelocityBehavior {

        private JetpackConfiguration jetpackConfiguration;

        public JetPackMovementBehavior(JetpackConfiguration jetpackConfiguration) {
            this.jetpackConfiguration = jetpackConfiguration;
        }

        public void CalculateVelocities(float time, float gravity, float radius, ref float yVelocity, ref float angularSpeed) {
            yVelocity = Mathf.Clamp(yVelocity + jetpackConfiguration.JetThrust * time, float.MinValue, jetpackConfiguration.JetMaxVelocity);
        }
    }

    public class RegularMovementBehavior : VelocityBehavior {
        public void CalculateVelocities(float time, float gravity, float radius, ref float yVelocity, ref float angularSpeed) {
            yVelocity = yVelocity - gravity * time;
        }
    }
}