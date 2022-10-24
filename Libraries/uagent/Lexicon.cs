//==============================================================================
// Filename: Lexicon.cs
// Author: Aaron Thompson
// Date Created: 5/25/2022
// Last Updated: 5/25/2022
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
namespace uagent {
public static class Lexicon {
// VARIABLES
//------------------------------------------------------------------------------
	//Map
	public static Dictionary<string, Dictionary<string, int[]>> cmdMap;

// FUNCTIONS
//------------------------------------------------------------------------------
	public static void Initalize() {
		cmdMap = new Dictionary<string, Dictionary<string, int[]>>();

		//Categories
		//Component Order
		// 1) Core
		// 2) Sensor
		// 3) Diet
		// 4) Metabolism
		// 5) Movement
		// 6) Combat
		// 7) Data
		// 8) Cognition
		cmdMap.Add("Core", new Dictionary<string, int[]>());
		cmdMap.Add("Sensor", new Dictionary<string, int[]>());
		cmdMap.Add("Diet", new Dictionary<string, int[]>());
		cmdMap.Add("Metabolism", new Dictionary<string, int[]>());
		cmdMap.Add("Movement", new Dictionary<string, int[]>());
		cmdMap.Add("Combat", new Dictionary<string, int[]>());
		cmdMap.Add("Data", new Dictionary<string, int[]>());
		cmdMap.Add("Cognition", new Dictionary<string, int[]>());

		//Metabolism
		cmdMap["Diet"].Add("Consume", new int[] { 3, 1, 1});

		//Movement
		//(x, y, z) = 3
		cmdMap["Movement"].Add("Move", new int[] { 5, 1, 3});

		//Combat
		//(x, y, z) = 3
		cmdMap["Combat"].Add("CombatMove", new int[] { 6, 1, 3 });
	}

// ClASSES/STRUCTS
//------------------------------------------------------------------------------
}
} // END namespace uagent
//==============================================================================
//==============================================================================