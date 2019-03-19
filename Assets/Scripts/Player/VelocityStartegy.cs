using UnityEngine;

namespace Game.velocity {

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