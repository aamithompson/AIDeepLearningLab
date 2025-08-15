//==============================================================================
// Filename: UAgentGlobalManager.cs
// Author: Aaron Thompson
// Date Created: 8/16/2020
// Last Updated: 2/14/2022
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
	public Dictionary<string, List<GameObject>> consumables;

	[Range(0.001f, 2.0f)]
	public float updateDeltaTime = 0.25f;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Awake() {
		agents = new List<GameObject>();
		consumables = new Dictionary<string, List<GameObject>>();
		uagent.Lexicon.Initalize();
		StartCoroutine(IEUpdateList());
	}

	void Start () {

	}
	
	void Update () {
		
	}

// MANAGE
//------------------------------------------------------------------------------
	private void UpdateList() {
		UAgentCore[] aTemp = FindObjectsOfType<UAgentCore>();
		Consumable[] cTemp = FindObjectsOfType<Consumable>();

		agents.Clear();
		for(int i = 0; i < aTemp.Length; i++) {
			agents.Add(aTemp[i].gameObject);
		}

		consumables.Clear();
		for(int i = 0; i < cTemp.Length; i++) {
			if(!consumables.ContainsKey(cTemp[i].id)) {
				consumables.Add(cTemp[i].id, new List<GameObject>());
			}

			consumables[cTemp[i].id].Add(cTemp[i].gameObject);
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
