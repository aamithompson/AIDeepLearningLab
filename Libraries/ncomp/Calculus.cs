//==============================================================================
// Filename: Calculus.cs
// Author: Aaron Thompson
// Date Created: 7/19/2020
// Last Updated: 9/28/2025
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
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

	public static float PartialDerivative(System.Func<Vector, float> f, Vector dims, Vector x) {
		Vector xh = new Vector(x);
		xh[(int)dims[0]] = xh[(int)dims[0]] + h;

		if (dims.length == 1) {
			return (f(xh) - f(x)) / h;
		} else {
			Vector dimsMinus = Vector.Zeros(dims.length - 1);
			for(int i = 1; i < dims.length; i++) {
				dimsMinus[i - 1] = dims[i];
			}

			return (PartialDerivative(f, dimsMinus, xh) - PartialDerivative(f, dimsMinus, x)) / h;
		}
	}

	public static float PartialDerivative(System.Func<Vector, float> f, int dim, Vector x, int n = 1) {
		Vector dims = Vector.Ones(n) * dim;
		
		return PartialDerivative(f, dims, x);
	}
	
	public static Matrix Jacobian(System.Func<Vector, float>[] functions, Vector x) {
		int n = x.length;
		Matrix matrix = Matrix.Zeros(n, n);

		for(int i = 0; i < n; i++) {
			for(int j = 0; j < n; j++) {
				matrix[i,j] = PartialDerivative(functions[i], j, x);
			}
		}

		return matrix;
	}

	public static Matrix Hessian(System.Func<Vector, float> f, Vector x) {
		int n = x.length;
		Matrix matrix = Matrix.Zeros(n, n);

		for(int i = 0; i < n; i++) {
			for(int j = 0; j < n; j++) {
				Vector dims = new Vector(new float[] {i, j});
				matrix[i,j] = PartialDerivative(f, dims, x);
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
			if (i%2 == 1) {
				sum += 4 * f(x);
			} else {
				sum += 2 * f(x);
			}
		}
		sum += f(x + dx);
		sum = (dx/3) * sum;

		return sum;
	}

	//Error Function
	public static float ErfDiferential(float x) {
		return System.MathF.Exp(-1 * (x * x));
	}

	public static float Erf(float x) {
		return (2/System.MathF.Sqrt(System.MathF.PI)) * IntegrateSim(ErfDiferential, 0, x);
	}


}
}// END namespace ncomp
//==============================================================================
//==============================================================================
