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
	public static Dictionary<string, int> ctiMap; //category to index map
	public static Dictionary<int, string> itcMap; //index to category map

// FUNCTIONS
//------------------------------------------------------------------------------
	public static void Initalize() {
		cmdMap = new Dictionary<string, Dictionary<string, int[]>>();
		ctiMap = new Dictionary<string, int>();
		itcMap = new Dictionary<int, string>();

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

		ctiMap.Add("Core", 1);
		ctiMap.Add("Sensor", 2);
		ctiMap.Add("Diet", 3);
		ctiMap.Add("Metabolism", 4);
		ctiMap.Add("Movement", 5);
		ctiMap.Add("Combat", 6);
		ctiMap.Add("Data", 7);
		ctiMap.Add("Cognition", 8);

		itcMap.Add(1, "Core");
		itcMap.Add(2, "Sensor");
		itcMap.Add(3, "Diet");
		itcMap.Add(4, "Metabolism");
		itcMap.Add(5, "Movement");
		itcMap.Add(6, "Combat");
		itcMap.Add(7, "Data");
		itcMap.Add(8, "Cognition");

		//Metabolism
		cmdMap["Diet"].Add("Consume", new int[] { 3, 0, 1});

		//Movement
		//(x, y, z) = 3
		cmdMap["Movement"].Add("Move", new int[] { 5, 0, 3});
		cmdMap["Movement"].Add("Turn", new int[] { 5, 1, 1});

		//Combat
		//(x, y, z) = 3
		cmdMap["Combat"].Add("CombatMove", new int[] { 6, 0, 3 });
	}

// ClASSES/STRUCTS
//------------------------------------------------------------------------------
}
} // END namespace uagent
//==============================================================================
//==============================================================================