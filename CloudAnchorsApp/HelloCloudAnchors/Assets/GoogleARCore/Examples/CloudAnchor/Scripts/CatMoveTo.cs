using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatMoveTo : MonoBehaviour {

    private Vector3 startMarker;
    private Vector3 endMarker;
    private float speed = 0.2F;
    private float startTime;
    private float journeyLength;
    void Start()
    {
        journeyLength = 0;
    }
    void Update()
    {
        if(journeyLength > 0)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            this.transform.position = Vector3.Lerp(startMarker, endMarker, fracJourney);
        }
    }

    public void StartMove(Vector3 endPos)
    {
        startMarker = this.transform.position;
        endMarker = endPos;
        startTime = Time.time;
        journeyLength = Vector3.Distance(startMarker, endMarker);
    }
}
