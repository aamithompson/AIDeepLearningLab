﻿//==============================================================================
// Filename: UAgentData.cs
// Author: Aaron Thompson
// Date Created: 3/4/2021
// Last Updated: 3/5/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAgentData : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	//AGENT COMPONENT(s)
	public UAgentCore uagentCore;

	//FORMAT SETTING(s)
	private int vectorSize = 6;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Awake() {
	}

	void Start() {
	}

	void Update() {
	}

// DATA COLLECTION
//------------------------------------------------------------------------------
	public string[] DataToString() {
		int varCount = vectorSize;
		string[] data = new string[varCount];

		if (uagentCore.hasSensor) {
			int rows = uagentCore.uagentSensor.rows;
			int columns = uagentCore.uagentSensor.columns;
			varCount = vectorSize * rows * columns;
			data = new string[varCount + 3];

			data[0] = rows.ToString();
			data[1] = columns.ToString();
			data[2] = vectorSize.ToString();

			for(int i = 0; i < rows; i++) {
				for(int j = 0; j < columns; j++) {
					for(int k = 0; k < vectorSize; k++) {
						int index = (i * columns * vectorSize) + (j * vectorSize) + (k + 3);
						string element = uagentCore.uagentSensor.sightData.GetElement(new int[] { i, j, k }).ToString();
						data[index] = element;
                    }
                }
            }
		}

		return data;
    }

}
//==============================================================================
//==============================================================================