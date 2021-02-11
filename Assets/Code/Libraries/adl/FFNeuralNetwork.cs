//==============================================================================
// Filename: Calculus.cs
// Author: Aaron Thompson
// Date Created: 12/9/2020
// Last Updated: 2/10/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;

namespace adl {
public class FFNeuralNetwork {
// VARIABLES
//------------------------------------------------------------------------------
	public List<List<Neuron>> network;
	public List<Matrix<float>> weights;

	public int depth { get { return network.Count; } }


// CONSTRUCTORS
//------------------------------------------------------------------------------
		public FFNeuralNetwork() {
		network = new List<List<Neuron>>();
		weights = new List<Matrix<float>>();
	
		//Input and output layers default is identity layer with 1 unit each.
		AddLayer(ActivationFunctions.Identity,-1, 2);
	}

// DATA MANAGEMENT
//------------------------------------------------------------------------------
	public void AddLayer(System.Func<float, float> f, int i=-1, int n=1, int width=1) {
		if(i == -1){
			network.Add(new List<Neuron>());

			if(depth <= 2) {
				i = depth - 1;
			} else {
				i = depth - 2;
			}

			if(depth > 1) {
				weights.Add(Matrix<float>.Ones(1,1));
			}
			
		} else if(i > 0 && i < depth - 1) {
			network.Insert(i, new List<Neuron>());
			weights.Insert(i, Matrix<float>.Ones(1, 1));
		} else {
			return;
		}

		network[i].Add(new Neuron(f));
		AddUnit(i, width-1);

		if(n > 1) {
			AddLayer(f, i, n-1, width);
		}
	}
	
	//Does not allow for the removal of the input OR output layer
	public void RemoveLayer(int i=-1) {
		if(i <= 0 || i >= depth - 1) {
			return;
		}

		network.RemoveAt(i);
	}

	public void AddUnit(int i, int n=1) {
		//Network
		System.Func<float, float> f = network[i][0].f;
		for(int j = 0; j < n; j++) {
			network[i].Add(new Neuron(f));
		}

		//Weights
		if(i > 0) {
			int[] shape = weights[i-1].GetShape();
			Matrix<float> newWeights = Matrix<float>.Ones(shape[0], shape[1] + n);
			newWeights.SetSlice(weights[i-1], new int[,] { {0, shape[0]}, {0, shape[1]} });
			weights[i-1].Copy(newWeights);
		}

		if(i < depth - 1) {
			int[] shape = weights[i].GetShape();
			Matrix<float> newWeights = Matrix<float>.Ones(shape[0] + n, shape[1]);
			newWeights.SetSlice(weights[i], new int[,] { {0, shape[0]}, {0, shape[1]} });
			weights[i].Copy(newWeights);
		}
	}

	//Will not remove a unit if layer width is less than or equal to 1
	public void RemoveUnit(int i, int n=1) {
		//Network
		for(int j = 0; j < n; j++) {
			if(network[i].Count <= 1) {
				break;
			}

			network[i].RemoveAt(network[i].Count - 1);
		}

		//Weights
		if(i > 0) {
			int[] shape = weights[i-1].GetShape();
			weights[i-1].Reshape(shape[0], shape[1] + n);
		}

		if(i < depth - 1) {
			int[] shape = weights[i].GetShape();
			weights[i].Reshape(shape[0] + n, shape[1]);
			}
	}

	public void SetLayerWidth(int i, int n) {
		if(n <= 1) {
			return;
		}

		if(network[i].Count > n) {
			while(network[i].Count > n) {
				RemoveUnit(i);
			}
		} else if(network[i].Count < n) {
			while(network[i].Count < n) {
				AddUnit(i);
			}
		}
	}

	public int GetLayerWidth(int i) {
			return network[i].Count;
	}
	
	//Does not allow for the input layer's function to be changed
	public void SetLayerFunction(int i, System.Func<float, float> f) {
		int n = network[i].Count;
		for(int j = 0; j < n; j++) {
			network[i][j].f = f;
		}
	}

// ACTIVATION
//------------------------------------------------------------------------------
	public Vector<float> Activate(Vector<float> input) {
		Vector<float> output = new Vector<float>(input);

		for(int i = 0; i < depth - 1; i++) {
			Matrix<float> v = new Matrix<float>(output.GetData(), output.length, 1);
			Matrix<float> result = Matrix<float>.MatMul(weights[i], v);

			output = new Vector<float>(result.GetData());

			for(int j = 0; j < network[i+1].Count; j++) {
				output[j] = network[i+1][j].Activate(output[j]);
			}
		}
		
		return output;
	}

// PERCEPTRON/NEURON/UNIT
//------------------------------------------------------------------------------
	public class Neuron {
			public System.Func<float, float> f;

			public Neuron(System.Func<float, float> f) {
				this.f = f;
			}

			public float Activate(float v) {
				return f(v);
			}
	}
}
}
//==============================================================================
//==============================================================================