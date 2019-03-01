using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerObject : MonoBehaviour {
    private MarkersPool markersPool;

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
}
