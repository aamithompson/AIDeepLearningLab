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
public class Tensor<T> : LArray<T> where T : System.IConvertible {

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public Tensor() {
		data = new T[0];
		shape = new int[0];
	}

	public Tensor(System.Array data) {
		SetData(data);
	}

	public Tensor(T[] data, int[] shape) {
		data = new T[data.Length];
		shape = new int[shape.Length];
		Reshape(shape);
		SetData(data);
	}

	public Tensor(Tensor<T> tensor) {
		data = new T[tensor.GetLength()];
		shape = new int[tensor.rank];
		for(int i = 0; i < rank; i++) {
			shape[i] = 0;
		}

		Copy(tensor);
	}


// DEFAULT OBJECTS
//------------------------------------------------------------------------------
	public static Tensor<T> Zeros(int[] shape) {
		Tensor<T> tensor = new Tensor<T>();

		tensor.Reshape(shape);

		return tensor;
	}

	public static Tensor<T> Ones(int[] shape) {
		Tensor<T> tensor = Zeros(shape);
		T one = (T)System.Convert.ChangeType(1, typeof(T));
		
		tensor.Reshape(shape);
		tensor.Fill(one);

		return tensor;
	}

// OPERATIONS
//------------------------------------------------------------------------------
	//ADDITION
	public static Tensor<T> Add(Tensor<T> A, Tensor<T> B) {
		Tensor<T> C = new Tensor<T>(A);
		C.Add(B);
		return C;
	}

	public static Tensor<T> operator +(Tensor<T> A, Tensor<T> B) {
		return Add(A, B);
	}
	
	//SCALAR MULTIPLICATION
	public static Tensor<T> Scale(T c, Tensor<T> A) {
		Tensor<T> B = new Tensor<T>(A);
		B.Scale(c);
		return B;
	}

	public static Tensor<T> operator *(T c, Tensor<T> A) {
		return Scale(c, A);
	}

	public static Tensor<T> operator *(Tensor<T> A, T c) {
		return Scale(c, A);
	}

	//SUBTRACT
	public static Tensor<T> Negate(Tensor<T> A) {
		Tensor<T> B = new Tensor<T>(A);
		B.Negate();
		return B;
	}

	public static Tensor<T> operator -(Tensor<T> A) {
		return Negate(A);
	}

	public static Tensor<T> Subtract(Tensor<T> A, Tensor<T> B) {
		Tensor<T> C = new Tensor<T>(A);
		C.Subtract(B);
		return C;
	}

	public static Tensor<T> operator -(Tensor<T> A, Tensor<T> B) {
		return Subtract(A, B);
	}

	//RANDOM
	public static Tensor<T> Random(Tensor<T> min, Tensor<T> max) {
		Tensor<T> tensor = new Tensor<T>(min);
		tensor.Randomize(min, max);
		return tensor;
	}
}
} // END namespace lmath
//==============================================================================
//==============================================================================
