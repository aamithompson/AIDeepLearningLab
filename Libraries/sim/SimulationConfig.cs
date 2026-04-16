//==============================================================================
// Filename: SimulationConfig.cs
// Author: Aaron Thompson
// Date Created: 3/24/2026
// Last Updated: 4/2/2026
//
// Description: Settings for the simulation.
//==============================================================================
using adl.genetics;
using Newtonsoft.Json;
using NUnit.Framework;
using statistics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

//------------------------------------------------------------------------------
public class SimulationConfig : MonoBehaviour {
// VARIABLES (BASE)
//------------------------------------------------------------------------------
    private const int _SPECIESCOUNT = 4;
    public static SimulationConfig Instance { get; private set; }

    //General
    public string configFolderPath;
    public string configFileName;
    public uint seed;

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
    //General
    public TMP_InputField configFolderNameTMP;
    public TMP_InputField configFileNameTMP;
    public TMP_InputField seedTMP;

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
        if (Instance != null) {
            LoadFromInstance();
        } else {
            Initialize();
        }

        Instance = this;
    }

// DATA MANIPULATION FUNCTION(s)
//------------------------------------------------------------------------------
    public void Initialize() {
        seed = (uint)(Random.value * uint.MaxValue);
        seedTMP.text = $"{seed}";
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

    public void UpdateUI() {
        //General
        configFolderNameTMP.text = configFolderPath;
        configFileNameTMP.text = configFileName;
        seedTMP.text = seed.ToString();

        //Population
        for(int i = 0; i < _SPECIESCOUNT; i++) {
            populationCountTMPs[i].text = populationCounts[i].ToString();
            populationRecordFoldersTMPs[i].text = populationRecordFolders[i];
            populationRecordFileNamesTMPs[i].text = populationRecordFileNames[i];
        }

        //Genetic Algorithm
        for(int i = 0; i < _SPECIESCOUNT + 1; i++) {
            EcoTrainer.SpeciesAlgorithmSettings settings = speciesAlgorithmSettings[i];
            int distribution = (int) settings.distributionType;

            distributionTypeTMPDrops[i].value = distribution;
            minWeightTMPs[i].text = settings.minWeight.ToString();
            maxWeightTMPs[i].text = settings.maxWeight.ToString();
            minBiasTMPs[i].text = settings.minBias.ToString();
            maxBiasTMPs[i].text = settings.maxBias.ToString();
            minWeightMutationTMPs[i].text = settings.minWeightMutation.ToString();
            maxWeightMutationTMPs[i].text = settings.maxWeightMutation.ToString();
            minBiasMutationTMPs[i].text = settings.minBiasMutation.ToString();
            maxBiasMutationTMPs[i].text = settings.maxBiasMutation.ToString();
        }

        //Ecosystem
        waterPointRadiusTMP.text = waterPointRadius.ToString();
        waterPointMaxDistanceTMP.text = waterPointMaxDistance.ToString();

        //Pathfinding
        int algorithm = (int) pathfindingAlgorithm;
        pathfindingAlgorithmTMPDrop.value = algorithm;
        pathfindingTileRadiusTMP.text = pathfindingTileRadius.ToString();
        pathfindingTileHeightTMP.text = pathfindingTileHeight.ToString();

        //Debug
        showPathfindingTilesToggle.isOn = showPathfindingTiles;
        showPathfindingPathsToggle.isOn = showPathfindingPaths;
        showWaterPointsToggle.isOn = showWaterPoints;
        showSpawnRegionsToggle.isOn = showSpawnRegions;
        showSightConesToggle.isOn = showSightCones;
        showHearingConesToggle.isOn = showHearingCones;
        showSoundEventsToggle.isOn = showSoundEvents;
    }
    
    public void LoadFromInstance() {
        configFolderPath = Instance.configFolderPath;
        configFileName = Instance.configFileName;
        seed = Instance.seed;
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

        UpdateUI();
    }

    public void LoadFromRealtimeConfig() {
        configFolderPath = configFolderNameTMP.text;
        configFileName = configFileNameTMP.text;

        //General
        seed = uint.Parse(seedTMP.text);

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

        //Debug
        showPathfindingTiles = showPathfindingTilesToggle.isOn;
        showPathfindingPaths = showPathfindingPathsToggle.isOn;
        showWaterPoints = showWaterPointsToggle.isOn;
        showSpawnRegions = showSpawnRegionsToggle.isOn;
        showSightCones = showSightConesToggle.isOn;
        showHearingCones = showHearingConesToggle.isOn;
        showSoundEvents = showSoundEventsToggle.isOn;

    }

    void LoadFromFile(string path) {
        string json = System.IO.File.ReadAllText(path);
        ConfigData data = JsonConvert.DeserializeObject<ConfigData>(json);

        seed = data.seed;
        populationCounts = data.PopulationCounts;
        populationRecordFolders = data.PopulationRecordFolders;
        populationRecordFileNames = data.PopulationRecordFileNames;
        speciesAlgorithmSettings = data.SpeciesAlgorithmSettings;
        waterPointRadius = data.WaterPointRadius;
        waterPointMaxDistance = data.WaterPointMaxDistance;
        pathfindingAlgorithm = data.PathfindingAlgorithm;
        pathfindingTileRadius = data.PathfindingTileRadius;
        pathfindingTileHeight = data.PathfindingTileHeight;
        showPathfindingTiles = data.ShowPathfindingTiles;
        showPathfindingPaths = data.ShowPathfindingPaths;
        showWaterPoints = data.ShowWaterPoints;
        showSpawnRegions = data.ShowSpawnRegions;
        showSightCones = data.ShowSightCones;
        showHearingCones = data.ShowHearingCones;
        showSoundEvents = data.ShowSoundEvents;

        UpdateUI();
    }

    void SaveToFile(string path) {
        var obj = new {
            seed = Instance.seed,
            populationCounts = Instance.populationCounts,
            populationRecordFolders = Instance.populationRecordFolders,
            populationRecordFileNames = Instance.populationRecordFileNames,
            speciesAlgorithmSettings = Instance.speciesAlgorithmSettings,
            waterPointRadius = Instance.waterPointRadius,
            waterPointMaxDistance = Instance.waterPointMaxDistance,
            pathfindingAlgorithm = Instance.pathfindingAlgorithm,
            pathfindingTileRadius = Instance.pathfindingTileRadius,
            pathfindingTileHeight = Instance.pathfindingTileHeight,
            showPathfindingTiles = Instance.showPathfindingTiles,
            showPathfindingPaths = Instance.showPathfindingPaths,
            showWaterPoints = Instance.showWaterPoints,
            showSpawnRegions = Instance.showSpawnRegions,
            showSightCones = Instance.showSightCones,
            showHearingCones = Instance.showHearingCones,
            showSoundEvents = Instance.showSoundEvents
        };

        string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        System.IO.File.WriteAllText(path, json);
    }

    private class ConfigData {
        public uint seed { get; set; }
        public List<int> PopulationCounts { get; set; }
        public List<string> PopulationRecordFolders { get; set; }
        public List<string> PopulationRecordFileNames { get; set; }

        public List<EcoTrainer.SpeciesAlgorithmSettings> SpeciesAlgorithmSettings { get; set; }
        public float WaterPointRadius { get; set; }
        public float WaterPointMaxDistance { get; set; }

        public PathfindingAlgorithm PathfindingAlgorithm { get; set; }
        public float PathfindingTileRadius { get; set; }
        public float PathfindingTileHeight { get; set; }

        public bool ShowPathfindingTiles { get; set; }
        public bool ShowPathfindingPaths { get; set; }
        public bool ShowWaterPoints { get; set; }
        public bool ShowSpawnRegions { get; set; }
        public bool ShowSightCones { get; set; }
        public bool ShowHearingCones { get; set; }
        public bool ShowSoundEvents { get; set; }
    }

    /*[StructLayout(LayoutKind.Explicit)]
    public struct RandomStateWrapper {
        [FieldOffset(0)] public Random.State state;
        [FieldOffset(0)] public uint v0;
        [FieldOffset(4)] public uint v1;
        [FieldOffset(8)] public uint v2;
        [FieldOffset(12)] public uint v3;
        public static implicit operator RandomStateWrapper(Random.State aState) {
            return new RandomStateWrapper { state = aState };
        }
        public static implicit operator Random.State(RandomStateWrapper aState) {
            return aState.state;
        }
    }*/
}
