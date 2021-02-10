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
			i = network.Count - 1;
		} else {
			network.Insert(i, new List<Neuron>());
		}

		network[i].Add(new Neuron(f));
		AddUnit(i, width-1);

		if(n > 1) {
			AddLayer(f, i, n-1, width);
		}
	}
	
	//Does not allow for the removal of the input OR output layer
	public void RemoveLayer(int i=-1) {
		
	}

	public void AddUnit(int i, int n=1) {
		System.Func<float, float> f = network[i][0].f;
		for(int j = 0; j < n; j++) {
			network[i].Add(new Neuron(f));
		}
	}

	//Will not remove a unit if layer width is less than or equal to 1
	public void RemoveUnit(int i) {

	}

	public void SetLayerWidth(int i, int n) {

	}

	public int GetLayerWidth(int i) {
			return network[i].Count;
	}
	
	//Does not allow for the input layer's function to be changed
	public void SetLayerFunction(int i, System.Func<float, float> f) {

	}

// ACTIVATION
//------------------------------------------------------------------------------
	public Vector<float> Activate(Vector<float> input) {
		Vector<float> output = Vector<float>.Zeros(GetLayerWidth(0));
		
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