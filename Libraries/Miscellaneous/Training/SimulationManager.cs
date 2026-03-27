//==============================================================================
// Filename: SimulationManager.cs
// Author: Aaron Thompson
// Date Created: 7/1/2022
// Last Updated: 3/24/2026
//
// Description: Designed to handle management of running simulations of agents.
// Helps with glueing together configuarion settings, ecosystems settings, agents,
// and more to provide experimental controls and data.
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
public class SimulationManager : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    public bool running;
    public float currentSessionTime;
    public int currentSession;

    //Components
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

// MONOBEHAVIOR FUNCTIONS
//------------------------------------------------------------------------------
    void Start() {
        
    }

    void Update() {
    }
}
//==============================================================================
//==============================================================================