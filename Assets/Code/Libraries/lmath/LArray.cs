//==============================================================================
// Filename: LArray.cs
// Author: Aaron Thompson
// Date Created: 6/7/2020
// Last Updated: 6/7/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lmath {
public abstract class LArray<T> where T : System.IConvertible {
// VARIABLES
//------------------------------------------------------------------------------
	protected T[] data;
	protected int[] shape;
	public int rank { get { return shape.Length; } }
	public static double epsilon = 0.00001;

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public LArray() {
		data = new T[0];
		shape = new int[0];
	}

	public LArray(System.Array data) {
		SetData(data);
	}

	public LArray(T[] data, int[] shape) {
		this.data = new T[data.Length];
		this.shape = new int[shape.Length];
		Reshape(shape);
		SetData(data);
	}
	
	public LArray(T e, int[] shape) {
		this.data = new T[data.Length];
		this.shape = new int[shape.Length];
		Reshape(shape);
		Fill(e);
	}

	public LArray(LArray<T> larray) {
			data = new T[larray.GetLength()];
			shape = new int[larray.rank];
			for(int i = 0; i < rank; i++) {
				shape[i] = 0;
			}

			Copy(larray);
	}

// DATA MANAGEMENT
//------------------------------------------------------------------------------
	//ELEMENT
	public T GetElement(int index) {
		return data[index];
	}

	public T GetElement(int[] indices) {
		return data[GetIndex(indices)];
	}

	public void SetElement(T e, int index){
		data[index] = e;
	}

	public void SetElement(T e, int[] indices) {
		data[GetIndex(indices)] = e;
	}

	public T this[int index] {
		get {
			return GetElement(index);
		}

		set {
			SetElement(value, index);
		}
	}

	//DATA
	//TODO : ERROR for when data.length != this.data.length
	public void SetData(T[] data) {
		for(int i = 0; i < data.Length; i++) {
				SetElement(data[i], i);
		}
	}

	//TODO : ERROR for when System.Array data ranks != shape.length
	public void SetData(System.Array data) {
		T[] data1D = NDArrayTo1DArray(data);
		int totalLength = 1;
		
		shape = new int[data.Rank];
		for(int i = 0; i < rank; i++) {
			shape[i] =  data.GetLength(i);
			totalLength *= shape[i];
		}
		this.data = new T[totalLength];
		SetData(data1D);
	}

	public T[] GetData() {
			T[] data = new T[this.data.Length];
			System.Array.Copy(this.data, data, data.Length);
			return data;
	}

	//SLICE
	//TODO : ERROR for when range is outside shape lengths
	//TODO : ERROR for when data length != total length
	//This function uses an INCLUSIVE [a, b] range
	public void SetSlice(T[] data, int[,] range) {
		int rank = range.GetLength(0);
		int[] coordinate = new int[rank];
		int totalLength = 1;
		
		for(int i = 0; i < rank; i++) {
			totalLength *= (range[i, 1] - range[i, 0]) + 1;
			coordinate[i] = range[i, 0];
		}

		for(int i = 0; i < totalLength; i++) {
			for(int j = rank - 1; j >= 0; j--) {
				if(coordinate[j] > range[j, 1]) {
					coordinate[j] = range[j, 0];
					if(j > 0) {
						coordinate[j - 1]++;
					}
				} else {
					SetElement(data[i], coordinate);
					break;
				}
			}

			coordinate[rank - 1]++;
		}
	}
	
	public void SetSlice(System.Array data, int[,] range) {
		T[] data1D = NDArrayTo1DArray(data);
		SetSlice(data1D, range);
	}

	public void SetSlice(LArray<T> larray, int[,] range) {
		SetSlice(larray.GetData(), range);
	}
	
	//TODO : ERROR for when range is outside rank lengths
	//This function uses an INCLUSIVE [a, b] range
	public T[] GetSlice(int[,] range) {
			//int[,] range -> {{a1, b1}, {a2, b2}, . . ., {aN, bN}}
			int rank = range.GetLength(0);
			int[] coordinate = new int[rank];
			int totalLength = 1;

			for(int i = 0; i < rank; i++) {
				totalLength *= (range[i, 1] - range[i, 0]) + 1;
				coordinate[i] = range[i, 0];
			}

			T[] slice = new T[totalLength];
			for(int i = 0; i < totalLength; i++){
				for(int j = rank - 1; j >= 0; j--) {
					if(coordinate[j] > range[j, 1]) {
						coordinate[j] = range[j, 0];
						if(j > 0) {
							coordinate[j - 1]++;
						}
					} else {
						slice[i] = data[GetIndex(coordinate)];
						break;
					}
				}

				coordinate[rank - 1]++;
			}

			return slice;
	}
	
	//SHAPE
	public int[] GetShape() {
		int[] shape = new int[this.shape.Length];
		System.Array.Copy(this.shape, shape, shape.Length);
		return shape;
	}

	public int GetLength() {
		int totalLength = 1;
		for(int i = 0; i < shape.Length; i++) {
			totalLength *= shape[i];
		}
		return totalLength;
	}
	
	//TODO : ERROR for when shape.length != this.shape.length 
	//AND shape.length > 0
	//Default value is 0
	public void Reshape(int[] shape) {
		int totalLength = 1;
		int rank = shape.Length;
		int[] coordinate = new int[rank];
		for(int i = 0; i < rank; i++) {
			coordinate[i] = 0;
			totalLength *= shape[i];
		}
		T zero = (T)System.Convert.ChangeType(0, typeof(T));
		T[] data = new T[totalLength];
		if(this.shape.Length == 0) {
			this.shape = new int[shape.Length];
			for(int i = 0; i < shape.Length; i++) {
				this.shape[i] = 0;
			}
		}

		for(int i = 0; i < totalLength; i++) {
			for (int j = rank - 1; j >= 0; j--) {
				if (coordinate[j] > shape[j]) {
					coordinate[j] = 0;
					if(j > 0) {
						coordinate[j - 1]++;
					}
				} else {
					break;
				}
			}

			bool outOfBounds = false;
			for (int j = 0; j < rank; j++) {
				//MonoBehaviour.print(string.Join(", ", new List<int>(this.shape).ConvertAll(x => x.ToString()).ToArray()));
				if (coordinate[j] >= this.shape[j]) {
					data[i] = zero;
					outOfBounds = true;
					break;
				}
			}

			if (!outOfBounds) {
				//MonoBehaviour.print(string.Join(", ", new List<int>(coordinate).ConvertAll(x => x.ToString()).ToArray()));
				data[i] = GetElement(coordinate);
			}

			coordinate[rank - 1]++;
		}

		this.data = new T[totalLength];
		System.Array.Copy(data, this.data, totalLength);
		System.Array.Copy(shape, this.shape, rank);
	}
	
	public void Copy(LArray<T> larray) {
		Reshape(larray.GetShape());
		
		for(int i = 0; i < data.Length; i++) {
			data[i] = larray.GetElement(i);
		}
	}

	public void Fill(T e) {
		for(int i = 0; i < data.Length; i++) {
				data[i] = e;
		}
	}

	//TODO : ERROR for when indices.length !=  shape.length
	//TODO : ERROR for when indices[i] != shape[i]
	protected int GetIndex(int[] indices) {
		int index = indices[rank - 1];

		for(int i = 0; i <= rank - 2; i++) {
			int product = indices[i];

			for(int j = 1 + i; j <= rank - 1; j++) {
				product *= shape[j];
			}

			index += product;
		}

		return index;
	}

// HELPER FUNCTIONS
//------------------------------------------------------------------------------
	private static T[] NDArrayTo1DArray(System.Array arrayND) {
		int rank = arrayND.Rank;
		long length = arrayND.Length;
		T[] array1D = new T[length];
		int[] dLength = new int[rank];
		int[] coordinate = new int[rank];

		//Setting up maximum length of each rank and intital 
		//coordinate
		for(int i = 0; i < rank; i++) {
			dLength[i] = arrayND.GetLength(i);
			coordinate[i] = 0;
		}

		//Iterating through a long since length is multiplicative via 
		//ranks which scales very quickly
		for(long i = 0; i < length; i++) {
			array1D[i] = (T)arrayND.GetValue(coordinate);
			coordinate[rank - 1]++;
			for(int j = rank - 1; j >= 0; j--) {
				if(coordinate[j] >= dLength[j]) {
					coordinate[j] = 0;
					if(j > 0) {
						coordinate[j - 1]++;
					}
				} else {
					break;
				}
			}
		}

		return array1D;
	}

// OPERATIONS
//------------------------------------------------------------------------------
	//ADDITION
	//TODO : ERROR if shape != larray.shape
	public void Add(LArray<T> larray) {
		for(int i = 0; i < larray.GetLength(); i++){
			double a = System.Convert.ToDouble(data[i]);
			double b = System.Convert.ToDouble(larray.GetElement(i));
			T e = (T)System.Convert.ChangeType(a + b, typeof(T));
			data[i] = e;
		}
	}

	//SCALAR MULTIPLICATION
	public void Scale(T c) {
		for(int i = 0; i < data.Length; i++){
			double a = System.Convert.ToDouble(data[i]);
			double b = System.Convert.ToDouble(c);
			T e = (T)System.Convert.ChangeType(a * b, typeof(T));
			data[i] = e;
		}
	}

	//SUBTRACT
	public void Negate() {
		T neg = (T)System.Convert.ChangeType(-1, typeof(T));
		Scale(neg);
	}

	public void Subtract(LArray<T> larray) {
		larray.Negate();
		Add(larray);
		larray.Negate();
	}

	//RANDOMIZE
	//TODO : ERROR if shape != min.shape != max.shape
	public void Randomize(LArray<T> min, LArray<T> max) {
		for(int i = 0; i < data.Length; i++) {
			float minValue = System.Convert.ToSingle(min.GetElement(i));
			float maxValue = System.Convert.ToSingle(max.GetElement(i));
			float e = UnityEngine.Random.Range(minValue, maxValue);
			data[i] = (T)System.Convert.ChangeType(e, typeof(T));
		}
	}

	//PRINT
	public override string ToString() {
		string s = "";
		int[] coordinate = new int[rank];
		int brackets = rank;
		
		for(long i = 0; i < data.LongLength; i++) {
			while(brackets > 0){
				s += "[";
				brackets--;
			}

			s += data[i].ToString();
			coordinate[rank - 1]++;

			if(coordinate[rank - 1] < shape[rank - 1]) {
				s += ", ";
				continue;
			} else {
				for (int j = rank - 1; j >= 0; j--) {
					if (coordinate[j] >= shape[j]) {
						coordinate[j] = 0;
						if(j > 0) {
							coordinate[j - 1]++;
						}
						s += "]";
						brackets++;
					} else {
						s += ", ";
						break;
					}
				}
			}
		}

		return s;
	}
	
	public void Print() {
		MonoBehaviour.print(ToString());
	}
}
} // END namespace lmath

