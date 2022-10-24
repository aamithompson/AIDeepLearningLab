//==============================================================================
// Filename: UAgentMetabolism.cs
// Author: Aaron Thompson
// Date Created: 10/7/2021
// Last Updated: 2/21/2022
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
	public float satiation;
	public float maxSatiation;
	public float hungerPerSecond;

	public float hydration;
	public float maxHydration;
	public float thirstPerSecond;

	public float rest;
	public float maxRest;
	public float tirePerSecond;

	public float stamina;
	public float maxStamina;
	public float staminaPerSecond;

	//INTERVAL SETTING(s)
	[Range(0.001f, 2.0f)]
	public float deltaTime = 0.0833f;

	//DEBUG

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	// Use this for initialization
	void Start () {
		satiation = maxSatiation;
		hydration = maxHydration;
		rest = maxRest;
		stamina = maxStamina;
		StartCoroutine(IEDrain());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

// FUNCTIONS
//------------------------------------------------------------------------------
	IEnumerator IEDrain() {
		yield return new WaitForSeconds(deltaTime);

		while (true) {
			satiation -= hungerPerSecond * deltaTime;
			satiation = Mathf.Max(satiation, 0);
			hydration -= thirstPerSecond * deltaTime;
			hydration = Mathf.Max(hydration, 0);
			rest -= tirePerSecond * deltaTime;
			rest = Mathf.Max(rest, 0);
			stamina += staminaPerSecond * deltaTime;
			stamina = Mathf.Min(stamina, maxStamina);

			yield return new WaitForSeconds(deltaTime);
        }
    }

	public void ModifyStats(float satiation = 0, float hydration=0, float rest=0, float stamina=0) {
		this.satiation += satiation;
		this.satiation = Mathf.Clamp(this.satiation, 0, maxSatiation);
		this.hydration += hydration;
		this.hydration = Mathf.Clamp(this.hydration, 0, maxHydration);
		this.rest += rest;
		this.rest = Mathf.Clamp(this.rest, 0, rest);
		this.stamina += stamina;
		this.stamina = Mathf.Clamp(this.stamina, 0, stamina);
	}

	public void ApplyNutrution(Nutrition nutrition) {
		ModifyStats(nutrition.satiation, nutrition.hydration, 0, 0);
    }
}
//==============================================================================
//==============================================================================
