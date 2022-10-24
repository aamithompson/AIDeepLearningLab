//==============================================================================
// Filename: Polygon.cs
// Author: Aaron Thompson
// Date Created: 1/31/2022
// Last Updated: 1/31/2022
//
// Description: A class for closed polygons which has functionality for
// collision detection, sampling, and general computations for shapes.
// https://en.wikipedia.org/wiki/Polygon
// https://en.wikipedia.org/wiki/Point_in_polygon
// https://en.wikipedia.org/wiki/Even%E2%80%93odd_rule
// https://en.wikipedia.org/wiki/Shoelace_formula
//==============================================================================
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using statistics;
//------------------------------------------------------------------------------
namespace geomath {
[System.Serializable]
public class Polygon {
// VARIABLES
//------------------------------------------------------------------------------
    public List<Vector2> vertices;

// CONSTRUCTORS/DESTUCTORS
//------------------------------------------------------------------------------
    public Polygon() {
        vertices = new List<Vector2>();
    }

    public Polygon(Vector2[] vertices) { 
        this.vertices = new List<Vector2>();
        for(int i = 0; i < vertices.Length; i++) {
            this.vertices.Add(vertices[i]);
        }
    }

    public Polygon(List<Vector2> vertices) { 
        this.vertices = new List<Vector2>();
        for(int i = 0; i < vertices.Count; i++) {
            this.vertices.Add(vertices[i]);
        }
    }

// NUMERICAL COMPUTATIONS
//------------------------------------------------------------------------------
    public float Area() {
        float area = 0;
        int n = vertices.Count;
        for(int i = 0; i < n - 1; i++) {
            area += (vertices[i].x * vertices[i+1].y) - (vertices[i+1].x * vertices[i].y);
        }
        area += (vertices[n-1].x * vertices[0].y) - (vertices[0].x * vertices[n-1].y);

        return System.Math.Abs(area/2);
    }

// COLLISION DETECTION
//------------------------------------------------------------------------------
    public bool Contains(float x, float y) {
        int j = vertices.Count - 1;
        bool odd = false;

        for(int i = 0; i < vertices.Count; i++) {
            if(x == vertices[i].x && y == vertices[i].y) {
                return true;
            }

            if((vertices[i].y > y) != (vertices[j].y > y)) {
                float slope = (x - vertices[i].x) * (vertices[j].y - vertices[i].y) - (vertices[j].x - vertices[i].x) * (y - vertices[i].y);
                if(slope == 0) {
                    return true;
                }

                if((slope < 0) != (vertices[j].y < vertices[i].y)) {
                    odd = !odd;
                }
            }

            j = i;
        }

        return odd;
    }

    public bool Contains(Vector2 point) {
        return Contains(point.x, point.y);
    }

    public bool Contains(Vector3 point) {
        return Contains(point.x, point.z);
    }

// SAMPLING
//------------------------------------------------------------------------------
    public Vector2 RandomSamplePoint(bool parallel=false) {
        float top = vertices[0].y;
        float bottom = vertices[0].y;
        float left = vertices[0].x;
        float right = vertices[0].x;

        for(int i = 0; i < vertices.Count; i++) {
            if(top > vertices[i].y) { top = vertices[i].y; }
            if(bottom < vertices[i].y) { bottom = vertices[i].y; }
            if(left < vertices[i].x) { left = vertices[i].x; }
            if(right > vertices[i].x) { right = vertices[i].x; }
        }

        Vector2 sample = (parallel) ? new Vector2(ParallelRandom.NextFloat(left, right), ParallelRandom.NextFloat(bottom, top)) : new Vector2(Random.Range(left, right), Random.Range(bottom, top));
        while(!Contains(sample)) {
            sample = (parallel) ? new Vector2(ParallelRandom.NextFloat(left, right), ParallelRandom.NextFloat(bottom, top)) : new Vector2(Random.Range(left, right), Random.Range(bottom, top));
        }

        return sample;
    }

    public Vector2[] RandomSamplePoints(int n, bool parallel=false) {
        Vector2[] points = new Vector2[n];
        if(parallel) {
            Parallel.For(0, n, i => {
                points[i] = RandomSamplePoint(parallel);
            });
        } else {
            for(int i = 0; i < n; i++) {
                points[i] = RandomSamplePoint(parallel);
            }
        }

        return points;
    }
}
} //END namespace geomath
//==============================================================================
//==============================================================================