using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    enum VelocityBehaviorType {
        regular,
        jetpack,
        paratroop
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

    private Rigidbody rigidBody;
    private BoxCollider bottomCollider;
    private MyGameManager gameManager;
    private float yVelocity = 0;

    private Dictionary<VelocityBehaviorType, VelocityBehavior> velocityBehaviors;

    public float StartBoostYPos { get; set; }
    public float ApexYPos { get; set; }
    public float CurrentJumpHeight { get; set; }
    public float YVelocity { get => yVelocity; set => yVelocity = value; }
    public float TimeScale { get; set; } = 1;
    public float AngularSpeed { get; set; }

    public delegate void PlayerOnJumpAction();

    public event PlayerOnJumpAction OnPlayerJump = delegate { };
    public event PlayerOnJumpAction OnPlayerForeJump = delegate { };

    void Start() {
        velocityBehaviors = new Dictionary<VelocityBehaviorType, VelocityBehavior> {
            { VelocityBehaviorType.regular, new RegularMovementBehavior(this)},
            { VelocityBehaviorType.jetpack, new RegularMovementBehavior(this)},
            { VelocityBehaviorType.paratroop, new RegularMovementBehavior(this)}
        };

        gameManager = FindObjectOfType<MyGameManager>();
        rigidBody = GetComponent<Rigidbody>();
        bottomCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate() {
        if (gameManager.CurrentGameMode != GameMode.play) {
            transform.rotation = Quaternion.Euler(0, 90 - currentAngel, 0);
            return;
        }
        rigidBody.velocity = velocityBehaviors[VelocityBehaviorType.regular].GetVelocity(ref yVelocity);
        SetAppropriateRotation();
    }

    public void SetAppropriateRotation() {
        transform.rotation = Quaternion.Euler(0, 90 - currentAngel, 0);
    }

    public void DoJump() {
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

    private void OnCollisionEnter(Collision collision) {
        if (gameManager.CurrentGameMode != GameMode.play) {
            return;
        }
        bool activatePlayback = false;
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
            DoJump();
        }
    }

    public interface VelocityBehavior {
        Vector3 GetVelocity(ref float yVelocity);
    }

    public class ParachuteMovementBehavior : VelocityBehavior {

        private PlayerMovement playerMovement;

        public ParachuteMovementBehavior(PlayerMovement playerMovement) {
            this.playerMovement = playerMovement;
        }

        public Vector3 GetVelocity(ref float yVelocity) {
            playerMovement.currentAngel += playerMovement.AngularSpeed * playerMovement.TimeScale;
            Vector3 nextPositions = JumpPhysics.CalculatePositionAtTime(yVelocity,
                playerMovement.currentAngel,
                playerMovement.radius,
                Time.deltaTime * playerMovement.TimeScale,
                JumpPhysics.g * playerMovement.gravityScale,
                playerMovement.transform.position);
            yVelocity = yVelocity + playerMovement.jetThrust * Time.deltaTime;
            return (nextPositions - playerMovement.transform.position) / Time.deltaTime;
        }
    }


    public class JetPackMovementBehavior : VelocityBehavior {

        private PlayerMovement playerMovement;

        public JetPackMovementBehavior(PlayerMovement playerMovement) {
            this.playerMovement = playerMovement;
        }

        public Vector3 GetVelocity(ref float yVelocity) {
            playerMovement.currentAngel += playerMovement.AngularSpeed * playerMovement.TimeScale;
            Vector3 nextPositions = JumpPhysics.CalculatePositionAtTime(yVelocity,
                playerMovement.currentAngel,
                playerMovement.radius,
                Time.deltaTime * playerMovement.TimeScale,
                JumpPhysics.g * playerMovement.gravityScale,
                playerMovement.transform.position);
            yVelocity = yVelocity + playerMovement.jetThrust * Time.deltaTime;
            return (nextPositions - playerMovement.transform.position) / Time.deltaTime;
        }
    }

    public class RegularMovementBehavior : VelocityBehavior {
        private PlayerMovement playerMovement;

        public RegularMovementBehavior(PlayerMovement playerMovement) {
            this.playerMovement = playerMovement;
        }

        public Vector3 GetVelocity(ref float yVelocity) {
            playerMovement.currentAngel += playerMovement.AngularSpeed * playerMovement.TimeScale * Time.deltaTime;
            Vector3 nextPositions = JumpPhysics.CalculatePositionAtTime(yVelocity,
                playerMovement.currentAngel,
                playerMovement.radius,
                Time.deltaTime * playerMovement.TimeScale,
                JumpPhysics.g * playerMovement.gravityScale,
                playerMovement.transform.position);
            yVelocity = yVelocity - JumpPhysics.g * playerMovement.gravityScale * Time.deltaTime;
            return (nextPositions - playerMovement.transform.position) / Time.deltaTime;
        }
    }
}
