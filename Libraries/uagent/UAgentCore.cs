//==============================================================================
// Filename: UAgentCore.cs
// Author: Aaron Thompson
// Date Created: 8/10/2020
// Last Updated: 8/16/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
public class UAgentCore : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	// AGENT COMPONENT(s)
	public UAgentGlobalManager uagentManager;
	public UAgentSensor uagentSensor;
	public UAgentData uagentData;
	public UAgentMetabolism uAgentMetabolism;

	public bool hasSensor { get { return uagentSensor != null; } }
	public bool hasData { get { return uagentData != null; } }
	public bool hasMetabolism { get { return uAgentMetabolism != null; } }

	// DEBUG SETTING(s)
	public bool debugEnabled = false;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Awake() {
		UpdateComponents();
	}

	void Start() {
		
	}

	void Update() {
		
	}

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	public void UpdateComponents() {
		uagentManager = FindObjectOfType<UAgentGlobalManager>();

		//https://docs.unity3d.com/ScriptReference/GameObject.GetComponent.html
		//"Returns the component of Type type if the game object has one 
		//attached, null if it doesn't."

		uagentSensor = gameObject.GetComponent<UAgentSensor>();
		uagentData = gameObject.GetComponent<UAgentData>();

		if (hasSensor) { uagentSensor.uagentCore = this; }
		if (hasData) { uagentData.uagentCore = this; }
	}

}
//==============================================================================
//==============================================================================
