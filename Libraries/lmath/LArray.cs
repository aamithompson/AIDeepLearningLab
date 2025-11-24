//==============================================================================
// Filename: LArray.cs
// Author: Aaron Thompson
// Date Created: 6/7/2020
// Last Updated: 10/25/2025
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using statistics;

namespace lmath {
public abstract class LArray {
// VARIABLES
//------------------------------------------------------------------------------
	protected float[] data;
	protected int[] shape;
	public int rank { get { return shape.Length; } }
	public static float epsilon = 0.00001f;

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public LArray() {
		data = new float[0];
		shape = new int[1] { 0 };
	}

	public LArray(System.Array data) {
		ValidateNotNullArgument(data);

		SetData(data);
	}

	public LArray(float[] data, int[] shape) {
		ValidateNotNullArgument(data);
		ValidateDefinedShape(shape);

		this.data = new float[data.Length];
		this.shape = new int[shape.Length];
		Reshape(shape);
		SetData(data);
	}
	
	public LArray(float e, int[] shape) {
		ValidateDefinedShape(shape);

		this.data = new float[data.Length];
		this.shape = new int[shape.Length];
		Reshape(shape);
		Fill(e);
	}

	public LArray(LArray larray) {
		ValidateNotNullArgument(larray);

		data = new float[larray.GetLength()];
		shape = new int[larray.rank];
		for(int i = 0; i < rank; i++) {
			shape[i] = 0;
		}

		Copy(larray);
	}

// DATA MANAGEMENT
//------------------------------------------------------------------------------
	//ELEMENT
	public float GetElement(int index) {
		if(index < 0) {
			index = GetLength() + index;
        }

		ValidateDataIndex(index);

		return data[index];
    }

	public float GetElement(int[] indices) {
		return GetElement(GetIndex(indices));
    }

	public void SetElement(float e, int index) {
		if(index < 0) {
			index = GetLength() + index;
        }

		ValidateDataIndex(index);

		data[index] = e;
    }

	public void SetElement(float e, int[] indices) {
		SetElement(e, GetIndex(indices));
	}

	public float this[int index] {
		get {
			return GetElement(index);
		}

		set {
			SetElement(value, index);
		}
	}

	//DATA
	public void SetData(float[] data) {
		for (int i = 0; i < data.Length; i++) {
				this.data[i] = data[i];
		}
	}

	public void SetData(System.Array data) {
		float[] data1D = NDArrayTo1DArray(data);
		int totalLength = 1;
		
		shape = new int[data.Rank];
		for(int i = 0; i < rank; i++) {
			shape[i] =  data.GetLength(i);
			totalLength *= shape[i];
		}
		this.data = new float[totalLength];
		SetData(data1D);
	}

	public float[] GetData() {
		float[] data = new float[this.data.Length];
		for (int i = 0; i < data.Length; i++) {
			data[i] = this.data[i];
		}

		return data;
	}

    public float[] AccessData() {
        return data;
    }

	//SLICE
	//This function uses an INCLUSIVE [a, b] range for each dimension.
	public void SetSlice(float[] data, int[,] range) {
		ValidateRange(range);

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
		float[] data1D = NDArrayTo1DArray(data);
		SetSlice(data1D, range);
    }

	public void SetSlice(LArray larray, int[,] range) {
		SetSlice(larray.AccessData(), range);
	}
	
	public float[] GetSlice(int[,] range) {
			ValidateRange(range);
			//int[,] range -> {{a1, b1}, {a2, b2}, . . ., {aN, bN}}
			int rank = range.GetLength(0);
			int[] coordinate = new int[rank];
			int totalLength = 1;

			for(int i = 0; i < rank; i++) {
				totalLength *= (range[i, 1] - range[i, 0]) + 1;
				coordinate[i] = range[i, 0];
			}

			float[] slice = new float[totalLength];
			for(int i = 0; i < totalLength; i++){
				for(int j = rank - 1; j >= 0; j--) {
					if(coordinate[j] > range[j, 1]) {
						coordinate[j] = range[j, 0];
						if(j > 0) {
							coordinate[j - 1]++;
						}
					} else {
						slice[i] = GetElement(coordinate);
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

	//Default value is 0
	public void Reshape(int[] shape) {
		ValidateDefinedShape(shape);
		ValidateEqualRank(shape);

		int totalLength = 1;
		int rank = shape.Length;
		int[] coordinate = new int[rank];
		for(int i = 0; i < rank; i++) {
			coordinate[i] = 0;
			totalLength *= shape[i];
		}

		float[] data = new float[totalLength];
		if(this.shape.Length == 0) {
			this.shape = new int[rank];
			for(int i = 0; i < rank; i++) {
				this.shape[i] = 0;
			}
		}

		for(int i = 0; i < totalLength; i++) {
			for (int j = rank - 1; j >= 0; j--) {
				if (coordinate[j] >= shape[j]) {
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
				if (coordinate[j] >= this.shape[j]) {
					data[i] = 0;
					outOfBounds = true;
					break;
				}
			}

			if (!outOfBounds) {
				data[i] = GetElement(coordinate);
			}
			
			coordinate[rank - 1]++;
		}

		this.data = new float[totalLength];
		SetData(data);
		this.shape = new int[rank];
		System.Array.Copy(shape, this.shape, rank);
	}
	
	public void Copy(LArray larray) {
		ValidateEqualRank(larray);

		Reshape(larray.GetShape());
		
		float[] data = larray.AccessData();
		for(int i = 0; i < data.Length; i++) {
			this.data[i] = data[i];
		}
	}

	public void Fill(float e) {
		for(int i = 0; i < data.Length; i++) {
				data[i] = e;
		}
	}
	
	protected int GetIndex(int[] indices) {
		ValidateNotNullArgument(indices);
		ValidateDataIndices(indices);
		
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
	private static float[] NDArrayTo1DArray(System.Array arrayND) {
		int rank = arrayND.Rank;
		long length = arrayND.Length;
		float[] array1D = new float[length];
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
			array1D[i] = (float)arrayND.GetValue(coordinate);
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
	public void Add(LArray larray) {
		ValidateNotNullArgument(larray);
		ValidateEqualShape(larray);

		float[] data = larray.AccessData();
		for(int i = 0; i < larray.GetLength(); i++) {
			this.data[i] += data[i];
		}
	}

	//SCALAR MULTIPLICATION
	public void Scale(float c) {
		for(int i = 0; i < data.Length; i++) {
			data[i] *= c;
		}
	}

	//SUBTRACT
	public void Negate() {
		float neg = -1.0f;
		Scale(neg);
	}

	public void Subtract(LArray larray) {
		ValidateNotNullArgument(larray);
		ValidateEqualShape(larray);

		larray.Negate();
		Add(larray);
		larray.Negate();
	}

	public void HadamardProduct(LArray larray) {
		ValidateNotNullArgument(larray);
		ValidateEqualShape(larray);

		float[] data = larray.AccessData();
		for(int i = 0; i < data.Length; i++) {
			this.data[i] *= data[i];
		}
    }

	//BOOLEAN
	public bool ContentEquals(LArray larray) {
		ValidateNotNullArgument(larray);

		int rank = shape.GetLength(0);
		if(larray.shape.GetLength(0) != rank) {
			return false;
		}

		for(int i = 0; i < rank; i++) {
			if(larray.shape[i] != this.shape[i]) {
				return false;
            }
		}

		float[] data = larray.AccessData();
		for(int i = 0; i < data.Length; i++) {
			if (data[i] != this.data[i]) {
				return false;
			}
        }

		return true;
	}

	//RANDOMIZE
	public void Randomize(LArray min, LArray max) {
		ValidateNotNullArgument(min);
		ValidateNotNullArgument(max);
		ValidateEqualShape(min);
		ValidateEqualShape(max);

		float[] minData = min.AccessData();
		float[] maxData = max.AccessData();
		for(int i = 0; i < data.Length; i++) {
			float minValue = minData[i];
			float maxValue = maxData[i];
			data[i] = Statistics.NextFloat(minValue, maxValue);
		}
	}

	public void Randomize(float min, float max) {
		for(int i = 0; i < data.Length; i++) { 
			data[i] = Statistics.NextFloat(min, max);
		}
	}

	public void RandomizeN(LArray mean, LArray stdDev) {
		ValidateNotNullArgument(mean);
		ValidateNotNullArgument(stdDev);
		ValidateEqualShape(mean);
		ValidateEqualShape(stdDev);

		float[] meanData = mean.AccessData();
		float[] stdDevData = stdDev.AccessData();
		for(int i = 0; i < data.Length; i++) {
			float meanValue = meanData[i];
			float stdDevValue = stdDevData[i];
			data[i] = Statistics.randomN(meanValue, stdDevValue);
        }
	}

	public void RandomizeN(float mean, float stdDev) {
		for(int i = 0; i < data.Length; i++) {
			data[i] = Statistics.randomN(mean, stdDev);
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
		System.Console.WriteLine(ToString());
	}

	public void Operation(System.Func<float, float> f) {
		for(int i = 0; i < data.Length; i++) {
			data[i] = f(data[i]);
        }
    }
// VALDIDATION FUNCTION(s)
//------------------------------------------------------------------------------
	protected static void ValidateNotNullArgument(float[] data) {
		if(data == null) {
			throw new System.ArgumentNullException(nameof(data));
		}
    }

	protected static void ValidateNotNullArgument(System.Array data) {
		if(data == null) {
			throw new System.ArgumentNullException(nameof(data));
		}
    }

	protected static void ValidateNotNullArgument(LArray larray) {
		if(larray == null) {
			throw new System.ArgumentNullException(nameof(larray));
		}
	}

	protected void ValidateNotEmpty() {
		if(data.Length == 0) {
			throw new System.ArgumentException($"Data is empty, it contains no values.");
        }
    }

	protected static void ValidateDefinedShape(int[] shape) {
		if(shape == null) {
			throw new System.ArgumentNullException(nameof(shape));
		}

		if(shape.Length < 1) {
				throw new System.ArgumentOutOfRangeException(nameof(shape), shape.Length, "Shape length must be at least 1");
		}

		for(int i = 0; i < shape.Length; i++) {
				if(shape[i] < 0) {
					throw new System.ArgumentException($"Value of shape {shape[i]} at index {i} is negative. Must use non-negative values for shape definition.");
				}
		}
    }

	protected void ValidateEqualRank(int[] shape) {
		if(shape.Length != this.shape.Length) {
			throw new System.ArgumentException($"Shape length {shape.Length} is not equal to expected shape length {this.shape.Length}");
		}
    }

	protected void ValidateEqualRank(LArray larray) {
		if(larray.shape.Length != this.shape.Length) {
			throw new System.ArgumentException($"Other larray shape length {larray.shape.Length} is not equal to expected shape length {this.shape.Length}");
		}
    }

	protected void ValidateEqualShape(LArray larray) {
		int[] shape = larray.GetShape();
		ValidateEqualRank(shape);

		for(int i = 0; i < shape.Length; i++) {
            if (shape[i] != this.shape[i]) {
				throw new System.ArgumentException($"Value of shape {shape[i]} at index {i} does not equal expected value {this.shape[i]}");
			}
        }
    }

	protected void ValidateDataIndex(int index) {
		if (index >= data.Length || index < 0) {
			throw new System.IndexOutOfRangeException($"Index {index} out of bounds for length {this.data.Length}");
		}
	}

	protected void ValidateDataIndex(int index, int shapeindex) {
		if (index >= shape[shapeindex] || index < 0) {
			throw new System.IndexOutOfRangeException($"Index {index} out of bounds for length {this.shape[shapeindex]}");
		}
	}

	protected void ValidateDataIndices(int[] indices) {
		ValidateEqualRank(indices);
		for(int i = 0; i < indices.Length; i++) {
			ValidateDataIndex(indices[i], i);
		}
	}

	protected void ValidateRange(int[,] range) {
		if (range.GetLength(0) != shape.Length) {
			throw new System.ArgumentException($"Rank of range {range.GetLength(0)} does not match the expected rank of shape {shape.Length}.");
		}

		int sum = 1;
		for (int i = 0; i < range.GetLength(0); i++) {
			if (range[i, 0] < 0 || range[i, 1] < 0)
			{
				throw new System.ArgumentOutOfRangeException(nameof(range), $"range {i} contains negative numbers.");
			}

			int length = (range[i, 1] - range[i, 0]) + 1;
			if (length <= 0)
			{
				throw new System.ArgumentOutOfRangeException(nameof(range), $"Dimension length {length} in range {i} is non-positive (<=0).");
			}

			if (length > shape[i])
			{
				throw new System.ArgumentOutOfRangeException(nameof(range), $"Dimension length {length} in range {i} is greater than max dimension length {shape[i]} in shape {i}.");
			}

			sum *= length;
		}

		if (sum > data.Length) {
			throw new System.ArgumentException($"Total range length {sum} is greater than data length {data.Length}.");
		}
	}
}
} // END namespace lmath