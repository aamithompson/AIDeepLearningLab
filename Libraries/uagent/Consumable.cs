//==============================================================================
// Filename: Consumable.cs
// Author: Aaron Thompson
// Date Created: 2/8/2022
// Last Updated: 2/14/2022
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
public class Consumable : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    public string id;
    public Nutrition totalNutrition;
    public int maxPortions;
    public bool isEdible = true;

    public int portions;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Start() {
        portions = maxPortions;
	}

// FUNCTIONS
//------------------------------------------------------------------------------
    public Nutrition GetNutrition(int portions=1) {
        if(maxPortions > 0) {
            portions = System.Math.Min(portions, this.portions);
        }

        if(portions <= 0) {
            return new Nutrition(0, 0);
        }

        int ratio = portions / maxPortions;
        return new Nutrition(totalNutrition.satiation * ratio,
                             totalNutrition.hydration * ratio);
    }

    public Nutrition Consume(int portions=1) {
        if(!isEdible) {
            return new Nutrition(0, 0);
        }

        portions = System.Math.Min(portions, this.portions);
        Nutrition nutrition = GetNutrition(portions);

        if(maxPortions <= 0) {
            if(portions > 1) {
                this.portions -= portions;
            } else if (portions == 0) {
                Destroy(this.gameObject);
            }
        }

        return nutrition;
    }
}

// HELPER CLASSES/STRUCTS
//------------------------------------------------------------------------------
[System.Serializable]
public struct Nutrition {
    public float satiation;
    public float hydration;

    public Nutrition(float satiation, float hydration) {
        this.satiation = satiation;
        this.hydration = hydration;
    }
}

//==============================================================================
//==============================================================================