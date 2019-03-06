using System;
using UnityEngine;

public abstract class CollectedItem : MonoBehaviour {
    public readonly bool revertOnPlayback;

    protected PlayerMovement playerMovement;
    protected MyGameManager myGameManager;

    private bool activated = false;

    public float AngelWhenCollected { get; private set; }

    private void Start() {
        myGameManager = FindObjectOfType<MyGameManager>();
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    protected CollectedItem(bool revertOnPlayback) {
        this.revertOnPlayback = revertOnPlayback;
    }

    private void OnTriggerEnter(Collider other) {
        if (!activated) {
            AngelWhenCollected = playerMovement.currentAngel;
            ActivateItem();
            activated = true;
            myGameManager.AddCollectedItem(this);
            gameObject.SetActive(false);
        }
    }

    protected abstract void ActivateItem();
    protected abstract void DeactivateItem();
    protected abstract void VisualizeOnUncollect();
    protected abstract void VisualizeOnPlayback();

    public void UnCollectItem() {
        VisualizeOnUncollect();
        DeactivateItem();
        activated = false;
    }

    public void SetVisible() {
        VisualizeOnPlayback();
        gameObject.SetActive(true);
    }

    public void SetInvisible() {
        gameObject.SetActive(false);
    }
}
