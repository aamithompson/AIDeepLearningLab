//==============================================================================
// Filename: UAgentCognition.cs
// Author: Aaron Thompson
// Date Created: 5/4/2022
// Last Updated: 2/5/2026
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using adl;
using lmath;
using uagent;
//------------------------------------------------------------------------------
[RequireComponent(typeof(UAgentCore))]
public class UAgentCognition : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	//AGENT COMPONENT(s)
	public UAgentCore uAgentCore;

	//CORE NETWORK
	private FFNeuralNetwork network;
	public int inputSize { get; private set; }
	public int outputSize { get; private set; }
	public int hLayerWidth; //h -> hidden
	public int hLayerDepth; //h -> hidden

	//VECTORS
	public Vector inputVector;
	public Vector outputVector;

	//BEHAVIOR
	private Dictionary<int, Dictionary<int, List<Vector>>> behaviorMap;
	public int bestCategory = -1;
	public int bestFunction = -1;
	public int bestIndex = -1;
	[Range(0.001f, 2.0f)]
	public float deltaTime = 0.25f;
	public float minPClamp = -1f;
	public float maxPClamp = 1f;

	//FUNCTIONALITY
	public bool isFunctional { get; private set; }
	public bool canInput { get; private set; }
	public bool canOutput { get; private set; }

	//CATEGORY INFORMATION
	List<string> categories;
	Dictionary<string, List<string>> functions;
	Dictionary<string, List<int>> fcount;
	Dictionary<int, int> ltcMap; //lexicon to category map

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Start() {
		if (network == null) {
			InitializeNetwork();
		}

		StartCoroutine(IECognition());
	}

    void Update() {
		if(uAgentCore.runningCmd) {
			if(uAgentCore.currentCmd.category == "Diet") {
				if(uAgentCore.currentCmd.function == "Consume") {
					if(uAgentCore.initiatingCmd) {
						int t = ((int)uAgentCore.currentCmd.v[0]) % uAgentCore.uAgentDiet.diet.Count;
						GameObject gameObject = uAgentCore.uAgentDiet.FindNearestConsumable(uAgentCore.uAgentDiet.diet[t]);
						if (gameObject != null) {
							uAgentCore.uAgentDiet.Consume(gameObject.GetComponent<Consumable>());
						}

						uAgentCore.initiatingCmd = false;
					}
				}
			}

			if(uAgentCore.currentCmd.category == "Movement") {
				if(uAgentCore.currentCmd.function == "Move") {
					if(uAgentCore.initiatingCmd) {
						Vector3 position = new Vector3(uAgentCore.currentCmd.v[1], uAgentCore.currentCmd.v[2], uAgentCore.currentCmd.v[3]);// + transform.position;
						uAgentCore.uAgentMovement.Move(position, uAgentCore.currentCmd.index);
						uAgentCore.initiatingCmd = false;
					}
				}

				if(uAgentCore.currentCmd.function == "Turn") {
					uAgentCore.uAgentMovement.Turn(uAgentCore.currentCmd.v[1], uAgentCore.currentCmd.index);
					uAgentCore.initiatingCmd = false;
                }
			}

			if (uAgentCore.currentCmd.category == "Combat") {
				if(uAgentCore.currentCmd.function == "CombatMove") {
                    if(uAgentCore.initiatingCmd) {
						Vector3 direction = new Vector3(uAgentCore.currentCmd.v[1], uAgentCore.currentCmd.v[2], uAgentCore.currentCmd.v[3]);
						uAgentCore.uAgentCombat.CombatMove(direction, uAgentCore.currentCmd.index);
						uAgentCore.initiatingCmd = false;
					}
                }
			}
		}
	}

	void FixedUpdate() {
		if (uAgentCore.runningCmd) {
			if(uAgentCore.currentCmd.category == "Diet") {

			}

			if(uAgentCore.currentCmd.category == "Movement") {
				if(uAgentCore.currentCmd.function == "Move") {
					if(!uAgentCore.initiatingCmd) {
						uAgentCore.uAgentMovement.Move(uAgentCore.currentCmd.index);
					}
				}

				if(uAgentCore.currentCmd.function == "Turn") {
                    if(!uAgentCore.initiatingCmd) {
						uAgentCore.uAgentMovement.Turn(uAgentCore.currentCmd.v[1], uAgentCore.currentCmd.index);
					}
				}
			}

			if(uAgentCore.currentCmd.category == "Combat") {
				if(uAgentCore.currentCmd.function == "CombatMove") {
					if(!uAgentCore.initiatingCmd) {
						uAgentCore.uAgentCombat.CombatMove(uAgentCore.currentCmd.index);
					}
				}
			}
		}
	}

// INTIALIZATION/UPDATE FUNCTIONS
//------------------------------------------------------------------------------
    public void InitializeNetwork() {
		UpdateCategories();
		inputSize = CalculateInputSize();
		outputSize = CalculateOutputSize();
		isFunctional = canInput && canOutput;

		if(isFunctional) {
			inputVector = Vector.Zeros(inputSize);
			outputVector = Vector.Zeros(outputSize);
			network = new FFNeuralNetwork(inputSize, outputSize);
			network.AddLayer(ActivationFunctions.Sigmoid, -1, hLayerDepth, hLayerWidth);
		} else {
			inputVector = new Vector();
			outputVector = new Vector();
			network = new FFNeuralNetwork();
		}

		UpdateBehaviorMap(true);
	}
	
	public void UpdateBehaviorMap(bool reboot=false) {
		reboot = behaviorMap == null || reboot;

		//NOTE: 1 + x denotes the first element as the probability and x the input
		//vector for the correlating function.
		if(reboot) {
			UpdateCategories();
			behaviorMap = new Dictionary<int, Dictionary<int, List<Vector>>>();
		}

		int n = 0;
		for(int s = 0; s < categories.Count; s++) {
			if(reboot) {
				behaviorMap.Add(Lexicon.ctiMap[categories[s]], new Dictionary<int, List<Vector>>());
			}

			for (int t = 0; t < functions[categories[s]].Count; t++) {
				//Index code from the lexicon
				int[] v = Lexicon.cmdMap[categories[s]][functions[categories[s]][t]];
                if(reboot) {
					//behaviorMap.Add(v[0], new Dictionary<int, List<Vector>>());
					behaviorMap[v[0]].Add(v[1], new List<Vector>());
				}

				//c is the number of instances of a particular function the agent has
				int c = fcount[categories[s]][t];
				for (int i = 0; i < c; i++) {
                    //1 -> Probability Score, v[2] -> Number of Floating Point Variables
                    //Hence output vector = (probablity score, out_1, out_2, . . ., out_n)
                    if(reboot) { 
						behaviorMap[v[0]][v[1]].Add(Vector.Zeros(1 + v[2]));
					}

					behaviorMap[v[0]][v[1]][i][0] = (Mathf.Clamp(outputVector[n], minPClamp, maxPClamp) - minPClamp) / (maxPClamp - minPClamp);
					for (int j = 1; j <= v[2]; j++) {
						behaviorMap[v[0]][v[1]][i][j] = outputVector[n + j];
					}

					n += 1 + v[2];
				}
			}
		}
	}

	public void UpdateCategories() {
		categories = new List<string>();
		functions = new Dictionary<string, List<string>>();
		fcount = new Dictionary<string, List<int>>();
		ltcMap = new Dictionary<int, int>();
		/*categories.Add("Core");
		functions.Add("Core", new List<string>());
		fcount.Add("Core", new List<int>());
		ltcMap.Add(Lexicon.ctiMap["Core"], categories.Count - 1);*/

		/*if (uAgentCore.hasSensor) { 
			categories.Add("Sensor");
			functions.Add("Sensor", new List<string>());
			fcount.Add("Sensor", new List<int>());
			ltcMap.Add(Lexicon.ctiMap["Sensor"], categories.Count - 1);
		}*/

		if(uAgentCore.hasDiet) { 
			categories.Add("Diet");
			functions.Add("Diet", new List<string>());
			fcount.Add("Diet", new List<int>());
			ltcMap.Add(Lexicon.ctiMap["Diet"], categories.Count - 1);

			functions["Diet"].Add("Consume");
			fcount["Diet"].Add(uAgentCore.uAgentDiet.diet.Count);
		}

		/*if(uAgentCore.hasMetabolism) { 
			categories.Add("Metabolism");
			functions.Add("Metabolism", new List<string>());
			fcount.Add("Metabolism", new List<int>());
			ltcMap.Add(Lexicon.ctiMap["Metabolism"], categories.Count - 1);
		}*/

		if(uAgentCore.hasMovement) {
			categories.Add("Movement");
			functions.Add("Movement", new List<string>());
			fcount.Add("Movement", new List<int>());
			ltcMap.Add(Lexicon.ctiMap["Movement"], categories.Count - 1);

			functions["Movement"].Add("Move");
			fcount["Movement"].Add(uAgentCore.uAgentMovement.movementSettings.Length);

			functions["Movement"].Add("Turn");
			fcount["Movement"].Add(uAgentCore.uAgentMovement.turnSettings.Length);
		}

		if(uAgentCore.hasCombat) { 
			categories.Add("Combat");
			functions.Add("Combat", new List<string>());
			fcount.Add("Combat", new List<int>());
			ltcMap.Add(Lexicon.ctiMap["Combat"], categories.Count - 1);

			functions["Combat"].Add("CombatMove");
			fcount["Combat"].Add(uAgentCore.uAgentCombat.cActions.Count);
		}

		/*if(uAgentCore.hasData) { 
			categories.Add("Data");
			functions.Add("Data", new List<string>());
			fcount.Add("Data", new List<int>());
			ltcMap.Add(Lexicon.ctiMap["Data"], categories.Count - 1);
		}*/
		
		/*if(uAgentCore.hasCognition) { 
			categories.Add("Cognition");
			functions.Add("Cognition", new List<string>());
			fcount.Add("Cognition", new List<int>());
			ltcMap.Add(Lexicon.ctiMap["Cognition"], categories.Count - 1);
		}*/
    }

	public void UpdateInputVector(bool reboot=false) {
		reboot = inputVector == null || reboot;
		if(reboot) {
			inputSize = CalculateInputSize();
			inputVector = Vector.Zeros(inputSize);
		}

		int rows = uAgentCore.uAgentSensor.rows;
		int columns = uAgentCore.uAgentSensor.columns;
		int vsize = UAgentSensor.sightVectorSize;
		int camWidth = uAgentCore.uAgentSensor.camWidth;
		int camHeight = uAgentCore.uAgentSensor.camHeight;
		int camVSize = UAgentSensor.camVectorSize;
		int hearingSectors = uAgentCore.uAgentSensor.hearingSectors;
		int sectorsVSize = UAgentSensor.camVectorSize;
		int index = 0;
		if(uAgentCore.hasSensor) {
			for(int i = 0; i < rows; i++) {
				for(int j = 0; j < columns; j++) {
					for(int k = 0; k < vsize; k++) {
						//index = k + ((j + (i * columns)) * vsize);
						inputVector[index] = uAgentCore.uAgentSensor.sightData.GetElement(new int[] { i, j, k });
						index++;
					}
				}
			}

			for(int i = 0; i < camWidth; i++) {
				for(int j = 0; j < camHeight; j++) {
					int camIndex = j + (i * camHeight);
					for(int k = 0; k < camVSize; k++) {
						inputVector[index] = uAgentCore.uAgentSensor.camData.GetElement(new int[] {camIndex, k});
						index++;
                    }
                }
            }

			Debug.Log(inputSize);
			for(int i = 0; i < hearingSectors; i++) {
				for(int j = 0; j < sectorsVSize; j++) {
                    Debug.Log(index);
                    inputVector[index] = uAgentCore.uAgentSensor.hearingData[i];
					index++;
				}
			}
		}
	}

	public void UpdateOutputVector(bool reboot=false) {
		reboot = outputVector == null || reboot;
        if(reboot) {
			outputSize = CalculateOutputSize();
			outputVector = Vector.Zeros(outputSize);
        }

        if(isFunctional) {
			outputVector.Copy(network.Activate(inputVector));
		}
	}

	public void SetConfiguration(Matrix configuration) {
		if (network == null) {
			InitializeNetwork();
		}

		int index = 0;
		for(int i = 0; i < network.weights.Count; i++) {
			for (int j = 0; j < network.weights[i].GetLength(); j++) {
				network.weights[i][j] = configuration[index];
				index++;
            }
		}

		for(int i = 0; i < network.biases.Count; i++) {
			for(int j = 0; j < network.biases[i].GetLength(); j++) {
				network.biases[i][j] = configuration[index];
				index++;
            }
		}
    }

// NETWORK USAGE
//------------------------------------------------------------------------------
    public void Process() {
		UpdateInputVector();
		UpdateOutputVector();
		UpdateBehaviorMap();
    }

	public void Process(Vector input) {
		inputVector.Copy(input);
		UpdateOutputVector();
		UpdateBehaviorMap();
    }

	public Vector GetBehavior(int category, int function, int index = 0) {
		Vector output = new Vector(behaviorMap[category][function][index]);
		return output;
	}

	public Vector GetBehavior(string category, string function, int index = 0) {
		int[] t = Lexicon.cmdMap[category][function];
		return GetBehavior(t[0], t[1], index);
    }

	public Vector GetBestBehavior() {
		// (-1, -1, - 1) -> Behavior Not Found
		Vector output = new Vector();
		List<int> cKeys = new List<int>(behaviorMap.Keys);

		for(int i = 0; i < cKeys.Count; i++) {
			List<int> fKeys = new List<int>(behaviorMap[cKeys[i]].Keys);
			for(int j = 0; j < fKeys.Count; j++) {
				for(int k = 0; k < behaviorMap[cKeys[i]][fKeys[j]].Count; k++) {
					if(output.GetShape()[0] == 0) {
						bestCategory = cKeys[i];
						bestFunction = fKeys[j];
						bestIndex = k;
						output.Copy(behaviorMap[cKeys[i]][fKeys[j]][k]);
						continue;
                    }

					if(output[0] < behaviorMap[cKeys[i]][fKeys[j]][k][0]) {
						bestCategory = cKeys[i];
						bestFunction = fKeys[j];
						bestIndex = k;
						output.Copy(behaviorMap[cKeys[i]][fKeys[j]][k]);
					}
                }
            }
        }

		return output;
    }

	IEnumerator IECognition() {
        while(true) {
			Process();
			Vector v = GetBestBehavior();

			//Debug.Log(behaviorMap.Count);
			//Debug.Log(categories.Count);
			Debug.Log(ltcMap[bestCategory].ToString() + ", " + bestFunction.ToString() + ", " + bestIndex.ToString());
			//outputVector.Print();
			v.Print();

			if (isFunctional && bestCategory > -1 && bestFunction > -1 && bestIndex > -1) {
				uAgentCore.Override(v, categories[ltcMap[bestCategory]], functions[categories[ltcMap[bestCategory]]][bestFunction], bestIndex);
            }

			yield return new WaitForSeconds(deltaTime);
		}
	}

// HELPER FUNCTIONS
//------------------------------------------------------------------------------
	private int CalculateInputSize() {
		int n = 0;

        if(uAgentCore.hasSensor) {
			int columns = uAgentCore.uAgentSensor.columns;
			int rows = uAgentCore.uAgentSensor.rows;
			int sightVectorSize = UAgentSensor.sightVectorSize;

			n += columns * rows * sightVectorSize;

			int camWidth = uAgentCore.uAgentSensor.camWidth;
			int camHeight = uAgentCore.uAgentSensor.camHeight;
			int camVectorSize = UAgentSensor.camVectorSize;

			n += camWidth * camHeight * camVectorSize;

			int hearingSectors = uAgentCore.uAgentSensor.hearingSectors;
			int hearingVectorSize = UAgentSensor.hearingVectorSize;

			n += hearingSectors * hearingVectorSize;
        }

		if(uAgentCore.hasMetabolism) {
			//Hunger (+1), Hydration (+1), Rest (+1), Stamina (+1)
			n += 4;
        }

		canInput = n > 0;

		return n;
	}

	private int CalculateOutputSize() {
		//NOTE: 1 + x denotes the first element as the probability and x the input
		//vector for the correlating function.
		int n = 0;
		for(int s = 0; s < categories.Count; s++) {
			for(int t = 0; t < functions[categories[s]].Count; t++) {
				int[] v = Lexicon.cmdMap[categories[s]][functions[categories[s]][t]];
				n += (1 + v[2]) * fcount[categories[s]][t];
            }
		}

		canOutput = n > 0;

		return n;
	}
}
//==============================================================================
//==============================================================================