//==============================================================================
// Filename: EnvironmentManager.cs
// Author: Aaron Thompson
// Date Created: 1/31/2022
// Last Updated: 1/31/2022
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geomath;
//------------------------------------------------------------------------------
public class EnvironmentManager : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    public string waterTag="Water";
    public string terrainTag = "Terrain";
    public float pointRadius;
    public float maxDistanceBetweenPoints;
    public float rdpEpsilon=0.5f;

    public Vector3[] waterPoints;

    public bool debugEnabled = false;
    public bool debugDrawWaterPoints = false;

// MONOBEHAVIOR FUNCTIONS
//------------------------------------------------------------------------------
    void Awake () {
        GenerateWaterPoints();
    }

// FUNCTIONS
//------------------------------------------------------------------------------
    private void GenerateWaterPoints(){
        if(pointRadius <= 0 || maxDistanceBetweenPoints <= 0) {
            return;
        }

        List<Vector3> points = new List<Vector3>();
        GameObject[] water = GameObject.FindGameObjectsWithTag(waterTag);

        for(int i = 0; i < water.Length; i++) {
            List<Vector2> points2D = new List<Vector2>();
            List<Polyline> polylines = new List<Polyline>();
            Vector3 offset = water[i].transform.position;
            Vector3 size = water[i].GetComponent<MeshRenderer>().bounds.size;
            float boundsLeft = offset.x - (size.x/2);
            float boundsRight = offset.x + (size.x/2);
            float boundsBottom = offset.z - (size.z/2);
            float boundsTop = offset.z + (size.z/2);
            float diameter = pointRadius * 2;
            float sqrDistance = maxDistanceBetweenPoints * maxDistanceBetweenPoints;

            for(float x = boundsLeft + pointRadius; x <= boundsRight; x+=diameter) {
                for(float y = boundsBottom + pointRadius; y <= boundsTop; y += diameter) {
                    Vector3 position = new Vector3(x, offset.y, y);
                    Collider[] collisions = Physics.OverlapSphere(position, pointRadius);
                    for(int j = 0; j < collisions.Length; j++) {
                        //If the water layer IS NOT a layer contained in the object's layers
                        if(collisions[j].gameObject.tag == terrainTag) {
                            Vector2 position2D = new Vector2(position.x, position.z);
                            if(points2D.Count > 0) {
                                int n = points2D.Count - 1;
                                if(Vector2.SqrMagnitude(position2D - points2D[n]) > sqrDistance) {
                                    Polyline line = new Polyline(points2D);
                                    line.SimplifyRDP(rdpEpsilon);
                                    polylines.Add(line);
                                    points2D.Clear();
                                }
                            }

                            points2D.Add(position2D);
                            break;
                        }
                    }
                }
            }

            for(int j = 0; j < polylines.Count; j++) {
                for(int k = 0; k < polylines[j].points.Count; k++) {
                    points.Add(new Vector3(polylines[j].points[k].x, offset.y, polylines[j].points[k].y));
                }
            }
        }

        waterPoints = points.ToArray();
    }

// DEBUG FUNCTIONS
//------------------------------------------------------------------------------
     private void OnDrawGizmos() {
        if(debugEnabled) {
            if(debugDrawWaterPoints) {
                for(int i = 0; i < waterPoints.Length; i++) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(waterPoints[i], pointRadius);
                }
            }
        }
     }
}
//==============================================================================
//==============================================================================
