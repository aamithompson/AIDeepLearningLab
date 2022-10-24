//==============================================================================
// Filename: Vector.cs
// Author: Aaron Thompson
// Date Created: 6/11/2020
// Last Updated: 8/27/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lmath {
public class Vector : LArray {
// VARIABLES
//------------------------------------------------------------------------------
	public int length { get { return data.Length; } }

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public Vector() {
		data = new float[0];
		shape = new int[1] { 0 };
	}
	
	public Vector(float[] data) {
		Reshape(data.Length);
		SetData(data);
	}

	public Vector(Vector vector) {
		data = new float[vector.length];
		shape = new int[1] { vector.length };

		Copy(vector);
	}

// DATA MANAGEMENT
//------------------------------------------------------------------------------
	public void Reshape(int n) {
		Reshape(new int[] { n });
	}

// DEFAULT OBJECTS
//------------------------------------------------------------------------------
	public static Vector Zeros(int n) {
		Vector vector = new Vector();
		
		vector.Reshape(n);

		return vector;
	}

	public static Vector Ones(int n) {
		Vector vector = Zeros(n);
		
		vector.Fill(1.0f);

		return vector;
	}

// OPERATIONS
//------------------------------------------------------------------------------
	//ADDITION
	public static Vector Add(Vector v1, Vector v2) {
		Vector v3 = new Vector(v1);
		v3.Add(v2);
		return v3;
	}

	public static Vector operator+(Vector v1, Vector v2) {
		return Add(v1, v2);
	}
	
	//SCALAR MULTIPLICATION
	public static Vector Scale(Vector v1, float c) {
		Vector v2 = new Vector(v1);
		v2.Scale(c);
		return v2;
	}

	public static Vector operator*(Vector v1, float c) {
		return Scale(v1, c);
	}

	public static Vector operator*(float c, Vector v1) {
		return Scale(v1, c);
	}
	
	//SUBTRACT
	public static Vector Negate(Vector v1) {
		Vector v2 = new Vector(v1);
		v2.Negate();
		return v2;
	}

	public static Vector operator-(Vector v1) {
		return Negate(v1);
	}

	public static Vector Subtract(Vector v1, Vector v2) {
		Vector v3 = new Vector(v1);
		v3.Subtract(v2);
		return v3;
	}

	public static Vector operator-(Vector v1, Vector v2) {
		return Subtract(v1, v2);
	}

	public static Vector HadamardProduct(Vector v1, Vector v2) {
		Vector v3 = new Vector(v1);
		v3.HadamardProduct(v2);
		return v3;
	}

	//RANDOM
	public static Vector Random(Vector min, Vector max) {
		Vector vector = Vector.Zeros(min.length);
		vector.Randomize(min, max);
		return vector;
	}

	public static Vector Random(float min, float max, int n) {
		Vector vector = Vector.Zeros(n);
		vector.Randomize(min, max);
		return vector;
	}

	public static Vector RandomN(Vector mean, Vector stdDev) {
		Vector vector = Vector.Zeros(mean.length);
		vector.RandomizeN(mean, stdDev);
		return vector;
	}

	public static Vector RandomN(float mean, float stdDev, int n) {
		Vector vector = Vector.Zeros(n);
		vector.RandomizeN(mean, stdDev);
		return vector;
	}

	//DOT PRODUCT
	//TODO : ERROR if length != vector.length
	public float Dot(Vector vector) {
		float sum = 0;

		float[] data = vector.AccessData();
		for(int i = 0; i < length; i++) {
			float a = this.data[i];
			float b = data[i];
			sum += (a * b);
		}

		return sum;
	}

	public static float Dot(Vector v1, Vector v2) {
		return v1.Dot(v2);
	}

	public static float operator*(Vector v1, Vector v2) {
		return Dot(v1, v2);
	}

	//NORM
	//TODO : ERROR if all elements are 0
	public float Norm(int n = 2) {
		double result = 0;

		for(int i = 0; i < length; i++) {
			result += System.Math.Pow(System.Math.Abs(data[i]), n);
		}

		return (float)System.Math.Pow(result, 1.0/n);
	}

	public float EuclidNorm() {
		return Norm();
	}

	public float MaxNorm() {
		float result = 0;

		for(int i = 0; i < length; i++) {
			result = System.Math.Max(result, System.Math.Abs(data[i]));
		}

		return result;
	}

	//UNIT
	//TODO : ERROR if all elements are 0
	public Vector Unit() {
		Vector unit = Zeros(length);
		float norm = Norm(2);

		float[] data = unit.AccessData();
		for(int i = 0; i < length; i++) {
			data[i] = this.data[i]/norm;
		}

		return unit;
	}

// CONDITIONS
//------------------------------------------------------------------------------
	//UNIT
	public bool IsUnit() {
		float norm = Norm(2);

		return System.Math.Abs(1.0 - norm) < epsilon;
	}

	public static bool IsUnit(Vector v1) {
		return v1.IsUnit();
	}

	//ORTHOGONAL
	public bool IsOrthogonal(Vector vector) {
		double dot = System.Convert.ToDouble(Dot(vector));

		return dot < epsilon;
	}

	public static bool IsOrthogonal(Vector v1, Vector v2) {
		return v1.IsOrthogonal(v2);
	}

	//ORTHONORMAL
	public bool IsOrthonormal(Vector vector) {
		return IsOrthogonal(vector) && IsUnit() && vector.IsUnit();
	}

	public static bool IsOrthonormal(Vector v1, Vector v2) {
		return v1.IsOrthonormal(v2);
	}
}
} //END namespace lmath
//==============================================================================
//==============================================================================