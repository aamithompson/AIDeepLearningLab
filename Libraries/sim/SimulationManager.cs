//==============================================================================
// Filename: SimulationManager.cs
// Author: Aaron Thompson
// Date Created: 7/1/2022
// Last Updated: 4/15/2026
//
// Description: Designed to handle management of running simulations of agents.
// Helps with glueing together configuarion settings, ecosystems settings, agents,
// and more to provide experimental controls and data.
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
public class SimulationManager : MonoBehaviour
{
// VARIABLES
//------------------------------------------------------------------------------
    public bool running;
    private bool loadConfig = true;
    public float currentSessionTime;
    public int currentSession;

    //Components
    public GameObject simulationControlsUI;
    public GameObject simulationConfigUI;

    public SimulationConfig simulationConfig;
    public SimulationPlayButton playButton;
    public SimulationPauseButton pauseButton;
    public SimulationStopButton stopButton;

    public EcoTrainer trainer;
    public EcoSpawnManager spawnManager;
    public EnvironmentManager environmentManager;
    public SoundManager soundManager;
    public SpeciesManager speciesManager;
    public Pathfinding pathfinding;
    public PathGrid pathGrid;
    public AgentUICore agentUICore;


    //Settings
    public float sessionTimeLength;
    public int numberOfSessions;

// MONOBEHAVIOR FUNCTION(s)
//------------------------------------------------------------------------------
    void Start() {
        playButton.simulationManager = this;
        pauseButton.simulationManager = this;
        stopButton.simulationManager = this;
    }

    
// TIME FUNCTION(s)
//------------------------------------------------------------------------------
    public void Resume() {
        Time.timeScale = 1.0f;
    }

    public void Pause() {
        Time.timeScale = 0.0f;
    }

    public void New() {
        if(loadConfig) {
            UpdateScene();
        }

        simulationConfigUI.SetActive(false);
        simulationControlsUI.SetActive(true);

        Time.timeScale = 1.0f;
    }

    public void Stop() {
        Time.timeScale = 0.0f;
        simulationConfigUI.SetActive(true);
        simulationControlsUI.SetActive(false);
    }

// UPDATE FUNCTION(s)
//------------------------------------------------------------------------------
    public void UpdateScene() {
        simulationConfig.LoadFromInstance();
        UpdatePathfinding();
        UpdateSpawnManager();
        UpdateEnvironmentManager();
        UpdateEcoTrainer();
    }

    //Agents
    public void UpdateSpawnManager() {
        if(spawnManager != null) {
            //Variables
            int n = System.Math.Min(spawnManager.maxPopulations.Count, simulationConfig.populationCounts.Count);
            for(int i = 0; i < n; i++) {
                spawnManager.maxPopulations[i] = simulationConfig.populationCounts[i];
            }

            //Generation
            spawnManager.SpawnPopulations();
        }
    }


    public void UpdateEcoTrainer() {
        if(trainer != null) {
            //Variables
            int n = System.Math.Min(trainer.algoSettings.Count, simulationConfig.speciesAlgorithmSettings.Count);
            for(int i = 1; i < n; i++) {
                trainer.algoSettings[i] = simulationConfig.speciesAlgorithmSettings[i];
            }

            trainer.distributionType = simulationConfig.speciesAlgorithmSettings[0].distributionType;
            trainer.minWeight = simulationConfig.speciesAlgorithmSettings[0].minWeight;
            trainer.maxWeight = simulationConfig.speciesAlgorithmSettings[0].maxWeight;
            trainer.minBias = simulationConfig.speciesAlgorithmSettings[0].minBias;
            trainer.maxBias = simulationConfig.speciesAlgorithmSettings[0].maxBias;
            trainer.minWeightMutation = simulationConfig.speciesAlgorithmSettings[0].minWeightMutation;
            trainer.maxWeightMutation = simulationConfig.speciesAlgorithmSettings[0].maxWeightMutation;
            trainer.minBiasMutation = simulationConfig.speciesAlgorithmSettings[0].minBiasMutation;
            trainer.maxBiasMutation = simulationConfig.speciesAlgorithmSettings[0].maxBiasMutation;
        }
    }

    //Environment
    public void UpdateEnvironmentManager() {
        if(environmentManager != null) {
            environmentManager.pointRadius = simulationConfig.waterPointRadius;
            environmentManager.maxDistanceBetweenPoints = simulationConfig.waterPointMaxDistance;
            environmentManager.GenerateWaterPoints();
        }
    }

    //Pathfinding
    public void UpdatePathfinding() {
        if(pathfinding != null && pathGrid != null) {
            //Variables
            pathfinding.algorithm = simulationConfig.pathfindingAlgorithm;
            pathGrid.nodeHeight = simulationConfig.pathfindingTileHeight;
            pathGrid.nodeRadius = simulationConfig.pathfindingTileRadius;
            pathGrid.debugDrawGrid = simulationConfig.showPathfindingTiles;
            pathGrid.debugEnabled = simulationConfig.showPathfindingTiles;

            //Generation
            pathGrid.CalculateParameters();
            pathGrid.CreateGrid();
        }
    }
}
//==============================================================================
//==============================================================================