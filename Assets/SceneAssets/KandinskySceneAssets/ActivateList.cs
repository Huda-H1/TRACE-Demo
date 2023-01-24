using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateList : MonoBehaviour
{
    public List<GameObject> objectsToActivate = new List<GameObject>();
    public int nextObjectToActivate = 0;
    public bool loop = false;
    public UnityEvent functionToCallOnDone = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateNext()
    {
        if (nextObjectToActivate >= 0)
        {
            if (nextObjectToActivate >= objectsToActivate.Count) // if the list has been fully traversed
                if (loop)
                    nextObjectToActivate = 0;  // if it should loop, then reset the list
                else
                    if (functionToCallOnDone != null) // if it should not loop but has a function to call at the end, call it
                        functionToCallOnDone.Invoke();


            if (nextObjectToActivate < objectsToActivate.Count)
            {
                objectsToActivate[nextObjectToActivate].SetActive(true);
                nextObjectToActivate++;
            }
        }
    }

    public void ActivateAll()
    {
        nextObjectToActivate = 0;

        while (nextObjectToActivate < objectsToActivate.Count)
        {
            objectsToActivate[nextObjectToActivate].SetActive(true);
            nextObjectToActivate++;
        }

        if (functionToCallOnDone != null) // if it should not loop but has a function to call at the end, call it
            functionToCallOnDone.Invoke();
    }
}
