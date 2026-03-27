//==============================================================================
// Filename: SimulationConfig.cs
// Author: Aaron Thompson
// Date Created: 3/24/2026
// Last Updated: 3/24/2026
//
// Description: Settings for the simulation.
//==============================================================================
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
//------------------------------------------------------------------------------
public class SimulationConfig : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    //Population
    public List<int> populationCounts;
    public List<string> populationRecordFolders;
    public List<string> populationRecordFileNames;

    //Genetic Algorithm
    public List<EcoTrainer.SpeciesAlgorithmSettings> speciesAlgorithmSettings;

    //Ecosystem
    public float waterPointRadius;
    public float waterPointMaxDistance;

    //Pathfinding
    public PathfindingAlgorithm pathfindingAlgorithm;
    public float pathfindingTileRadius;
    public float pathfindingTileHeight;

    //Debug
    public bool showPathfindingTiles;
    public bool showPathfindingPaths;
    public bool showWaterPoints;
    public bool showSpawnRegions;
    public bool showSightCones;
    public bool showHearingCones;
    public bool showSoundEvents;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
