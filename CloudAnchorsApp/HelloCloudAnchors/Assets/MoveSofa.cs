using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSofa : MonoBehaviour {
    private Vector3 startMarker;
    private Vector3 endMarker;
    private float speed = 1.0F;
    private float startTime;
    private float journeyLength;
    void Start()
    {
        journeyLength = 0;
    }
    void Update()
    {
        if (journeyLength > 0)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength;
            this.transform.position = Vector3.Lerp(startMarker, endMarker, fracJourney);

            if (fracJourney < 0.1f)
            {
                var lookPos = endMarker - transform.position;
               /* lookPos.y = 0.0F;
                lookPos.x = -90.0f;*/
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 1f);
                Debug.Log("Rotation alle Achsen: " + transform.rotation);
            }
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
