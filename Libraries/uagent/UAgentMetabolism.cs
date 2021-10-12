//==============================================================================
// Filename: UAgentMetabolism.cs
// Author: Aaron Thompson
// Date Created: 10/7/2021
// Last Updated: 10/7/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
[RequireComponent(typeof(UAgentCore))]
public class UAgentMetabolism : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	//AGENT COMPONENT(s)
	public UAgentCore uAgentCore;

	//METABOLISM SETTING(s)
	public float nutrition;
	public float maxNutrition;
	public float hungerPerSecond;

	public float hydration;
	public float maxHydration;
	public float thirstPerSecond;

	public float rest;
	public float maxRest;
	public float tirePerSecond;

	//INTERVAL SETTING(s)
	[Range(0.001f, 2.0f)]
	public float deltaTime = 0.0833f;

	//DEBUG

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Awake() {
		uAgentCore = GetComponent<UAgentCore>();
    }

	// Use this for initialization
	void Start () {
		nutrition = maxNutrition;
		hydration = maxHydration;
		rest = maxRest;
		StartCoroutine(IEDrain());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator IEDrain() {
		yield return new WaitForSeconds(deltaTime);

		while (true) {
			nutrition -= hungerPerSecond * deltaTime;
			nutrition = Mathf.Max(nutrition, 0);
			hydration -= thirstPerSecond * deltaTime;
			hydration = Mathf.Max(hydration, 0);
			rest -= tirePerSecond * deltaTime;
			rest = Mathf.Max(rest, 0);

			yield return new WaitForSeconds(deltaTime);
        }
    }
}
//==============================================================================
//==============================================================================
