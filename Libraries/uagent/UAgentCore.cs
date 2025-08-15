//==============================================================================
// Filename: UAgentCore.cs
// Author: Aaron Thompson
// Date Created: 8/10/2020
// Last Updated: 5/10/2022
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;
//------------------------------------------------------------------------------
public class UAgentCore : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	// AGENT COMPONENT(s)
	public UAgentGlobalManager uAgentManager;
	public UAgentSensor uAgentSensor;
	public UAgentDiet uAgentDiet;
	public UAgentMetabolism uAgentMetabolism;
	public UAgentMovement uAgentMovement;
	public UAgentCombat uAgentCombat;
	public UAgentData uAgentData;
	public UAgentCognition uAgentCognition;

	public bool hasSensor { get { return uAgentSensor != null; } }
	public bool hasDiet { get { return uAgentDiet != null; } }
	public bool hasMetabolism { get { return uAgentMetabolism != null; } }
	public bool hasMovement { get { return uAgentMovement != null; } }
	public bool hasCombat { get { return uAgentCombat != null;} }
	public bool hasData { get { return uAgentData != null; } }
	public bool hasCognition { get { return uAgentCognition != null;} }

	// COMMAND VARIABLE(s)
	private Queue<Command> commands;
	public Command currentCmd;
	public bool initiatingCmd;
	public bool runningCmd;

	// DEBUG SETTING(s)
	public bool debugEnabled = false;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Awake() {
		UpdateComponents(true);
	}

	void Start() {
		commands = new Queue<Command>();
	}

	void Update() {

	}

// FUNCTIONS
//------------------------------------------------------------------------------
	public void UpdateComponents(bool reset=false) {
		//Component Order
		// 1) Core
		// 2) Sensor
		// 3) Diet
		// 4) Metabolism
		// 5) Movement
		// 6) Combat
		// 7) Data
		// 8) Cognition

		uAgentManager = FindObjectOfType<UAgentGlobalManager>();

		//https://docs.unity3d.com/ScriptReference/GameObject.GetComponent.html
		//"Returns the component of Type type if the game object has one 
		//attached, null if it doesn't."

		uAgentSensor = gameObject.GetComponent<UAgentSensor>();
		uAgentDiet = gameObject.GetComponent<UAgentDiet>();
		uAgentMetabolism = gameObject.GetComponent<UAgentMetabolism>();
		uAgentMovement = gameObject.GetComponent<UAgentMovement>();
		uAgentCombat = gameObject.GetComponent<UAgentCombat>();
		uAgentData = gameObject.GetComponent<UAgentData>();
		uAgentCognition = gameObject.GetComponent<UAgentCognition>();

		if (hasSensor) { uAgentSensor.uAgentCore = this; }
		if (hasDiet) { uAgentDiet.uAgentCore = this; }
		if (hasMetabolism) { uAgentMetabolism.uAgentCore = this; }
		if (hasMovement) { uAgentMovement.uAgentCore = this; }
		if (hasCombat) { uAgentCombat.uAgentCore = this; }
		if (hasData) { uAgentData.uAgentCore = this; }
		if (hasCognition) { uAgentCognition.uAgentCore = this; }
	}

	public void Override(Vector v, string category, string function, int index=0) {
		currentCmd = new Command(category, function, index, v);
		initiatingCmd = true;
		runningCmd = true;
	}

	public void Next() {
		currentCmd = Dequeue();
		initiatingCmd = true;
		runningCmd = true;
	}
	
	public void Enqueue(Vector v, string category, string function, int index=0) {
		Command command = new Command(category, function, index, v);
		commands.Enqueue(command);
    }

	public Command Dequeue() {
		return commands.Dequeue();
    }

	public Command Peek() {
		return commands.Peek();
    }

// HELPER CLASSES/STRUCTS
//------------------------------------------------------------------------------
	public struct Command{
		public string category;
		public string function;
		public int index;
		public Vector v;

        public Command(string category, string function, int index, Vector v) {
			this.category = category;
			this.function = function;
			this.index = index;
			this.v = new Vector(v);
        }
	}
}
//==============================================================================
//==============================================================================
