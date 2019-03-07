using UnityEngine;
public class FuelItem : CollectedItem {

    [SerializeField] int fuelAmount = 50;
    [SerializeField] Material playback = null;
    [SerializeField] Material normal = null;

    public FuelItem() : base(true) {
    }

    protected override void ActivateItem() {
        myGameManager.ChangeJetpackFuel(fuelAmount);
    }

    protected override void DeactivateItem() {
    }

    protected override void VisualizeOnPlayback() {
        GetComponent<MeshRenderer>().sharedMaterial = playback;
    }

    protected override void VisualizeOnUncollect() {
        GetComponent<MeshRenderer>().sharedMaterial = normal;
    }
}
