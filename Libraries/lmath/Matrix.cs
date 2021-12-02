//==============================================================================
// Filename: Matrix.cs
// Author: Aaron Thompson
// Date Created: 5/31/2020
// Last Updated: 11/29/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lmath {
public class Matrix<T> : LArray<T> where T : System.IConvertible {
	public static int STRASSEN_MATRIX_SIZE = 64*64;

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public Matrix() {
		data = new double[0];
		shape = new int[2] { 0, 0 };
	}

	public Matrix(double[] data, int m, int n) {
		this.data = new double[data.Length];
		shape = new int[2] { m, n };
		Reshape(m, n);
		SetDoubleData(data);
	}

	public Matrix(T[] data, int m, int n) {
		this.data = new double[data.Length];
		shape = new int[2] { m, n };
		Reshape(m, n);
		SetData(data);
	}

	public Matrix(T[,] data) {
		SetData(data);
	}
	
	public Matrix(Matrix<T> matrix) {
		data = new double[matrix.GetLength()];
		shape = new int[2] { matrix.GetShape()[0], matrix.GetShape()[1] };
		Copy(matrix);
	}

// DATA MANAGEMENT
//------------------------------------------------------------------------------
	public double GetDouble(int i, int j) {
		return GetDouble(new int[] { i, j });
    }

	public T GetElement(int i, int j) {
		return GetElement(new int[] { i, j });
	}

	public void SetDouble(double e, int i, int j) {
		SetDouble(e, new int[] { i, j });
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
			row.SetDouble(GetDouble(i, k), 0, k);
		}
		
		return row;
	}

	public Matrix<T> GetColumn(int j) {
		Matrix<T> column = Zeros(shape[0], 1);
		
		for(int k = 0; k < shape[0]; k++) {
			column.SetDouble(GetDouble(k, j), k, 0);
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

		for(int i = 0; i < n; i++) {
			matrix.SetDouble(1, i, i);
		}

		return matrix;
	}

	public static Matrix<T> Diag(T e, int m, int n) {
		Matrix<T> matrix = Zeros(m, n);
		int diagonalLength = Mathf.Min(m, n);

		double value = System.Convert.ToDouble(e);
		for(int i = 0; i < diagonalLength; i++) {
				matrix.SetDouble(value, i, i);
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

	public static Matrix<T> RandomN(float mean, float stdDev, int m, int n) {
		Matrix<T> matrix = Matrix<T>.Zeros(m, n);
		matrix.RandomizeN(mean, stdDev);
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
					double a = A.GetDouble(i, k);
					double b = B.GetDouble(k, j);
					sum += a * b;
				}
				C.SetDouble(sum, i, j);
			}
		}

		return C;
	}
	
	//https://en.wikipedia.org/wiki/Strassen_algorithm
	public static Matrix<T> StrassenMul(Matrix<T> A, Matrix<T> B) {
		if(A.GetLength() < STRASSEN_MATRIX_SIZE && B.GetLength() < STRASSEN_MATRIX_SIZE) {
			return MatMul(A, B);
        }

		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		int p = B.GetShape()[1];
		int s = Mathf.Max(m, n, p);
		s = (s % 2 == 1) ? s + 1 : s;
		int sd2 = s/2;
		Matrix<T> AP = Matrix<T>.Zeros(s, s);
		Matrix<T> BP = Matrix<T>.Zeros(s, s);
		AP.SetSlice(A.GetData(), new int[,] { { 0, m - 1 }, { 0, n - 1 } });
		BP.SetSlice(B.GetData(), new int[,] { { 0, n - 1 }, { 0, p - 1 } });

		Matrix<T> A11 = new Matrix<T>(AP.GetDoubleSlice(new int[,] { { 0, sd2 - 1 }, { 0, sd2 - 1 } }), sd2, sd2);
		Matrix<T> A12 = new Matrix<T>(AP.GetDoubleSlice(new int[,] { { 0, sd2 - 1 }, { sd2, s - 1 } }), sd2, sd2);
		Matrix<T> A21 = new Matrix<T>(AP.GetDoubleSlice(new int[,] { { sd2, s - 1 }, { 0, sd2 - 1 } }), sd2, sd2);
		Matrix<T> A22 = new Matrix<T>(AP.GetDoubleSlice(new int[,] { { sd2, s - 1 }, { sd2, s - 1 } }), sd2, sd2);
		Matrix<T> B11 = new Matrix<T>(BP.GetDoubleSlice(new int[,] { { 0, sd2 - 1 }, { 0, sd2 - 1 } }), sd2, sd2);
		Matrix<T> B12 = new Matrix<T>(BP.GetDoubleSlice(new int[,] { { 0, sd2 - 1 }, { sd2, s - 1 } }), sd2, sd2);
		Matrix<T> B21 = new Matrix<T>(BP.GetDoubleSlice(new int[,] { { sd2, s - 1 }, { 0, sd2 - 1 } }), sd2, sd2);
		Matrix<T> B22 = new Matrix<T>(BP.GetDoubleSlice(new int[,] { { sd2, s - 1 }, { sd2, s - 1 } }), sd2, sd2);

		Matrix<T> M1 = Matrix<T>.StrassenMul(A11 + A22, B11 + B22);
		Matrix<T> M2 = Matrix<T>.StrassenMul(A21 + A22, B11);
		Matrix<T> M3 = Matrix<T>.StrassenMul(A11, B12 - B22);
		Matrix<T> M4 = Matrix<T>.StrassenMul(A22, B21 - B11);
		Matrix<T> M5 = Matrix<T>.StrassenMul(A11 + A12, B22);
		Matrix<T> M6 = Matrix<T>.StrassenMul(A21 - A11, B11 + B12);
		Matrix<T> M7 = Matrix<T>.StrassenMul(A12 - A22, B21 + B22);
		
		Matrix<T> C11 = M1 + M4 - M5 + M7;
		Matrix<T> C12 = M3 + M5;
		Matrix<T> C21 = M2 + M4;
		Matrix<T> C22 = M1 - M2 + M3 + M6;
		Matrix<T> CP = Matrix<T>.Zeros(s, s);
		CP.SetDoubleSlice(C11.GetDoubleData(), new int[,] { { 0, sd2 - 1 }, { 0, sd2 - 1 } });
		CP.SetDoubleSlice(C12.GetDoubleData(), new int[,] { { 0, sd2 - 1 }, { sd2, s - 1 } });
		CP.SetDoubleSlice(C21.GetDoubleData(), new int[,] { { sd2, s - 1 }, { 0, sd2 - 1 } });
		CP.SetDoubleSlice(C22.GetDoubleData(), new int[,] { { sd2, s - 1 }, { sd2, s - 1 } });

		if(m==n && n==p && p==s) {
			return CP;
        }

		Matrix<T> C = new Matrix<T>(CP.GetSlice(new int[,] { { 0, m - 1 }, { 0, p - 1 } }), m, p);
		return C;
	}

	public static Vector<T> MatVecMul(Matrix<T> A, Vector<T> x) {
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		Vector<T> y = Vector<T>.Zeros(m);

		for(int i = 0; i < m; i++) {
			double sum = 0;
			for(int j = 0; j < n; j++) {
				double a = A.GetDouble(i, j);
				double b = x.GetDouble(j);
				sum += a * b;
            }

			y.SetDouble(sum, i);
        }

		return y;
    }
	
	//TODO : ERROR if A.shape != B.shape
	public static Matrix<T> HadamardProduct(Matrix<T> A, Matrix<T> B) {
		Matrix<T> C = new Matrix<T>(A);
		C.HadamardProduct(B);
		return C;
	}

	//TRANSPOSE
	public static Matrix<T> Transpose(Matrix<T> A) {
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		Matrix<T> AT = Zeros(n, m);

		for(int i = 0; i < m; i++) {
			for(int j = 0; j < n; j++) {
				AT.SetDouble(A.GetDouble(i, j), j, i);
			}
		}

		return AT;
	}
	
	//FROBENIUS NORM
	public T Norm() {
		double result = 0;
		for(int i = 0; i < data.Length; i++) {
			double e = data[i];
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
			result += GetDouble(i, i);
		}

		return (T)System.Convert.ChangeType(result, typeof(T));
	}

	//DETERMINANT
	//TODO : ERROR if shape[0] != shape[1]
	public T Determinant(){
		int n = shape[0];
		if(n == 2) {
			double a = GetDouble(0, 0);
			double b = GetDouble(0, 1);
			double c = GetDouble(1, 0);
			double d = GetDouble(1, 1);
			
			double det = (a * d) - (b * c);
			return (T)System.Convert.ChangeType(det, typeof(T));
		}

		if(n == 1) {
			return (T)System.Convert.ChangeType(data[0], typeof(T));
		}

		double result = 0;
		for(int k = 0; k < n; k++) {
			Matrix<T> subMat = Zeros(n - 1, n - 1);
			//i = 1 because we know we can skip top row
			for(int i = 1; i < n; i++) {
				for(int j = 0; j < n; j++) {
					if(j < k) {
						subMat.SetDouble(GetDouble(i, j), i - 1, j);
					} else if(j > k) {
						subMat.SetDouble(GetDouble(i, j), i - 1, j - 1);
					}
				}
			}

			double negative = (((k + 1)%2) * 2) - 1;
			double coefficient = GetDouble(0, k);
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
