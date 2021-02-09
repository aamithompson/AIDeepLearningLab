//==============================================================================
// Filename: Calculus.cs
// Author: Aaron Thompson
// Date Created: 12/9/2020
// Last Updated: 2/9/2021
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

// PERCEPTRON/NEURON/UNIT
//------------------------------------------------------------------------------
	public class Neuron {
			private System.Func<Vector<float>, float> f;

			public Neuron(System.Func<Vector<float>, float> f) {
				this.f = f;
			}

			public float Activate(Vector<float> v) {
				return f(v);
			}
	}
}
}
//==============================================================================
//==============================================================================