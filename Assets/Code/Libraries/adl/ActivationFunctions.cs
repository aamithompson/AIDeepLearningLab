//==============================================================================
// Filename: ActivationFunctions.cs
// Author: Aaron Thompson
// Date Created: 12/6/2020
// Last Updated: 12/6/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace adl {
public static class ActivationFunctions {
	public static float Identity(float x) {
		return x;
	}

	public static float BinaryStep(float x) {
		if(x < 0) {
			return 0;
		} else {
			return 1;
		}
	}
	
	//Logistic, Sigmoid, or Softstep
	public static float Sigmoid(float x) {
		return 1 / (1 + Mathf.Exp(-x));
	}

	public static float Tanh(float x) {
		return (Mathf.Exp(x) - Mathf.Exp(-x))/ (Mathf.Exp(x) + Mathf.Exp(-x));
	}
	
	//Rectified Linear Unit
	public static float ReLU(float x) {
		if (x <= 0) {
			return 0;
		} else {
			return x;
		}
	}
	
	//Gaussian Error Linear Unit
	public static float GELU(float x) {
		return (1/2) * x * (1 + ncomp.Calculus.Erf(x/Mathf.Sqrt(2)));
	}
	
	//Leaky Rectified Linear Unit
	public static float LRELU(float x) {
		if (x < 0) {
			return 0.01f * x;
		} else {
			return x;
		}
	}
	
	//ElliotSig, or Soft Sign
	public static float ElliotSig(float x) {
		return x / (1 + Mathf.Abs(x));
	}

	public static float Swish(float x) {
		return x/(1 + Mathf.Exp(-x));
	}
	
	//SQNL
	public static float SQNL(float x) {
		if(x > 2.0f) {
			return 1;
		} else if(0.0f < x && x <= 2.0f) {
			return x - ((x * x)/4);
		} else if(-2.0 <= x && x <= 0.0f) {
			return x + ((x * x)/4);
		} else {
			return -1;
		}
	}
}
}
//==============================================================================
//==============================================================================
