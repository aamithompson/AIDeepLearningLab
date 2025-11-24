//==============================================================================
// Filename: Matrix.cs
// Author: Aaron Thompson
// Date Created: 5/31/2020
// Last Updated: 9/17/2025
//
// Description:
//	The matrix, that is 2D derivation of the LArray class, is a math object
//	which contains m rows and n columns. Supports basic arithmetic as well as
//	common functions for matrix math such as Matrix Multiplication, Transforms,
//	Determinants, Traces, Norms, and Hadamard Product.
//  Resource for optimizing matrix multiplication:
//  https://www.youtube.com/watch?v=o7h_sYMk_oc
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lmath {

//Some considerations:
//Implement tiling block matrices for optimal cache ~32 x 32
//Implement L2 cache tiling
//Implement divide and conquer block matrix multiplication
public class Matrix : LArray {
	public static int STRASSEN_MATRIX_SIZE = 512*512;
	public static int BLOCK_MATRIX_SIZE = 32;

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
		if(i < 0) {
			i = shape[0] + i;
        }

		if(j < 0) {
			j = shape[1] + j;
        }

		Validate2DIndex(i, j);

		int index = (i * shape[1]) + j;
		return data[index];
	}

	public void SetElement(float e, int i, int j) {
		if(i < 0) {
			i = shape[0] + i;
        }

		if(j < 0) {
			j = shape[0] + j;
        }

		Validate2DIndex(i, j);

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
		if(i < 0) {
			i = shape[0] + i;
		}

		ValidateDataIndex(i, 0);

		Matrix row = Zeros(1, shape[1]);

		for(int k = 0; k < shape[1]; k++) {
			row.SetElement(GetElement(i, k), 0, k);
		}
		
		return row;
	}

	public Matrix GetColumn(int j) {
		if(j < 0) {
			j = shape[1] + j;
		}

		ValidateDataIndex(j, 1);

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
		int diagonalLength = System.Math.Min(m, n);

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

		if(m < data.Length || n < data.Length) {
			throw new System.ArgumentException($"m {m} and/or {n} are less than given data length {data.Length}.");
		}

		Matrix matrix = Zeros(m, n);

		int max = System.Math.Min(System.Math.Min(m, n), data.Length);
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
		ValidateNotNullArgument(A);
		Matrix C = new Matrix(A);
		C.Add(B);
		return C;
	}

	public static Matrix operator +(Matrix A, Matrix B) {
		return Add(A, B);
	}

	//SCALAR MULTIPLICATION
	public static Matrix Scale(Matrix A, float c) {
		ValidateNotNullArgument(A);
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
		ValidateNotNullArgument(A);
		Matrix B = new Matrix(A);
		B.Negate();
		return B;
	}

	public static Matrix operator -(Matrix A) {
		return Negate(A);
	}

	public static Matrix Subtract(Matrix A, Matrix B) {
		ValidateNotNullArgument(A);
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


	public static Matrix MatMul(Matrix A, Matrix B, bool parallel=false) {
		ValidateMatrixMulSize(A, B);
		// (m x n) X (n x p) -> m x p
		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		int p = B.GetShape()[1];
		Matrix C = Zeros(m, p);

		//Pre-Transpose Optimization
		B = Matrix.Transpose(B);

        float[] arrA = A.AccessData();
        float[] arrB = B.AccessData();
		float[] arrC = C.AccessData();

		//Naive Implementation
		/*for(int i = 0; i < m; i++) {
			for(int j = 0; j < p; j++) {
				float sum = 0;
				for(int k = 0; k < n; k++){
					float a = arrA[(i * n) + k];
					float b = arrB[(k * p) + j];
                    sum += a * b;
				}

				arrC[(i * p) + j] = sum;
			}
		}*/

		if(parallel) {
			int iblocks = (m/BLOCK_MATRIX_SIZE) + 1;
			int kblocks = (p/BLOCK_MATRIX_SIZE) + 1;
			Parallel.For(0, iblocks, iblock => {
				int ib = iblock * BLOCK_MATRIX_SIZE;
				int iMax = System.Math.Min(ib + BLOCK_MATRIX_SIZE, m);
				Parallel.For(0, kblocks, kblock => {
					int kb = iblock * BLOCK_MATRIX_SIZE;
					int kMax = System.Math.Min(kb + BLOCK_MATRIX_SIZE, n);
					for(int jb = 0; jb < p; jb += BLOCK_MATRIX_SIZE) {
						int jMax = System.Math.Min(jb + BLOCK_MATRIX_SIZE, p);
						for(int i = ib; i < iMax; i++) {
							for(int k = kb; k < kMax; k++) {
								for(int j = jb; j < jMax; j++) {
									arrC[(i * p) + j] += arrA[(i * n) + k] * arrB[(j * n) + k];
								}
							}
						}
					}
				});
			});
		} else {
			for(int ib = 0; ib < m; ib += BLOCK_MATRIX_SIZE) {
				int iMax = System.Math.Min(ib + BLOCK_MATRIX_SIZE, m);
				for(int kb = 0; kb < n; kb += BLOCK_MATRIX_SIZE) {
					int kMax = System.Math.Min(kb + BLOCK_MATRIX_SIZE, n);
					for(int jb = 0; jb < p; jb += BLOCK_MATRIX_SIZE) {
						int jMax = System.Math.Min(jb + BLOCK_MATRIX_SIZE, p);
						for(int i = ib; i < iMax; i++) {
							for(int k = kb; k < kMax; k++) {
								for(int j = jb; j < jMax; j++) {
									arrC[(i * p) + j] += arrA[(i * n) + k] * arrB[(j * n) + k];
								}
							}
						}
					}
                }
            }
		}

		//Reversing Transposition
		B = Matrix.Transpose(B);

		return C;
	}



	public static Matrix NaiveMul(Matrix A, Matrix B) {
		ValidateMatrixMulSize(A, B);

		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		int p = B.GetShape()[1];
		Matrix C = Zeros(m, p);

		float[] arrA = A.AccessData();
		float[] arrB = B.AccessData();
		float[] arrC = C.AccessData();

		for (int i = 0; i < m; i++) {
			for (int j = 0; j < p; j++) {
				for (int k = 0; k < n; k++) {
					arrC[(i * p) + j] += arrA[(i * n) + k] * arrB[(k * p) + j];
				}
			}
		}

		return C;
	}
	
	//https://en.wikipedia.org/wiki/Strassen_algorithm
	public static Matrix StrassenMul(Matrix A, Matrix B, bool parallel=true) {
		ValidateMatrixMulSize(A, B);

		if(A.GetLength() < STRASSEN_MATRIX_SIZE && B.GetLength() < STRASSEN_MATRIX_SIZE) {
			return MatMul(A, B, parallel);
        }

		int m = A.GetShape()[0];
		int n = A.GetShape()[1];
		int p = B.GetShape()[1];
		int s = System.Math.Max(System.Math.Max(m, n), p);
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

		Matrix M1 = new Matrix();
		Matrix M2 = new Matrix();
		Matrix M3 = new Matrix();
		Matrix M4 = new Matrix();
		Matrix M5 = new Matrix();
		Matrix M6 = new Matrix();
		Matrix M7 = new Matrix();

        if (parallel) {
			Parallel.Invoke(
				() => M1 = Matrix.StrassenMul(A11 + A22, B11 + B22),
				() => M2 = Matrix.StrassenMul(A21 + A22, B11),
				() => M3 = Matrix.StrassenMul(A11, B12 - B22),
				() => M4 = Matrix.StrassenMul(A22, B21 - B11)
			);

			Parallel.Invoke(
				() => M5 = Matrix.StrassenMul(A11 + A12, B22),
				() => M6 = Matrix.StrassenMul(A21 - A11, B11 + B12),
				() => M7 = Matrix.StrassenMul(A12 - A22, B21 + B22)
			);
		} else {
			M1 = Matrix.StrassenMul(A11 + A22, B11 + B22);
			M2 = Matrix.StrassenMul(A21 + A22, B11);
			M3 = Matrix.StrassenMul(A11, B12 - B22);
			M4 = Matrix.StrassenMul(A22, B21 - B11);
			M5 = Matrix.StrassenMul(A11 + A12, B22);
			M6 = Matrix.StrassenMul(A21 - A11, B11 + B12);
			M7 = Matrix.StrassenMul(A12 - A22, B21 + B22);
		}
		
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
		if(n != x.GetLength()) {
			throw new System.ArgumentException($"Matrix A shape at index 1 {n} does not match vector length {x.GetLength()}.");
        }

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
	
	public static Matrix HadamardProduct(Matrix A, Matrix B) {
		ValidateNotNullArgument(A);
		Matrix C = new Matrix(A);
		C.HadamardProduct(B);
		return C;
	}

	//TRANSPOSE
	public static Matrix Transpose(Matrix A) {
		ValidateNotNullArgument(A);
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
	public float Trace() {
		ValidateSquareMatrix();

		float result = 0;
		for(int i = 0; i < shape[0]; i++) {
			result += GetElement(i, i);
		}

		return result;
	}

	//DETERMINANT
	public float Determinant(){
		ValidateSquareMatrix();

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

// VALDIDATION FUNCTION(s)
//------------------------------------------------------------------------------
	protected void Validate2DIndex(int i, int j) {
		bool ierror = i < 0 || i >= shape[0];
		bool jerror = j < 0 || j >= shape[1];
		if(ierror && jerror) {
			throw new System.IndexOutOfRangeException($"Index i {i} and index j {j} are out of bounds of expected max values {shape[0] - 1} and {shape[1] - 1} respectively.");
		}

		if(ierror) {
			throw new System.IndexOutOfRangeException($"Index i {i} is out of bounds of expected max value {shape[0] - 1}.");
		}

		if(jerror) {
			throw new System.IndexOutOfRangeException($"Index j {j} is out of bounds of expected max value {shape[1] - 1}.");
		}
	}

	protected static void ValidateMatrixMulSize(Matrix A, Matrix B) {
		if(A.GetShape()[1] != B.GetShape()[0]) {
			throw new System.ArgumentException($"Matrix A shape size at the index 1 {A.GetShape()[1]} does not match Matrix B shape size at index 0 {B.GetShape()[0]}");
		}
    }

	protected void ValidateSquareMatrix() {
		if(shape[0] != shape[1]) {
			throw new System.ArgumentException($"Shape at index 0 {shape[0]} does not match shape at index 1 {shape[1]}");
		}
    }

}
} // END namespace lmath
