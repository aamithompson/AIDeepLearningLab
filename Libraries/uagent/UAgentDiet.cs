//==============================================================================
// Filename: UAgentDiet.cs
// Author: Aaron Thompson
// Date Created: 2/8/2022
// Last Updated: 2/21/2022
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
[RequireComponent(typeof(UAgentCore))]
public class UAgentDiet : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	//AGENT COMPONENT(s)
	public UAgentCore uAgentCore;

	//NON-AGENT VARIABLE(s)
	public List<string> diet;
	public HashSet<string> dietSet;
	public float consumeRadius;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Start() {
		UpdateSets();
	}

// FUNCTIONS
//------------------------------------------------------------------------------
	public GameObject FindNearestConsumable(string id="") {
		GameObject consumable = null;
		float minDistance = -1;
		for(int i = 0; i < diet.Count; i++) {
			if(id != "" && diet[i] != id) {
				continue;
			}

            if(uAgentCore.uAgentManager.consumables.ContainsKey(diet[i])) {
				List<GameObject> cList = uAgentCore.uAgentManager.consumables[diet[i]];
				Debug.Log(cList.Count);
				if (minDistance < 0 && cList.Count > 0) {
					consumable = cList[0];
					minDistance = Vector3.SqrMagnitude(this.transform.position - cList[0].transform.position);
                }

				for(int j = 0; j < cList.Count; j++) {
					float distance = Vector3.SqrMagnitude(this.transform.position - cList[j].transform.position);
					if (distance < minDistance) {
						consumable = cList[j];
						minDistance = distance;
					}
				}
            }
        }

		return consumable;
    }

	public void Consume(Consumable consumable) {
		if((transform.position - consumable.gameObject.transform.position).sqrMagnitude <= consumeRadius * consumeRadius) {
			Nutrition nutrition = consumable.Consume();

			if (uAgentCore.hasMetabolism) {
				uAgentCore.uAgentMetabolism.ApplyNutrution(nutrition);
			}
		}
    }
	
	private void UpdateSets() {
		dietSet = new HashSet<string>(diet);
	}
}
//==============================================================================
//==============================================================================
