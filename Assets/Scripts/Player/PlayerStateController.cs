using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public enum JumpState {
    landing = -1,
    flyUp = 0,
    slowDown = 1,
    apex = 2,
    flyDown = 3,
    force = 4,
    jetpack = 5,
    patachute = 6,
    deltaplan = 7
}

public class PlayerStateController : MonoBehaviour {

    public float minVelocityForSlowDown = 7;

    private MyGameManager gameManager;
    private PlayerMovement playerMovement;
    private JetpackConfiguration jetpackConfiguration;
    private Animator animator;

    private bool forceTrigger;
    private bool apexWasReached;

    public JumpState CurrentJumpState { get; set; }

    public delegate void PlayerOnJetpackOffAction();

    public event PlayerOnJetpackOffAction OnJetpackOff = delegate { };

    void Start() {
        gameManager = FindObjectOfType<MyGameManager>();
        gameManager.OnPlaybackModeOn += OnPlaybackModeOn;
        gameManager.OnPlayModeOn += OnPlayModeOn;

        animator = GetComponent<Animator>();
        jetpackConfiguration = FindObjectOfType<JetpackConfiguration>();

        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.OnPlayerJump += OnJump;
        playerMovement.OnPlayerLanding += OnLanding;

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
                CheckForParachuteActivation();
                break;
            case JumpState.slowDown:
                CheckTransitionToApexState();
                ProcessUserInput();
                CheckForParachuteActivation();
                break;
            case JumpState.apex:
            case JumpState.flyDown:
                ProcessUserInput();
                CheckForDeltaplanActivation();
                CheckForParachuteActivation();
                break;
            case JumpState.jetpack:
                if (gameManager.HasFuel()) {
                    gameManager.ChangeJetpackFuel(-jetpackConfiguration.FuelConsumptionPerSecond * Time.deltaTime);
                } else {
                    TurneOffJetpack();
                }
                ProcessUserInput();
                break;
            case JumpState.deltaplan:
                if (CrossPlatformInputManager.GetButtonDown("Fire2")) {
                    TurneOffDeltaplan();
                }
                break;
            case JumpState.patachute:
                if (CrossPlatformInputManager.GetButtonDown("Fire3")) {
                    TurneOffParachute();
                }
                break;
            case JumpState.force:
                return;
        }
    }

    public void OnFlyDownAnimation() {
        if (CurrentJumpState != JumpState.flyDown) {
            apexWasReached = true;
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
        if (CrossPlatformInputManager.GetButtonDown("Jump")) {
            SetForceDownJumpTrigger();
        }
        ProcessUserJetpackInput();
    }

    private void CheckForParachuteActivation() {
        if (!forceTrigger) {
            if (CrossPlatformInputManager.GetButtonDown("Fire3") && gameManager.UseParachute()) {
                animator.SetTrigger("Parachute");
                ChanegeJumpState(JumpState.patachute);
            }
        }
    }

    private void ProcessUserJetpackInput() {
        if (CrossPlatformInputManager.GetButtonDown("Jetpack") && gameManager.HasFuel()) {
            if (!forceTrigger) {
                animator.SetTrigger("JetPack");
                ChanegeJumpState(JumpState.jetpack);
            }
        } else if (CrossPlatformInputManager.GetButtonUp("Jetpack") && CurrentJumpState == JumpState.jetpack) {
            TurneOffJetpack();
        }
    }

    private void CheckForDeltaplanActivation() {
        if (!forceTrigger) {
            if (CrossPlatformInputManager.GetButtonDown("Fire2") && gameManager.UsePlanner()) {
                animator.SetTrigger("Deltaplan");
                ChanegeJumpState(JumpState.deltaplan);
            }
        }
    }

    private void TurneOffDeltaplan() {
        ChanegeJumpState(JumpState.flyDown);
    }

    private void TurneOffParachute() {
        animator.ResetTrigger("Parachute");
        if (playerMovement.YVelocity > 0) {
            ChanegeJumpState(JumpState.flyUp);
        } else {
            ChanegeJumpState(JumpState.flyDown);
        }
    }

    private void TurneOffJetpack() {
        animator.ResetTrigger("JetPack");
        if (playerMovement.YVelocity > 0) {
            ChanegeJumpState(JumpState.flyUp);
        } else {
            ChanegeJumpState(JumpState.flyDown);
        }
        OnJetpackOff();
    }

    private void OnPlaybackModeOn() {
        animator.SetTrigger("Play Back");
        animator.SetInteger("Jump State", -1);
    }

    private void OnPlayModeOn() {
        animator.ResetTrigger("Play Back");
        ChanegeJumpState(CurrentJumpState);
        if (CurrentJumpState == JumpState.force) {
            animator.SetTrigger("Force Jump");
        } else if (CurrentJumpState == JumpState.jetpack) {
            if (CrossPlatformInputManager.GetButton("Jetpack")) {
                animator.SetTrigger("JetPack");
            } else {
                TurneOffJetpack();
            }
        } else if (CrossPlatformInputManager.GetButton("Jetpack")) {
            ChanegeJumpState(JumpState.jetpack);
            animator.SetTrigger("JetPack");
        } else if (CurrentJumpState == JumpState.patachute) {
            animator.SetTrigger("Parachute");
        } else if (CurrentJumpState == JumpState.deltaplan) {
            animator.SetTrigger("Deltaplan");
        }
    }

    private void OnLanding() {
        apexWasReached = false;
        forceTrigger = false;
        animator.ResetTrigger("Force Jump");
        animator.ResetTrigger("JetPack");
        animator.ResetTrigger("Parachute");
        animator.ResetTrigger("Deltaplan");
        animator.SetTrigger("New Jump");
        ChanegeJumpState(JumpState.landing);
    }

    private void OnJump() {
        Debug.Log("dddddrtfrftvttvt");
        ChanegeJumpState(JumpState.flyUp);
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
                playerMovement.TakeStartBoostYPosValue(apexWasReached);
                forceTrigger = true;
            }
        }
    }
}