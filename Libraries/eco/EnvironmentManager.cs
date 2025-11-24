//==============================================================================
// Filename: EnvironmentManager.cs
// Author: Aaron Thompson
// Date Created: 1/31/2022
// Last Updated: 11/14/2025
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
    public float raycastMaxDistance;
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
    /*private void GenerateWaterPoints()
    {
        if (pointRadius <= 0 || maxDistanceBetweenPoints <= 0)
        {
            return;
        }

        List<Vector3> points = new List<Vector3>();
        GameObject[] water = GameObject.FindGameObjectsWithTag(waterTag);

        for (int i = 0; i < water.Length; i++)
        {
            List<Vector2> points2D = new List<Vector2>();
            List<Polyline> polylines = new List<Polyline>();
            Vector3 offset = water[i].transform.position;
            Vector3 size = water[i].GetComponent<MeshRenderer>().bounds.size;
            float boundsLeft = offset.x - (size.x / 2);
            float boundsRight = offset.x + (size.x / 2);
            float boundsBottom = offset.z - (size.z / 2);
            float boundsTop = offset.z + (size.z / 2);
            float diameter = pointRadius * 2;
            float sqrDistance = maxDistanceBetweenPoints * maxDistanceBetweenPoints;

            for (float x = boundsLeft + pointRadius; x <= boundsRight; x += diameter)
            {
                for (float y = boundsBottom + pointRadius; y <= boundsTop; y += diameter)
                {
                    Vector3 position = new Vector3(x, offset.y, y);
                    Collider[] collisions = Physics.OverlapSphere(position, pointRadius);
                    for (int j = 0; j < collisions.Length; j++)
                    {
                        //If the water layer IS NOT a layer contained in the object's layers
                        if (collisions[j].gameObject.tag == terrainTag)
                        {
                            Vector2 position2D = new Vector2(position.x, position.z);
                            if (points2D.Count > 0)
                            {
                                int n = points2D.Count - 1;
                                if (Vector2.SqrMagnitude(position2D - points2D[n]) > sqrDistance)
                                {
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

            for (int j = 0; j < polylines.Count; j++)
            {
                for (int k = 0; k < polylines[j].points.Count; k++)
                {
                    points.Add(new Vector3(polylines[j].points[k].x, offset.y, polylines[j].points[k].y));
                }
            }
        }

        waterPoints = points.ToArray();
    }*/

    //Marching Squares
    private void GenerateWaterPoints(){
        List<Vector3> results = new List<Vector3>();
        //Lookup Table: (Case, Rotations)
        //Cases: 0 = empty, 1 = corner, 2 = edge, 3 = saddle
        (int, int)[] lookup = new (int, int)[16] {
            (0, 0), //0:  0000 (all below)
            (1, 0), //1:  0001 (corner BL)
            (1, 1), //2:  0010 (corner BR)
            (2, 0), //3:  0011 (edge bottom)
            (1, 2), //4:  0100 (corner TR)
            (3, 0), //5:  0101 (saddle)
            (2, 1), //6:  0110 (edge right)
            (1, 3), //7:  0111 (corner TL)
            (1, 3), //8:  1000 (corner TL)
            (2, 3), //9:  1001 (edge left)
            (3, 0), //10: 1010 (saddle)
            (1, 1), //11: 1011 (corner BR)
            (2, 2), //12: 1100 (edge top)
            (1, 2), //13: 1101 (corner TR)
            (1, 0), //14: 1110 (corner BL)
            (0, 0)  //15: 1111 (all above)
        };

        GameObject[] waterObjects = GameObject.FindGameObjectsWithTag(waterTag);
        int n = waterObjects.Length;
        for(int k = 0; k < n; k++) {
            //Tile and Vertex Setup
            Vector3 offset = waterObjects[k].transform.position;
            Vector3 size = waterObjects[k].GetComponent<MeshRenderer>().bounds.size;
            Debug.Log(offset);
            Debug.Log(size);  
            float height = offset.y;
            float boundsLeft = offset.x - (size.x / 2);
            float boundsRight = offset.x + (size.x / 2);
            float boundsBottom = offset.z - (size.z / 2);
            float boundsTop = offset.z + (size.z / 2);
            float diameter = pointRadius * 2;
            int verticesX = (int) (size.x / diameter) + 1;
            int verticesZ = (int) (size.z / diameter) + 1;
            bool[,] isAbove = new bool[verticesX, verticesZ]; //If terrain at point is above, then true. Default is false.
            for(int i  = 0; i < verticesX; i++) { 
                for(int j = 0; j < verticesZ; j++) {
                    float x = boundsLeft + i * diameter;
                    float z = boundsBottom + j * diameter;
                    Vector3 origin = new Vector3(x, height + raycastMaxDistance + 0.001f, z);
                    isAbove[i, j] = Physics.Raycast(origin, Vector3.down, raycastMaxDistance);
                    /*if(isAbove[i, j]) {
                        Debug.DrawLine(origin, origin + Vector3.up * raycastMaxDistance, Color.green, 180f);
                    } else { 
                        Debug.DrawLine(origin, origin + Vector3.up * raycastMaxDistance, Color.red, 180f);
                    }*/
                }
            }

            //Case and Rotation Setup
            int[,] tileCases = new int[verticesX - 1, verticesZ - 1];
            int[,] tileRotations = new int[verticesX - 1, verticesZ - 1];
            for(int i = 0; i < verticesX - 1; i++) {
                for(int j = 0; j < verticesZ - 1; j++) {
                    int config = 0;

                    if(isAbove[i, j]) {
                        config |= 8;
                    }
                    
                    if(isAbove[i+1, j]) {
                        config |= 4;
                    }

                    if(isAbove[i+1, j+1]) {
                        config |= 2;
                    }

                    if(isAbove[i, j+1]) {
                        config |= 1;
                    }

                    tileCases[i, j] = lookup[config].Item1;
                    tileRotations[i, j] = lookup[config].Item2;
                }
            }

            //Spawning Drinking Points
            for(int i = 0; i < verticesX - 1; i++) {
                for(int j = 0; j < verticesZ - 1; j++) {
                    float x = boundsLeft + pointRadius + (i * diameter);
                    float z = boundsBottom + pointRadius + (j * diameter);
                    Vector2 center = new Vector2(x, z);

                    List<Vector2> points = new List<Vector2>();
                    switch(tileCases[i, j]) {
                        case 1:
                            points.Add(center + (Vector2.left * pointRadius));
                            points.Add(center + (Vector2.up * pointRadius));
                            break;
                        case 2:
                            points.Add(center + (Vector2.left * pointRadius));
                            points.Add(center + (Vector2.right * pointRadius));
                            break;
                        case 3:
                            points.Add(center);
                            break;
                        default:
                            break;
                    }

                    for(int t = 0; t < points.Count; t++) {
                        points[t] = RotatePoint(points[t], center, tileRotations[i, j]);
                        results.Add(new Vector3(points[t].x, height, points[t].y));
                    }
                }
            }
        }

        waterPoints = results.ToArray();
    }

    //Helper for Marching Squares
    //rotates points 90 degrees clockwise n times.
    private Vector2 RotatePoint(Vector2 point, Vector2 center, int rotations) {
        Vector2 local = point - center;
        for(int i = 0; i < rotations; i++) {
            local = new Vector2(local.y, -local.x);
        }

        return local + center;
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
