using UnityEngine;
public class CoinsItem : CollectedItem {

    private int coinsAmount = 1;
    [SerializeField] Material playback = null;
    [SerializeField] Material normal = null;

    public CoinsItem() : base(true) {
    }

    protected override void ActivateItem() {
        myGameManager.ChangeCoins(coinsAmount);
    }

    protected override void DeactivateItem() {
        myGameManager.ChangeCoins(-coinsAmount);
    }

    protected override void VisualizeOnPlayback() {
        GetComponent<MeshRenderer>().sharedMaterial = playback;
    }

    protected override void VisualizeOnUncollect() {
        GetComponent<MeshRenderer>().sharedMaterial = normal;
    }
}
