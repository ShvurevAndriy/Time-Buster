using System.Collections.Generic;
using UnityEngine;

public class MarkersPool : MonoBehaviour
{
    private Queue<MarkerObject> pool = new Queue<MarkerObject>();

    [SerializeField] private MarkerObject markerPrefab;
    [SerializeField] private int initCapacity = 50;

    private void Awake()
    {
        for (int i = 0; i < initCapacity; i++)
        {
            ReleaseMarcker(CreateNewMarcker());
        }
    }

    public MarkerObject GetMarker(Vector3 pos, float scale, float timeToLife = 0)
    {
        return GetMarker(pos, scale, Color.black, timeToLife);
    }

    public MarkerObject GetMarker(Vector3 pos, float scale, Color color, float timeToLife = 0)
    {
        MarkerObject marcker;

        if (pool.Count > 0)
        {
            marcker = pool.Dequeue();
        }
        else
        {
            marcker = CreateNewMarcker();
        }

        marcker.transform.localScale = new Vector3(scale, scale, scale);
        marcker.transform.position = pos;
        marcker.SetColor(color);
        marcker.gameObject.SetActive(true);

        if (timeToLife > 0)
        {
            marcker.DestroyAfterSeconds(timeToLife);
        }

        return marcker;
    }

    public void ReleaseMarcker(MarkerObject marker)
    {
        marker.gameObject.SetActive(false);
        pool.Enqueue(marker);
    }

    private MarkerObject CreateNewMarcker()
    {
        var marker = Instantiate(markerPrefab, transform);
        marker.ReleaseEvent += ReleaseMarcker;
        return marker;
    }
}
