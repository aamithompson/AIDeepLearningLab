//==============================================================================
// Filename: UAgentMovement.cs
// Author: Aaron Thompson
// Date Created: 8/17/2020
// Last Updated: 1/31/2022
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
	public UAgentCore uAgentCore;

	//COMPONENT(s)
	Rigidbody rb;

	//SETTING(s)
	public float[] maxHeightDelta;
	public MovementSettings[] movementSettings;
	public float waypointRadius;

	//TRANSLATIONAL MOVEMENT
	public Vector3 velocity;
	public Vector3 acceleration;

	//ANGULAR MOVEMENT
	public float angularVelocity;
	public float angularAcceleration;

	//PATHFINDING
	public Transform target;
	public Vector3[] path;
	public int pathIndex = 0;


// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
    void Start() {
		rb = gameObject.GetComponent<Rigidbody>();
		rb.freezeRotation = true;

		PathRequestManager.RequestPath(transform.position, target.position, maxHeightDelta, OnPathFound);
	}

	void Update() {
		
	}

	void FixedUpdate() {
		if (path != null) {
			Move(0);
		}
	}

	public void OnDrawGizmos() {
		if (path != null && path.Length > 0) {
			Gizmos.color = Color.black;
			Gizmos.DrawWireCube(transform.position, Vector3.one);
			Gizmos.DrawLine(transform.position, path[pathIndex]);

			for (int i = pathIndex; i < path.Length; i++) {
				Gizmos.color = Color.black;
				Gizmos.DrawWireCube(path[i], Vector3.one);
				if (i < path.Length - 1) {
					Gizmos.DrawLine(path[i], path[i + 1]);
				}
			}
		}
	}

// MOVEMENT
//------------------------------------------------------------------------------
	//Movement functions use Time.fixedDeltaTime for the delta time measurement,
	//so ensure these functions are used in FixedUpdate.
	public void MoveDir(Vector3 direction, int i=0){
		MovementSettings ms = movementSettings[i];
		direction = direction.normalized;

        if(uAgentCore.hasMetabolism) {
            if(uAgentCore.uAgentMetabolism.stamina < ms.staminaPerSecond * Time.fixedDeltaTime) {
				return;
            }
        }

		//Angular Calculations
		if (angularVelocity < ms.minAngularVelocity) {
			angularVelocity = ms.minAngularVelocity;
		} else {
			angularVelocity += ms.angularAcceleration * Time.fixedDeltaTime;
			if (angularVelocity > ms.maxAngularVelocity) {
				angularVelocity = ms.maxAngularVelocity;
			}
		}

		//Linear Calculations
		if (velocity == Vector3.zero) {
			velocity = ms.minVelocity * direction;
		} else if(velocity.magnitude < ms.minVelocity) {
			velocity = ms.minVelocity * transform.forward;
        } else {
			velocity += ms.acceleration * transform.forward * Time.fixedDeltaTime;
			if(velocity.magnitude > ms.maxVelocity) {
				velocity = velocity.normalized * ms.maxVelocity;
			}
        }

		//Transformations
		transform.rotation = FixedRotateTowards(direction, angularVelocity, 'y');
		transform.position += velocity * Time.fixedDeltaTime;
		if(uAgentCore.hasMetabolism) {
			uAgentCore.uAgentMetabolism.stamina -= ms.staminaPerSecond * Time.fixedDeltaTime;
        }
	}

	public void MoveDir(Vector3 direction, string key) {
		for(int i = 0; i < movementSettings.Length; i++) {
			if(movementSettings[i].name == key) {
				MoveDir(direction, i);
				break;
			}
		}
    }

	public void Move(int i=0) {
		MovementSettings ms = movementSettings[i];

        if(uAgentCore.hasMetabolism) {
            if(uAgentCore.uAgentMetabolism.stamina < ms.staminaPerSecond * Time.fixedDeltaTime) {
				return;
            }
        }

		if(path != null) {
			Vector3 direction = (Vector3.Scale(path[pathIndex] - transform.position, Vector3.right + Vector3.forward)).normalized;
			MoveDir(direction);

			if(Vector3.SqrMagnitude(transform.position - path[pathIndex]) <= waypointRadius * waypointRadius) {
				pathIndex++;
				if(pathIndex > path.Length - 1) {
					path = null;
					pathIndex = 0;
					return;
				}
			}
		}
	}

	public void Move(string key) {
		for(int i = 0; i < movementSettings.Length; i++) {
			if(movementSettings[i].name == key) {
				Move(i);
				break;
			}
		}
    }

	public void Move(Vector3 target, int i=0){
		PathRequestManager.RequestPath(transform.position, target, maxHeightDelta, OnPathFound);
		Move(target, i);
	}

	public void Move(Vector3 target, string key) {
		for(int i = 0; i < movementSettings.Length; i++) {
			if(movementSettings[i].name == key) {
				Move(target, i);
				break;
			}
		}
    }

// PATHFINDING
//------------------------------------------------------------------------------
	public void OnPathFound(Vector3[] path, bool success) {
		this.path = (success) ? path : null;
    }

// HELPER FUNCTIONS
//------------------------------------------------------------------------------
	//This function uses Time.fixedDeltaTime for the delta time measurement,
	//so ensure these functions are used in FixedUpdate.
	public Quaternion FixedRotateTowards(Vector3 target, float angularVelocity, char axis = 'y') {
		Vector2 from;
		Vector2 to;

		if (char.ToLower(axis) == 'x') {
			from = new Vector2(transform.forward.y, transform.forward.z);
			to = new Vector2(target.y - transform.position.y, target.z - transform.position.z);
		} else if (char.ToLower(axis) == 'y') {
			from = new Vector2(transform.forward.x, transform.forward.z);
			to = new Vector2(target.x - transform.position.x, target.z - transform.position.z);
		} else if (char.ToLower(axis) == 'z') {
			from = new Vector2(transform.forward.x, transform.forward.y);
			to = new Vector2(target.x - transform.position.x, target.y - transform.position.y);
		} else {
			return new Quaternion();
        }

		float t = Mathf.Clamp01(angularVelocity * Time.fixedDeltaTime/Vector2.Angle(from, to));
		return Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target), t);
    }

	public Quaternion FixedRotateTowards(Vector3 target, float angularVelocity, Vector3 axis){
		if(axis == Vector3.right) {
			return FixedRotateTowards(target, angularVelocity, 'x');
        } else if (axis == Vector3.up) {
			return FixedRotateTowards(target, angularVelocity, 'y');
		} else if (axis == Vector3.forward) {
			return FixedRotateTowards(target, angularVelocity, 'z');
		}

		return new Quaternion();
    }

// HELPER CLASSES/STRUCTS
//------------------------------------------------------------------------------
	[System.Serializable]
	public struct MovementSettings {
		public string name;
		public float minVelocity;
		public float maxVelocity;
		public float acceleration;
		public float minAngularVelocity;
		public float maxAngularVelocity;
		public float angularAcceleration;
		public float staminaPerSecond;
	}
}
//==============================================================================
//==============================================================================
