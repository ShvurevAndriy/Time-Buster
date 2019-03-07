﻿using UnityEngine;
public class CoinsItem : CollectedItem {

    [SerializeField] int coinsAmount = 100;
    [SerializeField] Material playback = null;
    [SerializeField] Material normal = null;

    public CoinsItem() : base(true) {
    }

    protected override void ActivateItem() {
        myGameManager.ChangeScore(coinsAmount);
    }

    protected override void DeactivateItem() {
        myGameManager.ChangeScore(-coinsAmount);
    }

    protected override void VisualizeOnPlayback() {
        GetComponent<MeshRenderer>().sharedMaterial = playback;
    }

    protected override void VisualizeOnUncollect() {
        GetComponent<MeshRenderer>().sharedMaterial = normal;
    }
}
