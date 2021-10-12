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
using ncomp;

namespace adl {
public static class ActivationFunctions {
// FUNCTIONS
//------------------------------------------------------------------------------
	//Idenitity
	public static Operation Identity = new Operation(Identity_F, Identity_DF);

	public static float Identity_F(float x) {
		return x;
	}

	public static float Identity_DF(float x) {
		return 1;
    }

	//BinaryStep
	public static Operation BinaryStep = new Operation(BinaryStep_F, BinaryStep_DF);

	public static float BinaryStep_F(float x) {
		if(x < 0) {
			return 0;
		} else {
			return 1;
		}
	}

	public static float BinaryStep_DF(float x) {
		return 0;
	}
	
	//Logistic, Sigmoid, or Softstep
	public static Operation Sigmoid = new Operation(Sigmoid_F, Sigmoid_DF);

	public static float Sigmoid_F(float x) {
		return 1 / (1 + Mathf.Exp(-x));
	}

	public static float Sigmoid_DF(float x) {
		return Sigmoid_F(x) * (1 - Sigmoid_F(x));
    }
	
	//Tanh
	public static Operation Tanh = new Operation(Tanh_F, Tanh_DF);

	public static float Tanh_F(float x) {
		return (Mathf.Exp(x) - Mathf.Exp(-x))/ (Mathf.Exp(x) + Mathf.Exp(-x));
	}

	public static float Tanh_DF(float x) {
		return 1 - Mathf.Pow(Tanh_F(x), 2);
    }
	
	//Rectified Linear Unit
	public static Operation ReLU = new Operation(ReLU_F, ReLU_DF);

	public static float ReLU_F(float x) {
		if (x <= 0) {
			return 0;
		} else {
			return x;
		}
	}

	public static float ReLU_DF(float x) {
		if (x <= 0) {
			return 0;
		} else {
			return 1;
		}
    }
	
	//Gaussian Error Linear Unit
	//https://arxiv.org/abs/1606.08415
	public static Operation GELU = new Operation(GELU_F);

	public static float GELU_F(float x) {
		return (1/2) * x * (1 + ncomp.Calculus.Erf(x/Mathf.Sqrt(2)));
	}
	
	//Leaky Rectified Linear Unit
	public static Operation LReLU = new Operation(LReLU_F, LReLU_DF);

	public static float LReLU_F(float x) {
		if (x < 0) {
			return 0.01f * x;
		} else {
			return x;
		}
	}

	public static float LReLU_DF(float x) {
		if (x < 0) {
			return 0.01f;
		} else {
			return 1;
		}
	}
	
	//ElliotSig, or Soft Sign
	public static Operation ElliotSig = new Operation(ElliotSig_F, ElliotSig_DF);

	public static float ElliotSig_F(float x) {
		return x / (1 + Mathf.Abs(x));
	}

	public static float ElliotSig_DF(float x) {
		return 1 / Mathf.Pow((Mathf.Abs(x) + 1), 2);
	}

	//Swish
	//https://arxiv.org/abs/1710.05941
	public static Operation Swish = new Operation(Swish_F, Swish_DF);

	public static float Swish_F(float x) {
		return x/(1 + Mathf.Exp(-x));
	}

	public static float Swish_DF(float x) {
		return Swish_F(x) + Sigmoid_F(x) * (1 - Swish_F(x));
    }
	
	//SQNL
	//https://ieeexplore.ieee.org/document/8489043
	public static Operation SQNL = new Operation(SQNL_F, SQNL_DF);

	public static float SQNL_F(float x) {
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

	public static float SQNL_DF(float x) {
		if(0.0f < x && x <= 2.0f) {
			return 1 - (x/2);
		} else if(-2.0 <= x && x <= 0.0f) {
			return 1 + (x/2);
		} else {
			return 0;
		}
    }
}

// OPERATION CLASS
//------------------------------------------------------------------------------
public class Operation {
	public System.Func<float, float> f;
	public System.Func<float, float> df;

	public Operation(System.Func<float, float> f) {
		this.f = f;
		this.df = Derivative;
    }

	public Operation(System.Func<float, float> f, System.Func<float, float> df) {
		this.f = f;
		this.df = df;
    }

	public float Derivative(float x) {
		return Calculus.Derivative(f, x);
    }
}
}// END namespace adl
//==============================================================================
//==============================================================================
