using UnityEngine;

public class ReplayData {

    public Vector3 posotion;
    public float jumpHeight;
    public float currentAngel;
    public float angularSpeed;
    public float yVelocity;
    public float timeScale;
    public JumpState jumpState;
    public float startBoostYPos;
    public float apexYPos;
    public float cameraZoom;
    public float jetpackFuel;
    public int planners;
    public int parachutes;

    public static Builder builder() {
        return new Builder();
    }

    public class Builder {
        private Vector3 posotion;
        private float jumpHeight;
        private float currentAngel;
        private float angularSpeed;
        public float yVelocity;
        private float timeScale;
        private JumpState jumpState;
        private float startBoostYPos;
        private float apexYPos;
        private float cameraZoom;
        private float jetpackFuel;
        private int planners;
        private int parachutes;

        public Builder setPosition(Vector3 posotion) {
            this.posotion = posotion;
            return this;
        }
        public Builder setJumpHeight(float jumpHeight) {
            this.jumpHeight = jumpHeight;
            return this;
        }
        public Builder setStartBoostYPos(float startBoostYPos) {
            this.startBoostYPos = startBoostYPos;
            return this;
        }
        public Builder setApexYPos(float apexYPos) {
            this.apexYPos = apexYPos;
            return this;
        }
        public Builder setJumpState(JumpState jumpState) {
            this.jumpState = jumpState;
            return this;
        }

        public Builder setCameraZoom(float cameraZoom) {
            this.cameraZoom = cameraZoom;
            return this;
        }
        public Builder setYVelocity(float yVelocity) {
            this.yVelocity = yVelocity;
            return this;
        }
        public Builder setAngularSpeed(float angularSpeed) {
            this.angularSpeed = angularSpeed;
            return this;
        }
        public Builder setCurrentAngel(float currentAngel) {
            this.currentAngel = currentAngel;
            return this;
        }
        public Builder setTimeScale(float timeScale) {
            this.timeScale = timeScale;
            return this;
        }

        public Builder setJetpackFuel(float jetpackFuel) {
            this.jetpackFuel = jetpackFuel;
            return this;
        }

        public Builder setParachutes(int parachutes) {
            this.parachutes = parachutes;
            return this;
        }

        public Builder setPlanners(int planners) {
            this.planners = planners;
            return this;
        }

        public ReplayData build() {
            ReplayData replayData = new ReplayData();
            replayData.posotion = posotion;
            replayData.yVelocity = yVelocity;
            replayData.jumpHeight = jumpHeight;
            replayData.angularSpeed = angularSpeed;
            replayData.timeScale = timeScale;
            replayData.jumpState = jumpState;
            replayData.startBoostYPos = startBoostYPos;
            replayData.apexYPos = apexYPos;
            replayData.cameraZoom = cameraZoom;
            replayData.currentAngel = currentAngel;
            replayData.jetpackFuel = jetpackFuel;
            replayData.parachutes = parachutes;
            replayData.planners = planners;
            return replayData;
        }
    }
}
