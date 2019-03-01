using UnityEngine;

public enum JumpState {
    flyUp = 0,
    slowDown = 1,
    apex = 2,
    flyDown = 3,
    force = 4
}

public class PlayerStateController : MonoBehaviour {

    public float minVelocityForSlowDown;
    private MyGameManager gameManager;
    private PlayerMovement playerMovement;
    private Animator animator;

    private bool wasClickAfterPlayback;
    private bool forceTrigger;

    public JumpState CurrentJumpState { get;  set; }

    void Start() {
        gameManager = FindObjectOfType<MyGameManager>();
        gameManager.OnPlaybackModeOn += OnPlaybackModeOn;
        gameManager.OnPlayModeOn += OnPlayModeOn;

        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.OnPlayerJump += OnJump;

        CurrentJumpState = JumpState.flyDown;
    }

    void Update() {
        if (gameManager.CurrentGameMode != GameMode.play) {
            return;
        }

        switch (CurrentJumpState) {
            case JumpState.flyUp:
                CheckTransitionToSlowDownState();
                ProcessUserInput();
                break;
            case JumpState.slowDown:
                CheckTransitionToApexState();
                ProcessUserInput();
                break;
            case JumpState.apex:
            case JumpState.flyDown:
                ProcessUserInput();
                break;
            case JumpState.force:
                return;
        }
    }

    public void OnFlyDownAnimation() {
        if (CurrentJumpState != JumpState.flyDown) {
            ChanegeJumpState(JumpState.flyDown);
        }
    }

    public void OnForceDownAnimation() {
        if (CurrentJumpState != JumpState.force) {
            ChanegeJumpState(JumpState.force);
            playerMovement.ForceDownJump();
        }
    }

    private void ProcessUserInput() {
        if (Input.GetButtonDown("Jump") && !wasClickAfterPlayback) {
            wasClickAfterPlayback = true;
            return;
        }
        if (Input.GetButton("Jump") && wasClickAfterPlayback) {
            SetForceDownJumpTrigger();
        }
    }

    private void OnPlaybackModeOn() {
        animator.SetTrigger("Play Back");
        animator.SetInteger("Jump State", -1);
    }

    private void OnPlayModeOn() {
        wasClickAfterPlayback = false;
        animator.ResetTrigger("Play Back");
        ChanegeJumpState(CurrentJumpState);
        if (CurrentJumpState == JumpState.force) {
            animator.SetTrigger("Force Jump");
        }
    }

    private void OnJump() {
        forceTrigger = false;
        ChanegeJumpState(JumpState.flyUp);
        animator.ResetTrigger("Force Jump");
        animator.ResetTrigger("Immediate force jump");
        animator.SetTrigger("New Jump");
    }

    private void CheckTransitionToApexState() {
        if (playerMovement.YVelocity <= 0) {
            ChanegeJumpState(JumpState.apex);
        }
    }

    private void CheckTransitionToSlowDownState() {
        if (playerMovement.YVelocity <= minVelocityForSlowDown) {
            ChanegeJumpState(JumpState.slowDown);
            animator.ResetTrigger("New Jump");
        }
    }

    private void ChanegeJumpState(JumpState jumpState) {
        CurrentJumpState = jumpState;
        animator.SetInteger("Jump State", (int)jumpState);
    }

    private void SetForceDownJumpTrigger() {
        if (CurrentJumpState == JumpState.slowDown || CurrentJumpState == JumpState.apex || CurrentJumpState == JumpState.flyDown) {
            animator.SetTrigger("Force Jump");
            if (!forceTrigger) {
                playerMovement.TakeStartBoostYPosValue(CurrentJumpState);
                forceTrigger = true;
            }
        }
    }
}
