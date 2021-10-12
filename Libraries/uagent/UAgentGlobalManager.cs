//==============================================================================
// Filename: UAgentGlobalManager.cs
// Author: Aaron Thompson
// Date Created: 8/16/2020
// Last Updated: 8/16/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
public class UAgentGlobalManager : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	public List<GameObject> agents;

	[Range(0.001f, 2.0f)]
	public float updateDeltaTime = 0.25f;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Awake() {
		agents = new List<GameObject>();
		StartCoroutine(IEUpdateList());
	}

	void Start () {

	}
	
	void Update () {
		
	}

// MANAGE
//------------------------------------------------------------------------------
	private void UpdateList() {
		UAgentCore[] temp = FindObjectsOfType<UAgentCore>();

		agents.Clear();
		for(int i = 0; i < temp.Length; i++) {
			agents.Add(temp[i].gameObject);
		}
	}

	IEnumerator IEUpdateList() {
		while(true) {
			UpdateList();
			yield return new WaitForSeconds(updateDeltaTime);
		}
	}
}
//==============================================================================
//==============================================================================
