using UnityEngine;

public class ParachuteConfiguration : MonoBehaviour {
    [SerializeField] float xVelocity = 3;
    [SerializeField] float xAccel = 20;
    [SerializeField] float slowdownUp = 40;
    [SerializeField] float yVelocity = -2;
    [SerializeField] float freeFall = 10;
    [SerializeField] float slowdownDown = 20;

    public float SlowdownUp { get => slowdownUp; private set => slowdownUp = value; }
    public float YVelocity { get => yVelocity; private set => yVelocity = value; }
    public float FreeFall { get => freeFall; private set => freeFall = value; }
    public float SlowdownDown { get => slowdownDown; private set => slowdownDown = value; }
    public float XVelocity { get => xVelocity; private set => xVelocity = value; }
    public float XAccel { get => xAccel; private set => xAccel = value; }
}