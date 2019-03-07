using UnityEngine;

public class JetpackConfiguration : MonoBehaviour {
    [SerializeField] float jetThrust = 15f;
    [SerializeField] float jetMaxVelocity = float.PositiveInfinity;
    [SerializeField] float maxJetPackFuel = 200;
    [SerializeField] float fuelConsumptionPerSecond = 100;

    public float JetMaxVelocity { get => jetMaxVelocity; private set => jetMaxVelocity = value; }
    public float JetThrust { get => jetThrust; private set => jetThrust = value; }
    public float MaxJetPackFuel { get => maxJetPackFuel; private set => maxJetPackFuel = value; }
    public float FuelConsumptionPerSecond { get => fuelConsumptionPerSecond; private set => fuelConsumptionPerSecond = value; }
}