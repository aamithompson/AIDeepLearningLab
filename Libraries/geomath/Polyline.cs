//==============================================================================
// Filename: Polyline.cs
// Author: Aaron Thompson
// Date Created: 1/31/2022
// Last Updated: 1/31/2022
//
// Description: A class for polylines or more formally known as polygonal
// chains. This is represented as a sequence of points.
// https://en.wikipedia.org/wiki/Polygonal_chain
// https://en.wikipedia.org/wiki/Curve_fitting
// https://en.wikipedia.org/wiki/Cartographic_generalization
// https://en.wikipedia.org/wiki/Ramer%E2%80%93Douglas%E2%80%93Peucker_algorithm
// https://en.wikipedia.org/wiki/Visvalingam%E2%80%93Whyatt_algorithm
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
namespace geomath {
[System.Serializable]
public class Polyline {
// VARIABLES
//------------------------------------------------------------------------------
    public List<Vector2> points;

// CONSTRUCTORS/DESTUCTORS
//------------------------------------------------------------------------------
    public Polyline() {
        points = new List<Vector2>();
    }

    public Polyline(Vector2[] vertices) { 
        this.points = new List<Vector2>();
        for(int i = 0; i < vertices.Length; i++) {
            this.points.Add(vertices[i]);
        }
    }

    public Polyline(List<Vector2> vertices) { 
        this.points = new List<Vector2>();
        for(int i = 0; i < vertices.Count; i++) {
            this.points.Add(vertices[i]);
        }
    }

// SIMPLIFICATION
//------------------------------------------------------------------------------
    //Ramer-Douglas-Peucker algorithm
    public void SimplifyRDP(float epsilon = 0.01f) {
        points = RDPRecursion(points, epsilon);
    }

    public List<Vector2> RDPRecursion(List<Vector2> points, float epsilon = 0.01f) {
        float maxDistance = 0;
        int index = 0;
        int end = points.Count - 1;
        for(int i = 1; i < end - 1; i++) {
            float distance = PerpendicularDistance(i, 0, end);
            if(distance > maxDistance) {
                index = i;
                maxDistance = distance;
            }
        }

        List<Vector2> results = new List<Vector2>();
        if(maxDistance > epsilon) {
            // Results 1 <- [0, . . ., index]
            // Results 2 <- [index, . . ., end]
            List<Vector2> recursiveResults1 = RDPRecursion(points.GetRange(0, index), epsilon);
            List<Vector2> recursiveResults2 = RDPRecursion(points.GetRange(index - 1, (end - 1) - index), epsilon);

            results.AddRange(recursiveResults1.GetRange(0, recursiveResults1.Count - 1));
            results.AddRange(recursiveResults2);
        } else {
            results.Add(points[0]);
            results.Add(points[end]);
        }

        return results;
    }

    //Visvalingam-Whyatt algorithm
    public void SimplifyVW() {

    }

// HELPER FUNCTIONS
//------------------------------------------------------------------------------
    private float PerpendicularDistance(int i, int start, int end){
        float x0 = points[i].x;
        float y0 = points[i].y;
        float x1 = points[start].x;
        float y1 = points[start].y;
        float x2 = points[end].x;
        float y2 = points[end].y;

        float n = (x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1);
        float d = Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2);

        return Mathf.Abs(n)/Mathf.Sqrt(d);
    }
}
}//END namespace geomath
//==============================================================================
//==============================================================================