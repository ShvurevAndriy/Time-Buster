using System;
using System.Collections;
using UnityEngine;

public class MarkerObject : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    public event Action<MarkerObject> ReleaseEvent;

    public void DestroyAfterSeconds(float timeToLive)
    {
        StartCoroutine(ScheduleDestryTime(timeToLive));
    }

    public void DestroyNow()
    {
        ReleaseEvent?.Invoke(this);
    }

    private IEnumerator ScheduleDestryTime(float timeToLive)
    {
        yield return new WaitForSeconds(timeToLive);
        ReleaseEvent?.Invoke(this);
    }

    public void SetColor(Color color)
    {
        if (!meshRenderer)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        meshRenderer.material.color = color;
    }
}
