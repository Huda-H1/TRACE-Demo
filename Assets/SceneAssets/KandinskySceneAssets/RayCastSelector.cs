using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastSelector : MonoBehaviour
{
    public float selectorRayLength = 10.0f;
    public LayerMask selectionLayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rayCastSourcePos = transform.position;

        Ray selectorRay = new Ray(rayCastSourcePos, transform.forward);
        RaycastHit selectionHit;

        bool hit = Physics.Raycast(selectorRay, out selectionHit, selectorRayLength, selectionLayer);

        if (hit)
        {
            selectionHit.transform.gameObject.GetComponent<AudioSource>().enabled = true;
        }
    }
}
