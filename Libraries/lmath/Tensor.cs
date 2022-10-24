//==============================================================================
// Filename: Tensor.cs
// Author: Aaron Thompson
// Date Created: 5/20/2020
// Last Updated: 6/3/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
namespace lmath {
public class Tensor : LArray {

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public Tensor() {
		data = new float[0];
		shape = new int[0];
	}

	public Tensor(System.Array data) {
		SetData(data);
	}

	public Tensor(float[] data, int[] shape) {
		this.data = new float[data.Length];
		shape = new int[shape.Length];
		Reshape(shape);
		SetData(data);
	}

	public Tensor(Tensor tensor) {
		data = new float[tensor.GetLength()];
		shape = new int[tensor.rank];
		for(int i = 0; i < rank; i++) {
			shape[i] = 0;
		}

		Copy(tensor);
	}


// DEFAULT OBJECTS
//------------------------------------------------------------------------------
	public static Tensor Zeros(int[] shape) {
		Tensor tensor = new Tensor();

		tensor.Reshape(shape);

		return tensor;
	}

	public static Tensor Ones(int[] shape) {
		Tensor tensor = Zeros(shape);
		
		tensor.Reshape(shape);
		tensor.Fill(1.0f);

		return tensor;
	}

// OPERATIONS
//------------------------------------------------------------------------------
	//ADDITION
	public static Tensor Add(Tensor A, Tensor B) {
		Tensor C = new Tensor(A);
		C.Add(B);
		return C;
	}

	public static Tensor operator +(Tensor A, Tensor B) {
		return Add(A, B);
	}
	
	//SCALAR MULTIPLICATION
	public static Tensor Scale(float c, Tensor A) {
		Tensor B = new Tensor(A);
		B.Scale(c);
		return B;
	}

	public static Tensor operator *(float c, Tensor A) {
		return Scale(c, A);
	}

	public static Tensor operator *(Tensor A, float c) {
		return Scale(c, A);
	}

	//SUBTRACT
	public static Tensor Negate(Tensor A) {
		Tensor B = new Tensor(A);
		B.Negate();
		return B;
	}

	public static Tensor operator -(Tensor A) {
		return Negate(A);
	}

	public static Tensor Subtract(Tensor A, Tensor B) {
		Tensor C = new Tensor(A);
		C.Subtract(B);
		return C;
	}

	public static Tensor operator -(Tensor A, Tensor B) {
		return Subtract(A, B);
	}

	public static Tensor HadamardProduct(Tensor A, Tensor B) {
		Tensor C = new Tensor(A);
		C.HadamardProduct(B);
		return C;
    }

	//RANDOM
	public static Tensor Random(Tensor min, Tensor max) {
		Tensor tensor = Tensor.Zeros(min.shape);
		tensor.Randomize(min, max);
		return tensor;
	}

	public static Tensor Random(float min, float max, int[] shape) {
		Tensor tensor = Tensor.Zeros(shape);
		tensor.Randomize(min, max);
		return tensor;
	}

	public static Tensor RandomN(Tensor mean, Tensor stdDev) {
		Tensor tensor = Tensor.Zeros(mean.shape);
		tensor.RandomizeN(mean, stdDev);
		return tensor;
	}

	public static Tensor RandomN(float mean, float stdDev, int[] shape) {
		Tensor tensor = Tensor.Zeros(shape);
		tensor.RandomizeN(mean, stdDev);
		return tensor;
	}
}
} // END namespace lmath
//==============================================================================
//==============================================================================
