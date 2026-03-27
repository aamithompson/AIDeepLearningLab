//==============================================================================
// Filename: EcoTrainer.cs
// Author: Aaron Thompson
// Date Created: 7/1/2022
// Last Updated: 8/1/2022
//
// Description:
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
    public EcoTrainer trainer;

    //Settings
    public float sessionTimeLength;
    public int numberOfSessions;

// MONOBEHAVIOR FUNCTIONS
//------------------------------------------------------------------------------
    void Start() {
        
    }

    void Update() {
        if(running) {
            currentSessionTime += Time.deltaTime;

            if(currentSessionTime >= sessionTimeLength) {
                currentSessionTime = 0;
                currentSession++;

                if (currentSession >= numberOfSessions) {
                    running = false;
                }
            }
        } 
    }
}
//==============================================================================
//==============================================================================