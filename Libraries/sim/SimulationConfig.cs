//==============================================================================
// Filename: SimulationConfig.cs
// Author: Aaron Thompson
// Date Created: 3/24/2026
// Last Updated: 4/2/2026
//
// Description: Settings for the simulation.
//==============================================================================
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using adl.genetics;
using UnityEngine.UI;
using statistics;
//------------------------------------------------------------------------------
public class SimulationConfig : MonoBehaviour {
// VARIABLES (BASE)
//------------------------------------------------------------------------------
    private const int _SPECIESCOUNT = 4;
    public static SimulationConfig Instance { get; private set; }

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

// VARIABLES (UI)
//------------------------------------------------------------------------------
    public Button backButton;
    public Button acceptButton;

    //Population
    public List<TMP_InputField> populationCountTMPs;
    public List<TMP_InputField> populationRecordFoldersTMPs;
    public List<TMP_InputField> populationRecordFileNamesTMPs;

    //Genetic Algorithm
    public List<TMP_Dropdown> distributionTypeTMPDrops;
    public List<TMP_InputField> minWeightTMPs;
    public List<TMP_InputField> maxWeightTMPs;
    public List<TMP_InputField> minBiasTMPs;
    public List<TMP_InputField> maxBiasTMPs;
    public List<TMP_InputField> minWeightMutationTMPs;
    public List<TMP_InputField> maxWeightMutationTMPs;
    public List<TMP_InputField> minBiasMutationTMPs;
    public List<TMP_InputField> maxBiasMutationTMPs;

    //Ecosystem
    public TMP_InputField waterPointRadiusTMP;
    public TMP_InputField waterPointMaxDistanceTMP;

    //Pathfinding
    public TMP_Dropdown pathfindingAlgorithmTMPDrop;
    public TMP_InputField pathfindingTileRadiusTMP;
    public TMP_InputField pathfindingTileHeightTMP;

    //Debug
    public Toggle showPathfindingTilesToggle;
    public Toggle showPathfindingPathsToggle;
    public Toggle showWaterPointsToggle;
    public Toggle showSpawnRegionsToggle;
    public Toggle showSightConesToggle;
    public Toggle showHearingConesToggle;
    public Toggle showSoundEventsToggle;

// MONOBEHAVIOR FUNCTION(s)
//------------------------------------------------------------------------------
    void Start() {
        if(Instance != null) {
            LoadFromInstance();
        } else {
            Initialize();
        }

        Instance = this;
    }

// DATA MANIPULATION FUNCTION(s)
//------------------------------------------------------------------------------
    public void Initialize() {
        populationCounts = new List<int>();
        populationRecordFileNames = new List<string>();
        populationRecordFolders = new List<string>();
        speciesAlgorithmSettings = new List<EcoTrainer.SpeciesAlgorithmSettings>();

        for(int i = 0; i < _SPECIESCOUNT; i++) {
            populationCounts.Add(0);
            populationRecordFileNames.Add("");
            populationRecordFolders.Add("");
            speciesAlgorithmSettings.Add(new EcoTrainer.SpeciesAlgorithmSettings());
        }

        speciesAlgorithmSettings.Add(new EcoTrainer.SpeciesAlgorithmSettings());
    }
    
    public void LoadFromInstance() {
        populationCounts = Instance.populationCounts;
        populationRecordFolders = Instance.populationRecordFolders;
        populationRecordFileNames = Instance.populationRecordFileNames;
        speciesAlgorithmSettings = Instance.speciesAlgorithmSettings;
        waterPointRadius = Instance.waterPointRadius;
        waterPointMaxDistance = Instance.waterPointMaxDistance;
        pathfindingAlgorithm = Instance.pathfindingAlgorithm;
        pathfindingTileRadius = Instance.pathfindingTileRadius;
        pathfindingTileHeight = Instance.pathfindingTileHeight;
        showPathfindingTiles = Instance.showPathfindingTiles;
        showPathfindingPaths = Instance.showPathfindingPaths;
        showWaterPoints = Instance.showWaterPoints;
        showSpawnRegions = Instance.showSpawnRegions;
        showSightCones = Instance.showSightCones;
        showHearingCones = Instance.showHearingCones;
        showSoundEvents = Instance.showSoundEvents;
    }

    public void LoadFromRealtimeConfig() {
        //Populations
        for(int i = 0; i < _SPECIESCOUNT; i++) {
            populationCounts[i] = int.Parse(populationCountTMPs[i].text);
            populationRecordFolders[i] = populationRecordFoldersTMPs[i].text;
            populationRecordFileNames[i] = populationRecordFileNamesTMPs[i].text;
        }

        //Genetic Algorithms
        for(int i = 0; i < _SPECIESCOUNT + 1;  i++) {
            Distribution distribution = statistics.Distribution.Uniform;
            switch(distributionTypeTMPDrops[i].value) {
                case 0:
                   distribution = statistics.Distribution.Uniform;
                    break;
                case 1:
                    distribution = statistics.Distribution.Gaussian;
                    break;
            }

            speciesAlgorithmSettings[i] = new EcoTrainer.SpeciesAlgorithmSettings {
                speciesName = "",
                distributionType = distribution,
                minWeight = float.Parse(minWeightTMPs[i].text),
                maxWeight = float.Parse(maxWeightTMPs[i].text),
                meanWeight = 0.0f,
                stdDevWeight = 0.0f,

                minBias = float.Parse(minBiasTMPs[i].text),
                maxBias = float.Parse(maxBiasTMPs[i].text),
                meanBias = 0.0f,
                stdDevBias = 0.0f,

                minWeightMutation = float.Parse(minWeightMutationTMPs[i].text),
                maxWeightMutation = float.Parse(maxWeightMutationTMPs[i].text),
                meanWeightMutation = 0.0f,
                stdDevWeightMutation = 0.0f,

                minBiasMutation = float.Parse(minBiasMutationTMPs[i].text),
                maxBiasMutation = float.Parse(maxBiasMutationTMPs[i].text),
                meanBiasMutation = 0.0f,
                stdDevBiasMuation = 0.0f
            };
        }

        //Ecosystem
        waterPointRadius = float.Parse(waterPointRadiusTMP.text);
        waterPointMaxDistance = float.Parse(waterPointMaxDistanceTMP.text);

        //Pathfinding Algorithm
        PathfindingAlgorithm algorithm = PathfindingAlgorithm.AStar;
        switch(pathfindingAlgorithmTMPDrop.value) {
            case 0:
                algorithm = PathfindingAlgorithm.Dijkstra; 
                break;
            case 1:
                algorithm = PathfindingAlgorithm.Greedy;
                break;
            case 2:
                algorithm = PathfindingAlgorithm.AStar;
                break;
            case 3:
                algorithm = PathfindingAlgorithm.DStar;
                break;
            case 4:
                algorithm = PathfindingAlgorithm.DStarFocused;
                break;
            case 5:
                algorithm = PathfindingAlgorithm.DStarLite;
                break;
        }
        pathfindingAlgorithm = algorithm;
        pathfindingTileRadius = float.Parse(pathfindingTileRadiusTMP.text);
        pathfindingTileHeight = float.Parse(pathfindingTileHeightTMP.text);
    }
}
