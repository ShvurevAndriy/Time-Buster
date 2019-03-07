using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerObject : MonoBehaviour {
    private MarkersPool markersPool;
    private MeshRenderer meshRenderer;

    private void Start() {
        markersPool = FindObjectOfType<MarkersPool>();
    }

    public void DestroyAfterSeconds(float timeToLive) {
        StartCoroutine(ScheduleDestryTime(timeToLive));
    }

    public void DestroyNow() {
        markersPool.AddToPool(this);
    }

    private IEnumerator ScheduleDestryTime(float timeToLive) {
        yield return new WaitForSeconds(timeToLive);
        DestroyNow();
    }

    public void SetColor(Color color) {
        if (!meshRenderer) {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        meshRenderer.material.color = color;
    }
}
