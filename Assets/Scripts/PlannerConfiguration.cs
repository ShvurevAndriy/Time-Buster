using UnityEngine;
public class PlannerConfiguration : MonoBehaviour {
    [SerializeField] float freeFall = 2;
    [SerializeField] float yVelocity = -5;
    [SerializeField] float slowdownDown = 20;

    public float FreeFall { get => freeFall; private set => freeFall = value; }
    public float YVelocity { get => yVelocity; private set => yVelocity = value; }
    public float SlowdownDown { get => slowdownDown; private set => slowdownDown = value; }
}