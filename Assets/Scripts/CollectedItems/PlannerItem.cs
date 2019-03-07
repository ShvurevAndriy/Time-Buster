using UnityEngine;
public class PlannerItem : CollectedItem {

    [SerializeField] int plannerAmount = 1;
    [SerializeField] Material playback = null;
    [SerializeField] Material normal = null;

    public PlannerItem() : base(true) {
    }

    protected override void ActivateItem() {
        myGameManager.ChangeParachutes(plannerAmount);
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
