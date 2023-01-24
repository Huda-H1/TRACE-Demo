using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveTo : MonoBehaviour
{
    public Transform startLocation = null;
    public Transform endLocation = null;
    public float transitTime = 2.0f;
    public float currentProgress = 0.0f;
    public UnityEvent functionToCallOnDone = null;

    private float startTime = 0.0f;
    private bool endReached = false;

    // Start is called before the first frame update
    void Start()
    {
        if (startLocation == null)
        {
            startLocation = gameObject.transform;
        }
        if (endLocation == null)
        {
            currentProgress = 1.0f;
        }

        startTime = Time.time;
        endReached = false;
    }

    void OnEnable()
    {
        startTime = Time.time;
        endReached = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentProgress < 1.0f)
        {
            if (endLocation != null)
            {
                // transition to the destination
                currentProgress = (Time.time - startTime) / transitTime;

                if (currentProgress > 1.0f)
                    currentProgress = 1.0f;

                gameObject.transform.localScale = Vector3.Lerp(startLocation.localScale, endLocation.localScale, currentProgress);
                gameObject.transform.position = Vector3.Lerp(startLocation.position, endLocation.position, currentProgress);
                gameObject.transform.eulerAngles = Vector3.Lerp(startLocation.eulerAngles, endLocation.eulerAngles, currentProgress);

            }
        }
        else
        {
            if (!endReached)
            {
                if (functionToCallOnDone != null)
                    functionToCallOnDone.Invoke();
                endReached = true;
            }
        }
    }
}
