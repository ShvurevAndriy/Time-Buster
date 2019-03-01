using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private const float g = 9.8f;

    [Range(20, 200)] public float radius = 35f;
    [Range(45, 89)] public float jumpAngel = 77f;
    public float minJumpHeight = 5f;
    public float maxJumpHeight = 50f;
    public float gravityScale = 4;
    public float forceJumpTimeScale = 3;

    private Rigidbody rigidBody;
    private BoxCollider bottomCollider;
    private MyGameManager gameManager;

    public float CurrentAngel { get; set; } = 0;
    public float StartBoostYPos { get; set; }
    public float ApexYPos { get; set; }
    public float CurrentJumpHeight { get; set; }
    public float YVelocity { get; set; } = 0;
    public float TimeScale { get; set; } = 1;
    public float AngularSpeed { get; set; }

    public delegate void PlayerOnJumpAction();

    public event PlayerOnJumpAction OnPlayerJump = delegate { };
    public event PlayerOnJumpAction OnPlayerForeJump = delegate { };

    void Start() {
        gameManager = FindObjectOfType<MyGameManager>();
        rigidBody = GetComponent<Rigidbody>();
        bottomCollider = GetComponent<BoxCollider>();
        transform.position = new Vector3(Mathf.Cos(Mathf.Deg2Rad * CurrentAngel) * radius, transform.position.y, Mathf.Sin(Mathf.Deg2Rad * CurrentAngel) * radius);
    }

    void FixedUpdate() {
        
        if (gameManager.CurrentGameMode != GameMode.play) {
            transform.rotation = Quaternion.Euler(0, 90 - CurrentAngel, 0);
            return;
        }

        CurrentAngel += AngularSpeed * TimeScale;
        transform.rotation = Quaternion.Euler(0, 90 - CurrentAngel, 0);
        Vector3 nextPositions = JumpPhysics.CalculatePositionAtTime(YVelocity, CurrentAngel, radius, Time.deltaTime * TimeScale, g * gravityScale, transform.position);
        Vector3 horizontalVelocities = (nextPositions - transform.position) / Time.deltaTime;
        YVelocity = YVelocity - g * gravityScale * Time.deltaTime;
        rigidBody.velocity = horizontalVelocities;
    }

    public void DoJump() {
        TimeScale = 1;
        float forceHeight = 0;
        if (!float.IsNegativeInfinity(StartBoostYPos)) {
            forceHeight = JumpPhysics.CalculateBoostJumpHeight(StartBoostYPos, transform.position.y);
        }

        float CurrentJumpHeight = JumpPhysics.CalculateNextJumpHeight(forceHeight, minJumpHeight, maxJumpHeight);
        Vector2 velocities = JumpPhysics.CalculateNextJumpVelocities(CurrentJumpHeight, g * gravityScale, jumpAngel);
        YVelocity = velocities.y;
        AngularSpeed = velocities.x / radius;
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
}
