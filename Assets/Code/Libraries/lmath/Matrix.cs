﻿//==============================================================================
// Filename: Matrix.cs
// Author: Aaron Thompson
// Date Created: 5/31/2020
// Last Updated: 6/11/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lmath {
public class Matrix<T> : LArray<T> where T : System.IConvertible {
// CONSTRUCTORS
//------------------------------------------------------------------------------
	public Matrix() {
		data = new T[0];
		shape = new int[2] { 0, 0 };
	}

	public Matrix(T[] data, int m, int n) {
		this.data = new T[data.Length];
		shape = new int[2] { m, n };
		Reshape(m, n);
		SetData(data);
	}

	public Matrix(T[,] data) {
		SetData(data);
	}
	
	public Matrix(Matrix<T> matrix) {
		data = new T[matrix.GetLength()];
		shape = new int[2] { matrix.GetShape()[0], matrix.GetShape()[1] };
		Copy(matrix);
	}

// DATA MANAGEMENT
//------------------------------------------------------------------------------
	public T GetElement(int i, int j) {
		return GetElement(new int[] { i, j });
	}

	public void SetElement(T e, int i, int j) {
		SetElement(e, new int[] { i, j });
	}

	public T this[int i, int j] {
		get {
			return GetElement(i, j);
		}

		set {
			SetElement(value, i, j);
		}
	}

	public Matrix<T> GetRow(int i) {
		Matrix<T> row = Zeros(1, shape[1]);

		for(int k = 0; k < shape[1]; k++) {
			row[0, k] = GetElement(i, k);
		}
		
		return row;
	}

	public Matrix<T> GetColumn(int j) {
		Matrix<T> column = Zeros(shape[0], 1);
		
		for(int k = 0; k < shape[0]; k++) {
			column[k, 0] = GetElement(k, j);
		}

		return column;
	}

	public void Reshape(int i, int j) {
		Reshape(new int[] { i, j });
	}

// DEFAULT OBJECTS
//------------------------------------------------------------------------------
	public static Matrix<T> Zeros(int m, int n) {
		Matrix<T> matrix = new Matrix<T>();
		
		matrix.Reshape(m, n);

		return matrix;
	}

	public static Matrix<T> Ones(int m, int n) {
		Matrix<T> matrix = Zeros(m, n);
		T one = (T)System.Convert.ChangeType(1, typeof(T));

		matrix.Fill(one);

		return matrix;
	}

	public static Matrix<T> Identity(int n) {
		Matrix<T> matrix = Zeros(n, n);
		T one = (T)System.Convert.ChangeType(1, typeof(T));

		for(int i = 0; i < n; i++) {
			matrix.SetElement(one, i, i);
		}

		return matrix;
	}

	public static Matrix<T> Diag(T e, int m, int n) {
		Matrix<T> matrix = Zeros(m, n);
		int diagonalLength = Mathf.Min(m, n);

		for(int i = 0; i < diagonalLength; i++) {
				matrix.SetElement(e, i, i);
		}

		return matrix;
	}
	
	public static Matrix<T> Diag(T[] data, int m=-1, int n=-1) {
		if(m == -1) {
			m = data.Length;
		}
		if(n == -1) {
			n = data.Length;
		}
		Matrix<T> matrix = Zeros(m, n);

		int max = Mathf.Min(new int[] { m, n, data.Length});
		for(int i = 0; i < max; i++) {
			matrix[i, i] = data[i];
		}
		
		return matrix;
	}

	public static Matrix<T> Diag(Vector<T> vector, int m=-1, int n=-1) {
		return Diag(vector.GetData(), m, n);
	}

// OPERATIONS
//------------------------------------------------------------------------------
	//ADDITION
	public static Matrix<T> Add(Matrix<T> A, Matrix<T> B) {
		Matrix<T> C = new Matrix<T>(A);
		C.Add(B);
		return C;
	}

	public static Matrix<T> operator +(Matrix<T> A, Matrix<T> B) {
		return Add(A, B);
	}

	//SCALAR MULTIPLICATION
	public static Matrix<T> Scale(Matrix<T> A, T c) {
		Matrix<T> B = new Matrix<T>(A);
		B.Scale(c);
		return B;
	}

	public static Matrix<T> operator *(Matrix<T> A, T c) {
		return Scale(A, c);
	}

	public static Matrix<T> operator *(T c, Matrix<T> A) {
		return Scale(A, c);
	}

	//SUBTRACT
	public static Matrix<T> Negate(Matrix<T> A) {
		Matrix<T> B = new Matrix<T>(A);
		B.Negate();
		return B;
	}

	public static Matrix<T> operator -(Matrix<T> A) {
		return Negate(A);
	}

	public static Matrix<T> Subtract(Matrix<T> A, Matrix<T> B) {
		Matrix<T> C = new Matrix<T>(A);
		C.Subtract(B);
		return C;
	}

	public static Matrix<T> operator -(Matrix<T> A, Matrix<T> B) {
		return Subtract(A, B);
	}

	//RANDOM
	public static Matrix<T> Random(Matrix<T> min, Matrix<T> max) {
		Matrix<T> matrix = new Matrix<T>(min);
		matrix.Randomize(min, max);
		return matrix;
	}

	//MATRIX MULTIPLICATION
	//TODO : ERROR if A.shape[1] != B.shape[0]
	public static Matrix<T> MatMul(Matrix<T> A, Matrix<T> B) {
		// (m x n) X (n x p) -> m x p
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		int p = B.GetShape()[1];
		Matrix<T> C = Zeros(m, p);
		
		for(int i = 0; i < m; i++) {
			for(int j = 0; j < p; j++) {
				double sum = 0;
				for(int k = 0; k < n; k++){
					double a = System.Convert.ToDouble(A[i, k]);
					double b = System.Convert.ToDouble(B[k, j]);
					sum += a * b;
				}
				C[i, j] = (T)(System.Convert.ChangeType(sum , typeof(T)));
				}
		}

		return C;
	}
	
	//TODO : ERROR if A.shape != B.shape
	public static Matrix<T> HadamardProduct(Matrix<T> A, Matrix<T> B) {
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		Matrix<T> C = Zeros(m,n);

		for(int i = 0; i < m * n; i++) {
			double a = System.Convert.ToDouble(A[i]);
			double b = System.Convert.ToDouble(B[i]);
			C[i] = (T)System.Convert.ChangeType(a * b, typeof(T));
		}

		return C;
	}

	//TRANSPOSE
	public static Matrix<T> Transpose(Matrix<T> A) {
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		Matrix<T> AT = Zeros(n, m);

		for(int i = 0; i < m; i++) {
			for(int j = 0; j < n; j++) {
				AT[j, i] = A[i, j];
			}
		}

		return AT;
	}
	
	//FROBENIUS NORM
	public T Norm() {
		double result = 0;
		for(int i = 0; i < data.Length; i++) {
			double e = System.Convert.ToDouble(data[i]);
			result += e * e;
		}
		result = System.Math.Sqrt(result);

		return (T)System.Convert.ChangeType(result, typeof(T));
		}
	
	//TRACE
	//TODO : ERROR if shape[0] != shape[1]
	public T Trace() {
		double result = 0;
		for(int i = 0; i < shape[0]; i++) {
			double e = System.Convert.ToDouble(GetElement(i, i));
			result += e;
		}

		return (T)System.Convert.ChangeType(result, typeof(T));
	}

	//DETERMINANT
	//TODO : ERROR if shape[0] != shape[1]
	public T Determinant(){
		int n = shape[0];
		if(n == 2) {
			double a = System.Convert.ToDouble(GetElement(0, 0));
			double b = System.Convert.ToDouble(GetElement(0, 1));
			double c = System.Convert.ToDouble(GetElement(1, 0));
			double d = System.Convert.ToDouble(GetElement(1, 1));
			
			double det = (a * d) - (b * c);
			return (T)System.Convert.ChangeType(det, typeof(T));
		}

		if(n == 1) {
			return data[0];
		}

		double result = 0;
		for(int k = 0; k < n; k++) {
			Matrix<T> subMat = Zeros(n - 1, n - 1);
			//i = 1 because we know we can skip top row
			for(int i = 1; i < n; i++) {
				for(int j = 0; j < n; j++) {
					if(j < k) {
						subMat[i - 1, j] = GetElement(i, j);
					} else if(j > k) {
						subMat[i - 1, j - 1] = GetElement(i, j);
					}
				}
			}

			double negative = (((k + 1)%2) * 2) - 1;
			double coefficient = System.Convert.ToDouble(GetElement(0, k));
			result += negative * coefficient * System.Convert.ToDouble(subMat.Determinant());
			}
		
		return (T)System.Convert.ChangeType(result, typeof(T));
	}

//CONDITIONS
//------------------------------------------------------------------------------
public bool IsSymmetric() {
	if(shape[0] != shape[1]) {
		return false;
	}

	for(int i = 0; i < shape[0] - 1; i++) {
		for(int j = i + 1; j < shape[0]; j++) {
			if(!GetElement(i, j).Equals(GetElement(j, i))) {
				return false;
			}
		}
	}

	return true;
}

}
} // END namespace lmath
