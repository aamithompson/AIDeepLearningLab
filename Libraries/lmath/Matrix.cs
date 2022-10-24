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
public class Matrix : LArray {
	public static int STRASSEN_MATRIX_SIZE = 128*128;

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public Matrix() {
		data = new float[0];
		shape = new int[2] { 0, 0 };
	}

	public Matrix(float[] data, int m, int n) {
		this.data = new float[data.Length];
		shape = new int[2] { m, n };
		Reshape(m, n);
		SetData(data);
	}

	public Matrix(float[,] data) {
		SetData(data);
	}
	
	public Matrix(Matrix matrix) {
		data = new float[matrix.GetLength()];
		shape = new int[2] { matrix.GetShape()[0], matrix.GetShape()[1] };
		Copy(matrix);
	}

// DATA MANAGEMENT
//------------------------------------------------------------------------------
	public float GetElement(int i, int j) {
		int index = (i * shape[1]) + j;
			return data[index];
	}

	public void SetElement(float e, int i, int j) {
		int index = (i * shape[1]) + j;
		data[index] = e;
	}

	public float this[int i, int j] {
		get {
			return GetElement(i, j);
		}

		set {
			SetElement(value, i, j);
		}
	}

	public Matrix GetRow(int i) {
		Matrix row = Zeros(1, shape[1]);

		for(int k = 0; k < shape[1]; k++) {
			row.SetElement(GetElement(i, k), 0, k);
		}
		
		return row;
	}

	public Matrix GetColumn(int j) {
		Matrix column = Zeros(shape[0], 1);
		
		for(int k = 0; k < shape[0]; k++) {
			column.SetElement(GetElement(k, j), k, 0);
		}

		return column;
	}

	public void Reshape(int i, int j) {
		Reshape(new int[] { i, j });
	}

// DEFAULT OBJECTS
//------------------------------------------------------------------------------
	public static Matrix Zeros(int m, int n) {
		Matrix matrix = new Matrix();
		
		matrix.Reshape(m, n);

		return matrix;
	}

	public static Matrix Ones(int m, int n) {
		Matrix matrix = Zeros(m, n);

		matrix.Fill(1.0f);

		return matrix;
	}

	public static Matrix Identity(int n) {
		Matrix matrix = Zeros(n, n);

		for(int i = 0; i < n; i++) {
			matrix.SetElement(1.0f, i, i);
		}

		return matrix;
	}

	public static Matrix Diag(float e, int m, int n) {
		Matrix matrix = Zeros(m, n);
		int diagonalLength = Mathf.Min(m, n);

		for(int i = 0; i < diagonalLength; i++) {
				matrix.SetElement(e, i, i);
		}

		return matrix;
	}
	
	public static Matrix Diag(float[] data, int m=-1, int n=-1) {
		if(m == -1) {
			m = data.Length;
		}
		if(n == -1) {
			n = data.Length;
		}
		Matrix matrix = Zeros(m, n);

		int max = Mathf.Min(new int[] { m, n, data.Length});
		for(int i = 0; i < max; i++) {
			matrix[i, i] = data[i];
		}
		
		return matrix;
	}

	public static Matrix Diag(Vector vector, int m=-1, int n=-1) {
		return Diag(vector.GetData(), m, n);
	}

// OPERATIONS
//------------------------------------------------------------------------------
	//ADDITION
	public static Matrix Add(Matrix A, Matrix B) {
		Matrix C = new Matrix(A);
		C.Add(B);
		return C;
	}

	public static Matrix operator +(Matrix A, Matrix B) {
		return Add(A, B);
	}

	//SCALAR MULTIPLICATION
	public static Matrix Scale(Matrix A, float c) {
		Matrix B = new Matrix(A);
		B.Scale(c);
		return B;
	}

	public static Matrix operator *(Matrix A, float c) {
		return Scale(A, c);
	}

	public static Matrix operator *(float c, Matrix A) {
		return Scale(A, c);
	}

	//SUBTRACT
	public static Matrix Negate(Matrix A) {
		Matrix B = new Matrix(A);
		B.Negate();
		return B;
	}

	public static Matrix operator -(Matrix A) {
		return Negate(A);
	}

	public static Matrix Subtract(Matrix A, Matrix B) {
		Matrix C = new Matrix(A);
		C.Subtract(B);
		return C;
	}

	public static Matrix operator -(Matrix A, Matrix B) {
		return Subtract(A, B);
	}

	//RANDOM
	public static Matrix Random(Matrix min, Matrix max) {
		Matrix matrix = Matrix.Zeros(min.shape[0], min.shape[1]);
		matrix.Randomize(min, max);
		return matrix;
	}

	public static Matrix Random(float min, float max, int m, int n) {
		Matrix matrix = Matrix.Zeros(m, n);
		matrix.Randomize(min, max);
		return matrix;
	}

	public static Matrix RandomN(Matrix mean, Matrix stdDev) {
		Matrix matrix = Matrix.Zeros(mean.shape[0], mean.shape[1]);
		matrix.RandomizeN(mean, stdDev);
		return matrix;
	}

	public static Matrix RandomN(float mean, float stdDev, int m, int n) {
		Matrix matrix = Matrix.Zeros(m, n);
		matrix.RandomizeN(mean, stdDev);
		return matrix;
	}

	//MATRIX MULTIPLICATION
	//TODO : ERROR if A.shape[1] != B.shape[0]
    public static Matrix MatMul(Matrix A, Matrix B) {
		// (m x n) X (n x p) -> m x p
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		int p = B.GetShape()[1];
		Matrix C = Zeros(m, p);
		
        float[] arrA = A.AccessData();
        float[] arrB = B.AccessData();
		float[] arrC = C.AccessData();
		for(int i = 0; i < m; i++) {
			for(int j = 0; j < p; j++) {
				float sum = 0;
				for(int k = 0; k < n; k++){
					float a = arrA[(i * n) + k];
					float b = arrB[(k * p) + j];
                    sum += a * b;
				}

				arrC[(i * p) + j] = sum;
			}
		}

		return C;
	}
	
	//https://en.wikipedia.org/wiki/Strassen_algorithm
	public static Matrix StrassenMul(Matrix A, Matrix B) {
		if(A.GetLength() < STRASSEN_MATRIX_SIZE && B.GetLength() < STRASSEN_MATRIX_SIZE) {
			return MatMul(A, B);
        }

		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		int p = B.GetShape()[1];
		int s = Mathf.Max(m, n, p);
		s = (s % 2 == 1) ? s + 1 : s;
		int sd2 = s/2;
		Matrix AP = Matrix.Zeros(s, s);
		Matrix BP = Matrix.Zeros(s, s);
		AP.SetSlice(A.GetData(), new int[,] { { 0, m - 1 }, { 0, n - 1 } });
		BP.SetSlice(B.GetData(), new int[,] { { 0, n - 1 }, { 0, p - 1 } });

		Matrix A11 = new Matrix(AP.GetSlice(new int[,] { { 0, sd2 - 1 }, { 0, sd2 - 1 } }), sd2, sd2);
		Matrix A12 = new Matrix(AP.GetSlice(new int[,] { { 0, sd2 - 1 }, { sd2, s - 1 } }), sd2, sd2);
		Matrix A21 = new Matrix(AP.GetSlice(new int[,] { { sd2, s - 1 }, { 0, sd2 - 1 } }), sd2, sd2);
		Matrix A22 = new Matrix(AP.GetSlice(new int[,] { { sd2, s - 1 }, { sd2, s - 1 } }), sd2, sd2);
		Matrix B11 = new Matrix(BP.GetSlice(new int[,] { { 0, sd2 - 1 }, { 0, sd2 - 1 } }), sd2, sd2);
		Matrix B12 = new Matrix(BP.GetSlice(new int[,] { { 0, sd2 - 1 }, { sd2, s - 1 } }), sd2, sd2);
		Matrix B21 = new Matrix(BP.GetSlice(new int[,] { { sd2, s - 1 }, { 0, sd2 - 1 } }), sd2, sd2);
		Matrix B22 = new Matrix(BP.GetSlice(new int[,] { { sd2, s - 1 }, { sd2, s - 1 } }), sd2, sd2);

		Matrix M1 = Matrix.StrassenMul(A11 + A22, B11 + B22);
		Matrix M2 = Matrix.StrassenMul(A21 + A22, B11);
		Matrix M3 = Matrix.StrassenMul(A11, B12 - B22);
		Matrix M4 = Matrix.StrassenMul(A22, B21 - B11);
		Matrix M5 = Matrix.StrassenMul(A11 + A12, B22);
		Matrix M6 = Matrix.StrassenMul(A21 - A11, B11 + B12);
		Matrix M7 = Matrix.StrassenMul(A12 - A22, B21 + B22);
		
		Matrix C11 = M1 + M4 - M5 + M7;
		Matrix C12 = M3 + M5;
		Matrix C21 = M2 + M4;
		Matrix C22 = M1 - M2 + M3 + M6;
		Matrix CP = Matrix.Zeros(s, s);
		CP.SetSlice(C11.GetData(), new int[,] { { 0, sd2 - 1 }, { 0, sd2 - 1 } });
		CP.SetSlice(C12.GetData(), new int[,] { { 0, sd2 - 1 }, { sd2, s - 1 } });
		CP.SetSlice(C21.GetData(), new int[,] { { sd2, s - 1 }, { 0, sd2 - 1 } });
		CP.SetSlice(C22.GetData(), new int[,] { { sd2, s - 1 }, { sd2, s - 1 } });

		if(m==n && n==p && p==s) {
			return CP;
        }

		Matrix C = new Matrix(CP.GetSlice(new int[,] { { 0, m - 1 }, { 0, p - 1 } }), m, p);
		return C;
	}

	public static Vector MatVecMul(Matrix A, Vector x) {
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		Vector y = Vector.Zeros(m);
        
        float[] arrA = A.AccessData();
        float[] arrX = x.AccessData();
		float[] arrY = y.AccessData();
		for(int i = 0; i < m; i++) {
			float sum = 0;
			for(int j = 0; j < n; j++) {
				float a = arrA[(i * n) + j];
				float b = arrX[j];
				sum += a * b;
            }

			arrY[i] = sum;
        }

		return y;
    }
	
	//TODO : ERROR if A.shape != B.shape
	public static Matrix HadamardProduct(Matrix A, Matrix B) {
		Matrix C = new Matrix(A);
		C.HadamardProduct(B);
		return C;
	}

	//TRANSPOSE
	public static Matrix Transpose(Matrix A) {
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		Matrix AT = Zeros(n, m);

		for(int i = 0; i < m; i++) {
			for(int j = 0; j < n; j++) {
				AT.SetElement(A.GetElement(i, j), j, i);
			}
		}

		return AT;
	}
	
	//FROBENIUS NORM
	public float Norm() {
		float result = 0;
		for(int i = 0; i < data.Length; i++) {
			float e = data[i];
			result += e * e;
		}
		result = (float)System.Math.Sqrt(result);

		return (float)System.Math.Sqrt(result);
		}
	
	//TRACE
	//TODO : ERROR if shape[0] != shape[1]
	public float Trace() {
		float result = 0;
		for(int i = 0; i < shape[0]; i++) {
			result += GetElement(i, i);
		}

		return result;
	}

	//DETERMINANT
	//TODO : ERROR if shape[0] != shape[1]
	public float Determinant(){
		int n = shape[0];
		if(n == 2) {
			float a = GetElement(0, 0);
			float b = GetElement(0, 1);
			float c = GetElement(1, 0);
			float d = GetElement(1, 1);

			return (a * d) - (b * c); ;
		}

		if(n == 1) {
			return data[0];
		}

		float result = 0;
		for(int k = 0; k < n; k++) {
			Matrix subMat = Zeros(n - 1, n - 1);
			//i = 1 because we know we can skip top row
			for(int i = 1; i < n; i++) {
				for(int j = 0; j < n; j++) {
					if(j < k) {
						subMat.SetElement(GetElement(i, j), i - 1, j);
					} else if(j > k) {
						subMat.SetElement(GetElement(i, j), i - 1, j - 1);
					}
				}
			}

			float negative = (((k + 1)%2) * 2) - 1;
			float coefficient = GetElement(0, k);
			result += negative * coefficient * subMat.Determinant();
		}
		
		return result;
	}

//CONDITIONS
//------------------------------------------------------------------------------
public bool IsSymmetric() {
	if(shape[0] != shape[1]) {
		return false;
	}

	for(int i = 0; i < shape[0] - 1; i++) {
		for(int j = i + 1; j < shape[0]; j++) {
			if(i == j) {
				continue;
            }

			if(GetElement(i, j) != (GetElement(j, i))) {
				return false;
			}
		}
	}

	return true;
}

}
} // END namespace lmath
