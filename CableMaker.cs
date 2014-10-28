//========================================
//==	Static Hanging Cable Maker      ==
//==	File: CableMaker.cs	            ==
//==	By: Ivan Notaros - Nothke	    ==
//==	Use and alter Freely	        ==
//========================================

//Description:
// This script will create a model of a hanging cable between 2 points. 
// The cable is a STATIC object that is created at start and is not meant to change during runtime. (although it can be, by calling ReDraw())
// It is simply just used to help create a hanging cable model quickly.

//How To Use:
// 1. Simply add this script to the object you want a cable going from (but see "Tip").
// 2. Assign the cableStart transform and cableEnd in the inspector. The gizmos should now show up in the Scene view.
// 3. Usually, you should just change the "segments" value as it will automatically stretch the cable and will keep the line quad resolution. Check comments for each value for more info.
// 4. Press play and your cable will be generated!

// Tip: You can attach the script to any object, it doesn't need to be the one from which the cable will start.
//      In this way, you can keep all the cable scripts in one object for easier configuration, for example if you have a utility pole from which multiple cables hang. It's up to you what method you prefer.

using UnityEngine;
using System.Collections;
using System;

public class CableMaker : MonoBehaviour
{

    public Transform cableStart; // Point A
    public Transform cableEnd; // Point B
    public Material cableMaterial;

    public int segments = 4; // number of cable segments. This also stretches the cable.
    public float relax = 6; // How much will the cables "relax". Use it in conjunction with segments.
    public float heightScale = 1; // Multiplies height. DO NOT CHANGE this value as it will "squash" the catenary, unless cables look very awkward.
    public float width = 0.05f; // Width of the cable (line renderer)
    public bool drawGizmos = true; // Disables gizmos in scene view
    public bool simpleGizmos; // Setting simpleGizmos to true will render gizmos in scene view as a simple line between points instead of the whole catenary. It might be useful in a scene with a lot of cables if fps drops.

    LineRenderer line;

    float firstPosDiff;
    float startingLineY;

    void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        if (!cableStart)
            return;

        if (cableEnd) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(cableStart.position, width / 2);

        if (cableEnd)
        {

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(cableEnd.position, width / 2);

            if (simpleGizmos)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(cableStart.position, cableEnd.position);
            }
            else
            {
                Vector3[] gizmosCatenary = CreateCatenary();

                for (int i = 0; i < gizmosCatenary.Length - 1; i++)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(gizmosCatenary[i], gizmosCatenary[i + 1]);
                    //Gizmos.color = Color.grey;
                    if (i != 0) Gizmos.DrawWireSphere(gizmosCatenary[i], width / 2);
                }
            }
        }
    }

    void Start()
    {
        DrawLine(CreateCatenary());
    }

    /// <summary>
    /// Redraws the line
    /// </summary>
    public void ReDraw()
    {
        DrawLine(CreateCatenary());
    }

    void DrawLine(Vector3[] positions)
    {
        if (!line)
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.material = cableMaterial;
            line.SetWidth(width, width);
        }

        line.SetVertexCount(positions.Length);

        for (int i = 0; i < positions.Length; i++)
        {
            line.SetPosition(i, positions[i]);
        }
    }

    // TODO: Another renderer could be added here, for example a TubeRenderer

    Vector3[] CreateCatenary()
    {
        // Make sure segments are never 0 or below
        if (segments < 1) segments = 1;

        // Make sure segments are even
        int linePositionsNum = segments * 2 + 1;

        // Find height difference between points
        float deltaHeight = cableStart.position.y - cableEnd.position.y;

        // Getting a vector between points
        Vector3 lineVector = cableEnd.position - cableStart.position;

        // Create an array of positions
        Vector3[] linePositions = new Vector3[linePositionsNum];


        for (int i = 0; i < linePositionsNum; i++)
        {
            // Aligning all positions on the vector at equal distances
            Vector3 linePos = cableEnd.position - ((lineVector / (linePositionsNum - 1)) * i);

            // Applying the catenary formula
            linePos.y = -relax + (heightScale * (relax * (float)Math.Cosh((i - (linePositionsNum / 2)) / relax)));

            // Removing height difference of the first point
            if (i == 0)
            {
                startingLineY = linePos.y;
                firstPosDiff = cableEnd.position.y - startingLineY;
            }
            linePos.y += firstPosDiff;

            // Stepping the height to match the height difference between two points
            linePos.y += i * (deltaHeight / (linePositionsNum - 1));

            // Add a position to the array
            linePositions[i] = linePos;
        }

        return linePositions;
    }
}