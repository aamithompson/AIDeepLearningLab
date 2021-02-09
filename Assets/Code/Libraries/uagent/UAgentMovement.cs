//==============================================================================
// Filename: UAgentMovement.cs
// Author: Aaron Thompson
// Date Created: 8/17/2020
// Last Updated: 8/17/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//------------------------------------------------------------------------------
public class UAgentMovement : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	//AGENT COMPONENT(s)
	public UAgentCore uagentCore;

	//TRANSLATIONAL MOVEMENT
	public Vector3 velocity;
	public Vector3 acceleration;
	public Vector2 localAcceleration;

	//ANGULAR MOVEMENT
	public float angularVelocity;
	public float angularAcceleration;


// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Start () {
		
	}

	void FixedUpdate() {
		
	}
}
//==============================================================================
//==============================================================================
