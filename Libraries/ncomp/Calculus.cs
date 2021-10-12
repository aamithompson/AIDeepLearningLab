//==============================================================================
// Filename: Calculus.cs
// Author: Aaron Thompson
// Date Created: 7/19/2020
// Last Updated: 12/7/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;
//------------------------------------------------------------------------------
namespace ncomp { 
public static class Calculus {
	public static float h = 0.0001f;
	public static int n = 64;
	
//DERIVATIVE
//------------------------------------------------------------------------------
	public static float Derivative(System.Func<float, float> f, float x, int n=1){
		if(n==1) {
			return (f(x + h) - f(x)) / h;
		} else {
			return (Derivative(f, x + h, n-1) - Derivative(f, x, n-1)) / h;
		}
	}

	public static float PartialDerivative(System.Func<Vector<float>, float> f, Vector<int> dims, Vector<float> x) {
		Vector<float> xh = new Vector<float>(x);
		xh[dims[0]] = xh[dims[0]] + h;

		if (dims.length == 1) {
			return (f(xh) - f(x)) / h;
		} else {
			Vector<int> dimsMinus = Vector<int>.Zeros(dims.length - 1);
			for(int i = 1; i < dims.length; i++) {
				dimsMinus[i - 1] = dims[i];
			}

			return (PartialDerivative(f, dimsMinus, xh) - PartialDerivative(f, dimsMinus, x)) / h;
		}
	}

	public static float PartialDerivative(System.Func<Vector<float>, float> f, int dim, Vector<float> x, int n = 1) {
		Vector<int> dims = Vector<int>.Ones(n) * dim;
		
		return PartialDerivative(f, dims, x);
	}
	
	public static Matrix<float> Jacobian(System.Func<Vector<float>, float>[] functions, Vector<float> x) {
		int n = x.length;
		Matrix<float> matrix = Matrix<float>.Zeros(n, n);

		for(int i = 0; i < n; i++) {
			for(int j = 0; j < n; j++) {
				matrix[n,n] = PartialDerivative(functions[i], j, x);
			}
		}

		return matrix;
	}

	public static Matrix<float> Hessian(System.Func<Vector<float>, float> f, Vector<float> x) {
		int n = x.length;
		Matrix<float> matrix = Matrix<float>.Zeros(n, n);

		for(int i = 0; i < n; i++) {
			for(int j = 0; j < n; j++) {
				Vector<int> dims = new Vector<int>(new int[] {i, j});
				matrix[n,n] = PartialDerivative(f, dims, x);
			}
		}

		return matrix;
	}

//INTEGRAL
//------------------------------------------------------------------------------
	//Trapezoidal Rule	
	public static float IntegrateTrp(System.Func<float, float> f, float a, float b, int n=-1) {
		if(n == -1) {
			n = Calculus.n;
		}

		//Formula:
		//                           dx
		//integrate(f(x)dx, a, b) ~= -- (f(x0) + 2f(x1) + . . . + 2f(x(n-1)) + f(xn))
		//                            2
		//OR
		//
		// N   f(x(k-1)) + f(xk)
		//sum( ----------------- * dx )
		// k=1          2
		float dx = (b - a) / n;
		float x = a;

		float sum = f(x);
		for(int i = 1; i < n; i++) {
			x += dx;
			sum += 2 * f(x);
		}
		sum += f(x + dx);
		sum = (dx/2) * sum;

		return sum;
	}

	//Simpson's Rule
	public static float IntegrateSim(System.Func<float, float> f, float a, float b, int n = -1) {
		if (n == -1) {
			n = Calculus.n;
		}

		if (n%2 == 1) {
			n++;
		}

		//Formula:
		//                           dx
		//integrate(f(x)dx, a, b) ~= -- (f(x0) + 4f(x1) + 2f(x2) + 4f(x3) . . . + 2f(x(n-2)) + 4f(x(n-1)) + f(xn))
		//                            3
		//---> n is even
		float dx = (b - a) / n;
		float x = a;

		float sum = f(x);
		for(int i = 1; i < n; i++) {
			x += dx;
			if (n%2 == 1) {
				sum += 4 * f(x);
			} else {
				sum += 2 * f(x);
			}
		}
		sum += f(x + dx);
		sum = (dx/2) * sum;

		return sum;
	}

	//Error Function
	public static float ErfDiferential(float x) {
		return Mathf.Exp(-1 * (x * x));
	}

	public static float Erf(float x) {
		return (2/Mathf.Sqrt(Mathf.PI)) * IntegrateSim(ErfDiferential, 0, x);
	}


}
}// END namespace ncomp
//==============================================================================
//==============================================================================
