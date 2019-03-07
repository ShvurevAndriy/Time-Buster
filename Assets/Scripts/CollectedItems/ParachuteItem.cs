using UnityEngine;
public class ParachuteItem : CollectedItem {

    [SerializeField] int parachuteAmount = 1;
    [SerializeField] Material playback = null;
    [SerializeField] Material normal = null;

    public ParachuteItem() : base(true) {
    }

    protected override void ActivateItem() {
        myGameManager.ChangeParachutes(parachuteAmount);
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
