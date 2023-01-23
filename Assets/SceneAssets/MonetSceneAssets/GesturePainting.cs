using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GesturePainting : MonoBehaviour
{
    private bool drawing = false;
    private LineRenderer currentLineRenderer = null;
    public Transform indexFingerTipPos = null;
    public Transform thumbTipPos = null;
    public float drawTimeStep = 0.01f;
    public Material chosenMaterial = null;
    private float nextDrawTime = 0.0f;
    private float drawStartTime = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        //lastDrawTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (drawing)
        {
            if (nextDrawTime <= Time.time)
            {
                // work out the line width
                Vector3 fingerDifVec = indexFingerTipPos.position - thumbTipPos.position;
                //currentLineRenderer.curve.AddKey(0.0f, fingerDifVec.magnitude);
                Vector3 drawPos = thumbTipPos.position + (fingerDifVec / 2.0f);

                if (currentLineRenderer != null)
                {
                        currentLineRenderer.positionCount++;
                        currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, drawPos);

                    float drawTime = Time.time - drawStartTime;

                    if (drawTime > 1.0f)
                    {
                        currentLineRenderer.widthCurve.keys[1].time = 0.5f / drawTime;
                        currentLineRenderer.widthCurve.keys[2].time = 1.0f - (0.5f / drawTime);
                    }
                }

                // update the draw time
                nextDrawTime = Time.time + drawTimeStep;
            }
        }
    }

    public void PaintingStart()
    {
        // create the new line
        GameObject newObj = new GameObject();
        currentLineRenderer = newObj.AddComponent(typeof(LineRenderer)) as LineRenderer;
        //currentLineRenderer.startColor = Color.red;
        //currentLineRenderer.startWidth = 0.1f;

        currentLineRenderer.material = chosenMaterial;
        //currentLineRenderer.widthMultiplier = 0.2f;
        currentLineRenderer.useWorldSpace = true;
        currentLineRenderer.generateLightingData = true;

        // activate drawing
        drawing = true;
        nextDrawTime = Time.time;  // force a new line start now

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, 0);
        curve.AddKey(.1f, .1f);
        curve.AddKey(.9f, .1f);
        curve.AddKey(1, 0);
        currentLineRenderer.widthCurve = curve;
        currentLineRenderer.widthMultiplier = 0.5f;
        drawStartTime = Time.time;
        currentLineRenderer.positionCount = 0;
    }

    public void PaintingStop()
    {
        // deactivate drawing
        drawing = false;
    }
}
