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
[RequireComponent(typeof(UAgentCore))]
public class UAgentMovement : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	//AGENT COMPONENT(s)
	public UAgentCore uagentCore;

	//SETTING(s)
	public float[] maxHeightDelta;

	//TRANSLATIONAL MOVEMENT
	public Vector3 velocity;
	public Vector3 acceleration;
	public Vector2 localAcceleration;

	//ANGULAR MOVEMENT
	public float angularVelocity;
	public float angularAcceleration;

	//LOLOLOLOLOLOLOLOLOLOLOL
	public Transform target;
	public Vector3[] path;


// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Start () {
		
	}

    void Update() {
		PathRequestManager.RequestPath(transform.position, target.position, maxHeightDelta, OnPathFound);
    }

    void FixedUpdate() {
		
	}

    public void OnDrawGizmos() {
        if(path != null && path.Length > 0) {
			Gizmos.color = Color.black;
			Gizmos.DrawWireCube(transform.position, Vector3.one);
			Gizmos.DrawLine(transform.position, path[0]);

			for (int i = 0; i < path.Length; i++) {
				Gizmos.color = Color.black;
				Gizmos.DrawWireCube(path[i], Vector3.one);
				if(i < path.Length - 1) {
					Gizmos.DrawLine(path[i], path[i+1]);
                }
            }
        }
    }

	public void OnPathFound(Vector3[] path, bool success) {
		this.path = path;
    }
}
//==============================================================================
//==============================================================================
