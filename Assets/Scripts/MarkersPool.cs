using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkersPool : MonoBehaviour {

    private Queue<MarkerObject> pool = new Queue<MarkerObject>();

    [SerializeField] MarkerObject marker = null;
    [SerializeField] int initCapacity = 50;

    void Awake() {
        FillPoolWithInitialCapacity();
    }

    public MarkerObject GetMarker(Vector3 pos, float scale, float timeToLife = 0) {

        return GetMarker (pos, scale, Color.black, timeToLife);
    }

    public MarkerObject GetMarker(Vector3 pos, float scale, Color color, float timeToLife = 0) {
        MarkerObject objectFromPool;
        if (pool.Count > 0) {
            objectFromPool = pool.Dequeue();
        } else {
            objectFromPool = CreateNewObject();
        }
        objectFromPool.transform.localScale = new Vector3(scale, scale, scale);
        objectFromPool.transform.position = pos;
        objectFromPool.GetComponent<MeshRenderer>().material.color = color;
        objectFromPool.gameObject.SetActive(true);
        if (timeToLife > 0) {
            objectFromPool.DestroyAfterSeconds(timeToLife);
        }
        return objectFromPool;
    }

    private void FillPoolWithInitialCapacity() {
        while (initCapacity-- > 0) {
            AddToPool(CreateNewObject());
        }
    }

    private MarkerObject CreateNewObject() {
        return Instantiate(marker, transform);
    }

    public void AddToPool(MarkerObject objectToDestroy) {
        objectToDestroy.gameObject.SetActive(false);
        objectToDestroy.transform.localScale = Vector3.one;
        pool.Enqueue(objectToDestroy);
    }
}
