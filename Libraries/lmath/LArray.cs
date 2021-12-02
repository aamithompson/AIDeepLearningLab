//==============================================================================
// Filename: LArray.cs
// Author: Aaron Thompson
// Date Created: 6/7/2020
// Last Updated: 8/27/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using statistics;

namespace lmath {
public abstract class LArray<T> where T : System.IConvertible {
// VARIABLES
//------------------------------------------------------------------------------
	protected double[] data;
	protected int[] shape;
	public int rank { get { return shape.Length; } }
	public static double epsilon = 0.00001;

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public LArray() {
		data = new double[0];
		shape = new int[0];
	}

	public LArray(System.Array data) {
		SetData(data);
	}

	public LArray(T[] data, int[] shape) {
		this.data = new double[data.Length];
		this.shape = new int[shape.Length];
		Reshape(shape);
		SetData(data);
	}
	
	public LArray(T e, int[] shape) {
		this.data = new double[data.Length];
		this.shape = new int[shape.Length];
		Reshape(shape);
		Fill(e);
	}

	public LArray(LArray<T> larray) {
			data = new double[larray.GetLength()];
			shape = new int[larray.rank];
			for(int i = 0; i < rank; i++) {
				shape[i] = 0;
			}

			Copy(larray);
	}

// DATA MANAGEMENT
//------------------------------------------------------------------------------
	//ELEMENT
	public double GetDouble(int index) {
		if(index < 0) {
			index = GetLength() + index;
        }

		return data[index];
    }

	public double GetDouble(int[] indices) {
		return GetDouble(GetIndex(indices));
    }

	public T GetElement(int index) {
		return (T)System.Convert.ChangeType(GetDouble(index), typeof(T));
	}

	public T GetElement(int[] indices) {
		return GetElement(GetIndex(indices));
	}

	public void SetDouble(double e, int index) {
		if(index < 0) {
			index = GetLength() + index;
        }

		data[index] = e;
    }

	public void SetDouble(double e, int[] indices) {
		SetDouble(e, GetIndex(indices));
	}

	public void SetElement(T e, int index){
		SetDouble(System.Convert.ToDouble(e), index);
	}

	public void SetElement(T e, int[] indices) {
		SetElement(e, GetIndex(indices));
	}

	public T this[int index] {
		get {
			return GetElement(index);
		}

		set {
			SetElement(value, index);
		}
	}

	public void SetDoubleData(double[] data) {
		for(int i = 0; i < data.Length; i++) {
				SetDouble(data[i], i);
		}
	}

	//DATA
	//TODO : ERROR for when data.length != this.data.length
	public void SetData(T[] data) {
		for(int i = 0; i < data.Length; i++) {
				SetElement(data[i], i);
		}
	}

	public void SetDoubleData(System.Array data) {
		double[] data1D = NDArrayTo1DArrayDouble(data);
		int totalLength = 1;
		
		shape = new int[data.Rank];
		for(int i = 0; i < rank; i++) {
			shape[i] =  data.GetLength(i);
			totalLength *= shape[i];
		}
		this.data = new double[totalLength];
		SetDoubleData(data1D);
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
		this.data = new double[totalLength];
		SetData(data1D);
	}

	public double[] GetDoubleData() {
		double[] data = new double[this.data.Length];
		for (int i = 0; i < data.Length; i++) {
			data[i] = GetDouble(i);
		}

		return data;
	}

	public T[] GetData() {
		T[] data = new T[this.data.Length];
		for(int i = 0; i < data.Length; i++) {
			data[i] = GetElement(i);
        }

		return data;
	}

	//SLICE
	//TODO : ERROR for when range is outside shape lengths
	//TODO : ERROR for when data length != total length
	//This function uses an INCLUSIVE [a, b] range
	public void SetDoubleSlice(double[] data, int[,] range) {
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
					SetDouble(data[i], coordinate);
					break;
				}
			}

			coordinate[rank - 1]++;
		}
	}

	public void SetDoubleSlice(System.Array data, int[,] range) {
		double[] data1D = NDArrayTo1DArrayDouble(data);
		SetDoubleSlice(data1D, range);
    }

	public void SetSlice(LArray<T> larray, int[,] range) {
		SetDoubleSlice(larray.GetDoubleData(), range);
	}

	public void SetSlice(T[] data, int[,] range) {
		double[] arr = new double[data.Length];
		for(int i = 0; i < data.Length; i++) {
			arr[i] = System.Convert.ToDouble(data[i]);
        }

		SetDoubleSlice(arr, range);
	}
	
	public void SetSlice(System.Array data, int[,] range) {
		T[] data1D = NDArrayTo1DArray(data);
		SetSlice(data1D, range);
	}
	
	//TODO : ERROR for when range is outside rank lengths
	//This function uses an INCLUSIVE [a, b] range
	public double[] GetDoubleSlice(int[,] range) {
			//int[,] range -> {{a1, b1}, {a2, b2}, . . ., {aN, bN}}
			int rank = range.GetLength(0);
			int[] coordinate = new int[rank];
			int totalLength = 1;

			for(int i = 0; i < rank; i++) {
				totalLength *= (range[i, 1] - range[i, 0]) + 1;
				coordinate[i] = range[i, 0];
			}

			double[] slice = new double[totalLength];
			for(int i = 0; i < totalLength; i++){
				for(int j = rank - 1; j >= 0; j--) {
					if(coordinate[j] > range[j, 1]) {
						coordinate[j] = range[j, 0];
						if(j > 0) {
							coordinate[j - 1]++;
						}
					} else {
						slice[i] = GetDouble(coordinate);
						break;
					}
				}

				coordinate[rank - 1]++;
			}

			return slice;
	}

	public T[] GetSlice(int[,] range) {
		double[] arr = GetDoubleSlice(range);
		T[] slice = new T[arr.Length];
		for(int i = 0; i < arr.Length; i++) {
			slice[i] = (T)System.Convert.ChangeType(arr[i], typeof(T));
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

		double[] data = new double[totalLength];
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
					data[i] = 0;
					outOfBounds = true;
					break;
				}
			}

			if (!outOfBounds) {
				//MonoBehaviour.print(string.Join(", ", new List<int>(coordinate).ConvertAll(x => x.ToString()).ToArray()));
				data[i] = GetDouble(coordinate);
			}
			
			coordinate[rank - 1]++;
		}

		this.data = new double[totalLength];
		SetDoubleData(data);
		System.Array.Copy(shape, this.shape, rank);
	}
	
	public void Copy(LArray<T> larray) {
		Reshape(larray.GetShape());
		
		for(int i = 0; i < data.Length; i++) {
			data[i] = larray.GetDouble(i);
		}
	}

	public void Fill(T e) {
		double value = System.Convert.ToDouble(e);
		for(int i = 0; i < data.Length; i++) {
				data[i] = value;
		}
	}

	//TODO : ERROR for when indices.length !=  shape.length
	//TODO : ERROR for when indices[i] != shape[i]
	protected int GetIndex(int[] indices) {
		for(int i = 0; i < rank; i++) {
			if(indices[i] < 0) {
				indices[i] = shape[i] + indices[i];
            }
        }

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
	private static double[] NDArrayTo1DArrayDouble(System.Array arrayND) {
		int rank = arrayND.Rank;
		long length = arrayND.Length;
		double[] array1D = new double[length];
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
			array1D[i] = (double)arrayND.GetValue(coordinate);
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
		for(int i = 0; i < larray.GetLength(); i++) {
			data[i] += larray.GetDouble(i);
		}
	}

	//SCALAR MULTIPLICATION
	public void Scale(T c) {
		double value = System.Convert.ToDouble(c);
		for(int i = 0; i < data.Length; i++) {
			data[i] *= value;
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

	public void HadamardProduct(LArray<T> larray) {
		for(int i = 0; i < data.Length; i++) {
			data[i] *= larray.GetDouble(i);
		}
    }

	//RANDOMIZE
	//TODO : ERROR if shape != min.shape != max.shape
	public void Randomize(LArray<T> min, LArray<T> max) {
		for(int i = 0; i < data.Length; i++) {
			float minValue = System.Convert.ToSingle(min.GetElement(i));
			float maxValue = System.Convert.ToSingle(max.GetElement(i));
			float e = UnityEngine.Random.Range(minValue, maxValue);
			data[i] = e;
		}
	}

	public void RandomizeN(float mean, float stdDev) {
		for(int i = 0; i < data.Length; i++) {
			float e = Statistics.randomN(mean, stdDev);
			data[i] = e;
		}
	}

	//PRINT
	public override string ToString() {
		string s = "";
		int[] coordinate = new int[rank];
		int brackets = rank;
		
		for(int i = 0; i < data.LongLength; i++) {
			while(brackets > 0){
				s += "[";
				brackets--;
			}

			s += GetElement(i).ToString();
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

	public void Operation(System.Func<T, T> f) {
		for(int i = 0; i < data.Length; i++) {
			data[i] = System.Convert.ToDouble(f(GetElement(i)));
        }
    }
}
} // END namespace lmath

