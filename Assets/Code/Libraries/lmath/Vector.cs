//==============================================================================
// Filename: Vector.cs
// Author: Aaron Thompson
// Date Created: 6/11/2020
// Last Updated: 6/11/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lmath {
public class Vector<T> : LArray<T> where T : System.IConvertible {
// VARIABLES
//------------------------------------------------------------------------------
	public int length { get { return data.Length; } }

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public Vector() {
		data = new T[0];
		shape = new int[1] { 0 };
	}
	
	public Vector(T[] data) {
		Reshape(data.Length);
		SetData(data);
	}

	public Vector(Vector<T> vector) {
		data = new T[vector.length];
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
	public static Vector<T> Zeros(int n) {
		Vector<T> vector = new Vector<T>();
		
		vector.Reshape(n);

		return vector;
	}

	public static Vector<T> Ones(int n) {
		Vector<T> vector = Zeros(n);
		T one = (T)System.Convert.ChangeType(1, typeof(T));
		
		vector.Fill(one);

		return vector;
	}

// OPERATIONS
//------------------------------------------------------------------------------
	//ADDITION
	public static Vector<T> Add(Vector<T> v1, Vector<T> v2) {
		Vector<T> v3 = new Vector<T>(v1);
		v3.Add(v2);
		return v3;
	}

	public static Vector<T> operator+(Vector<T> v1, Vector<T> v2) {
		return Add(v1, v2);
	}
	
	//SCALAR MULTIPLICATION
	public static Vector<T> Scale(Vector<T> v1, T c) {
		Vector<T> v2 = new Vector<T>(v1);
		v2.Scale(c);
		return v2;
	}

	public static Vector<T> operator*(Vector<T> v1, T c) {
		return Scale(v1, c);
	}

	public static Vector<T> operator*(T c, Vector<T> v1) {
		return Scale(v1, c);
	}
	
	//SUBTRACT
	public static Vector<T> Negate(Vector<T> v1) {
		Vector<T> v2 = new Vector<T>(v1);
		v2.Negate();
		return v2;
	}

	public static Vector<T> operator-(Vector<T> v1) {
		return Negate(v1);
	}

	public static Vector<T> Subtract(Vector<T> v1, Vector<T> v2) {
		Vector<T> v3 = new Vector<T>(v1);
		v3.Subtract(v2);
		return v3;
	}

	public static Vector<T> operator-(Vector<T> v1, Vector<T> v2) {
		return Subtract(v1, v2);
	}

	//RANDOM
	public static Vector<T> Random(Vector<T> min, Vector<T> max) {
		Vector<T> vector = new Vector<T>(min);
		vector.Randomize(min, max);
		return vector;
	}

	//DOT PRODUCT
	//TODO : ERROR if length != vector.length
	public T Dot(Vector<T> vector) {
		double sum = 0;

		for(int i = 0; i < length; i++) {
			double a = System.Convert.ToDouble(data[i]);
			double b = System.Convert.ToDouble(vector.GetElement(i));
			sum += (a * b);
		}

		return (T)System.Convert.ChangeType(sum, typeof(T));
	}

	public static T Dot(Vector<T> v1, Vector<T> v2) {
		return v1.Dot(v2);
	}

	public static T operator*(Vector<T> v1, Vector<T> v2) {
		return Dot(v1, v2);
	}

	//NORM
	//TODO : ERROR if all elements are 0
	public T Norm(int n = 2) {
		double result = 0;

		for(int i = 0; i < length; i++) {
			double e = System.Convert.ToDouble(data[i]);
			result += System.Math.Pow(System.Math.Abs(e), n);
		}
		result = System.Math.Pow(result, 1.0/n);

		return (T)System.Convert.ChangeType(result, typeof(T));
	}

	public T EuclidNorm() {
		return Norm();
	}

	public T MaxNorm() {
		double result = 0;

		for(int i = 0; i < length; i++) {
			double e = System.Convert.ToDouble(data[i]);
			result = System.Math.Max(result, System.Math.Abs(e));
		}

		return (T)System.Convert.ChangeType(result, typeof(T));
	}

	//UNIT
	//TODO : ERROR if all elements are 0
	public Vector<T> Unit() {
		Vector<T> unit = Zeros(length);
		double norm = System.Convert.ToDouble(Norm(2));

		for(int i = 0; i < length; i++) {
			double e = System.Convert.ToDouble(data[i]);
			unit[i] = (T)System.Convert.ChangeType(e/norm, typeof(T));
		}

		return unit;
	}

// CONDITIONS
//------------------------------------------------------------------------------
	//UNIT
	public bool IsUnit() {
		double norm = System.Convert.ToDouble(Norm(2));

		return System.Math.Abs(1.0 - norm) < epsilon;
	}

	public static bool IsUnit(Vector<T> v1) {
		return v1.IsUnit();
	}

	//ORTHOGONAL
	public bool IsOrthogonal(Vector<T> vector) {
		double dot = System.Convert.ToDouble(Dot(vector));

		return dot < epsilon;
	}

	public static bool IsOrthogonal(Vector<T> v1, Vector<T> v2) {
		return v1.IsOrthogonal(v2);
	}

	//ORTHONORMAL
	public bool IsOrthonormal(Vector<T> vector) {
		return IsOrthogonal(vector) && IsUnit() && vector.IsUnit();
	}

	public static bool IsOrthonormal(Vector<T> v1, Vector<T> v2) {
		return v1.IsOrthonormal(v2);
	}
}
} //END namespace lmath
//==============================================================================
//==============================================================================