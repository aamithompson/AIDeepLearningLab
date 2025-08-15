//==============================================================================
// Filename: EcoSpawnManager.cs
// Author: Aaron Thompson
// Date Created: 1/16/2022
// Last Updated: 5/2/2022
//
// Description:
// https://en.wikipedia.org/wiki/Polygon
// https://en.wikipedia.org/wiki/Point_in_polygon
// https://en.wikipedia.org/wiki/Even%E2%80%93odd_rule
// https://en.wikipedia.org/wiki/Shoelace_formula
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geomath;
//------------------------------------------------------------------------------
[RequireComponent(typeof(SpeciesManager))]
public class EcoSpawnManager : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    private SpeciesManager speciesManager;

    public List<Polygon> regions;
    public List<int> maxPopulations;
    public List<GameObject> environPresets;
    public List<List<GameObject>> environObjects;
    public List<int> maxEnvironCount;
    public float spawnPadding;
    public float raycastHeight;

    public bool debugEnabled = false;
    public bool debugDrawRegions = false;
    public float debugDrawHeight = 0.0f;
    public bool debugDrawRaycast = false;
    public GameObject debugTestObject;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
    void Start() { 
        speciesManager = GetComponent<SpeciesManager>();

        environObjects = new List<List<GameObject>>();
        for(int i = 0; i < environPresets.Count; i++) {
            maxEnvironCount.Add(0);
            List<GameObject> list = new List<GameObject>();
            environObjects.Add(list);
        }

        //SpawnPopulations();
        UpdateEnvironment(0);
    }

    void Update() {
        if(debugEnabled) {
            if(debugTestObject != null) {
                Debug.Log(regions[0].Contains(debugTestObject.transform.position));
            }
        }
    }

// FUNCTIONS
//------------------------------------------------------------------------------
    public void SpawnPopulation(int index, bool parallel=false) {
        float[] areasPercents = new float[regions.Count];
        areasPercents[0] = regions[0].Area(); 
        for(int i = 1; i < regions.Count; i++) {
            areasPercents[i] = regions[i].Area() + areasPercents[i-1];
        }
        for (int i = 0; i < regions.Count; i++) {
            areasPercents[i] /= areasPercents[regions.Count - 1];
        }

        int[] distribution = new int[regions.Count];
        for(int i = 0; i < maxPopulations[index]; i++) {
            float percent = Random.value;
            for(int j = 0; j < regions.Count; j++) {
                if(areasPercents[j] > percent) {
                    distribution[j]++;
                    break;
                }
            }
        }

        for(int i = 0; i < regions.Count; i++) {
            Vector2[] points = regions[i].RandomSamplePoints(distribution[i], parallel);
            for(int j = 0; j < points.Length; j++) {
                RaycastHit rHit;
                Vector3 point = new Vector3(points[j].x, raycastHeight, points[j].y);
                if(Physics.Raycast(point, Vector3.down, out rHit)) {
                    GameObject individual = speciesManager.InstantiateCreature(index, rHit.point + Vector3.up * spawnPadding);
                    individual.transform.position = individual.transform.position + Vector3.up * individual.transform.lossyScale.y/2;
                }
            }
        }
    }

    public void SpawnPopulations(bool parallel=false) {
        for(int i = 0; i < speciesManager.species.Count; i++) {
            SpawnPopulation(i, parallel);
        }
    }

    public void UpdateEnvironment(int index, bool parallel=false) {
        float[] areasPercents = new float[regions.Count];
        areasPercents[0] = regions[0].Area(); 
        for(int i = 1; i < regions.Count; i++) {
            areasPercents[i] = regions[i].Area() + areasPercents[i-1];
        }
        for (int i = 0; i < regions.Count; i++) {
            areasPercents[i] /= areasPercents[regions.Count - 1];
        }

        int[] distribution = new int[regions.Count];
        for(int i = 0; i < maxEnvironCount[index]; i++) {
            float percent = Random.value;
            for(int j = 0; j < regions.Count; j++) {
                if(areasPercents[j] > percent) {
                    distribution[j]++;
                    break;
                }
            }
        }

        for(int i = 0; i < regions.Count; i++) {
            Vector2[] points = regions[i].RandomSamplePoints(distribution[i], parallel);
            for(int j = 0; j < points.Length; j++) {
                RaycastHit rHit;
                Vector3 point = new Vector3(points[j].x, raycastHeight, points[j].y);
                if(Physics.Raycast(point, Vector3.down, out rHit)) {
                    GameObject individual = InstantiateEnvironObject(index, rHit.point + Vector3.up * spawnPadding);
                    individual.transform.position = individual.transform.position + Vector3.up * individual.transform.lossyScale.y/2;
                }
            }
        }
    }

    public GameObject InstantiateEnvironObject(int index, Vector3 position) {
        GameObject gameObject = Instantiate(environPresets[index]);
        gameObject.transform.position = position;
        gameObject.name = gameObject.name.Replace("(Clone)", "").Trim();
        environObjects[index].Add(gameObject);
        maxEnvironCount[index]++;
        return gameObject;
    }

// DEBUG FUNCTIONS
//------------------------------------------------------------------------------
    private void OnDrawGizmos() {
        if(debugEnabled){
            if(debugDrawRegions){
                Gizmos.color = Color.red;
                for(int i = 0; i < regions.Count; i++) {
                    for(int j = 0; j < regions[i].vertices.Count - 1; j++) {
                        Vector3 a = new Vector3(regions[i].vertices[j].x, debugDrawHeight, regions[i].vertices[j].y);
                        Vector3 b = new Vector3(regions[i].vertices[j+1].x, debugDrawHeight, regions[i].vertices[j+1].y);
                        Gizmos.DrawLine(a, b);
                    }

                    Vector3 end = new Vector3(regions[i].vertices[regions[i].vertices.Count - 1].x, debugDrawHeight, regions[i].vertices[regions[i].vertices.Count - 1].y);
                    Vector3 start = new Vector3(regions[i].vertices[0].x, debugDrawHeight, regions[i].vertices[0].y);
                    Gizmos.DrawLine(end, start);
                }
            }

            if(debugDrawRaycast) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(transform.position + Vector3.up * (raycastHeight - transform.position.y), Vector3.one * 1.5f + Vector3.up * 8.5f);
            }
        }
    }
}
//==============================================================================
//==============================================================================