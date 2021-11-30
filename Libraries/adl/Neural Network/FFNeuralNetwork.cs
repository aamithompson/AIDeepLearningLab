//==============================================================================
// Filename: FFNeuralNetwork.cs
// Author: Aaron Thompson
// Date Created: 12/9/2020
// Last Updated: 11/16/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;

namespace adl {
public class FFNeuralNetwork {
// VARIABLES
//------------------------------------------------------------------------------
	public List<List<Neuron>> network;
	public List<Matrix<float>> weights;
	public List<Vector<float>> biases;

	public int depth { get { return network.Count; } }


// CONSTRUCTORS
//------------------------------------------------------------------------------
	public FFNeuralNetwork() {
		network = new List<List<Neuron>>();
		weights = new List<Matrix<float>>();
		biases = new List<Vector<float>>();
	
		//Input and output layers default is identity layer with 1 unit each.
		AddLayer(ActivationFunctions.Identity);
		AddLayer(ActivationFunctions.Identity);
	}

	public FFNeuralNetwork(int sizeX=1, int sizeY=1) {
		network = new List<List<Neuron>>();
		weights = new List<Matrix<float>>();
		biases = new List<Vector<float>>();

		AddLayer(ActivationFunctions.Identity,-1, 1, sizeX);
		AddLayer(ActivationFunctions.Identity,-1, 1, sizeY);
    }

	public FFNeuralNetwork(Operation op, int sizeX = 1, int sizeY = 1) {
			network = new List<List<Neuron>>();
			weights = new List<Matrix<float>>();
			biases = new List<Vector<float>>();

			AddLayer(ActivationFunctions.Identity, -1, 1, sizeX);
			AddLayer(op, -1, 1, sizeY);
	}

		// DATA MANAGEMENT
		//------------------------------------------------------------------------------
		public void AddLayer(Operation op, int i=-1, int n=1, int width=1) {
		if(i == -1) {
			if(depth < 2) {
				network.Add(new List<Neuron>());
				i = depth - 1;
			} else {
				i = depth - 1;
				AddLayer(op, i, n, width);
				return;
			}

			if(depth > 1) {
				//Matrix<float> min = Matrix<float>.Ones(network[i - 1].Count, 1) * -1;
				//Matrix<float> max = Matrix<float>.Ones(network[i - 1].Count, 1);
				//weights.Add(Matrix<float>.Random(min, max));
				weights.Add(Matrix<float>.RandomN(0, 1, network[i - 1].Count, 1));
				//biases.Add(Vector<float>.Zeros(1));
				biases.Add(Vector<float>.RandomN(0, 1, 1));
			}
			
		} else if(i > 0 && i < depth) {
			network.Insert(i, new List<Neuron>());
			//Matrix<float> min = Matrix<float>.Ones(1, network[i + 1].Count) * - 1;
			//Matrix<float> max = Matrix<float>.Ones(1, network[i + 1].Count);
			//weights.Insert(i, Matrix<float>.Random(min, max));
			weights.Insert(i, Matrix<float>.RandomN(0, 1, 1, network[i + 1].Count));
			weights[i-1] = Matrix<float>.RandomN(0, 1, network[i - 1].Count, 1);
			//biases.Insert(i-1, Vector<float>.Zeros(1));
			biases.Insert(i-1, Vector<float>.RandomN(0, 1, 1));
			//weights[i-1] = Matrix<float>.Ones(network[i-1].Count, 1);
		} else {
			return;
		}

		network[i].Add(new Neuron(op));
		AddUnit(i, width-1);

		/*Debug.Log("[Network Depth]: " + depth);
		for(int j = 0; j < weights.Count; j++) {
			weights[j].Print();
        }*/
		
		if(n > 1) {
			AddLayer(op, i, n-1, width);
		}
	}

	public void AddLayer(System.Func<float, float> f, int i = -1, int n = 1, int width = 1) {
		Operation op = new Operation(f);
		AddLayer(op, i, n, width);
	}
	
	//Does not allow for the removal of the input OR output layer
	public void RemoveLayer(int i=-1) {
		if(i == -1) {
			i = depth - 2;
		}

		if(i <= 0 || i >= depth - 1) {
			return;
		}

		network.RemoveAt(i);
	}

	public void AddUnit(int i, int n=1) {
		if(n < 1) {
			return;
        }

		//Network
		Operation op = network[i][0].op;
		for(int j = 0; j < n; j++) {
			network[i].Add(new Neuron(op));
		}

		//Weights
		if(i > 0) {
			int[] shape = weights[i-1].GetShape();
			//Matrix<float> min = Matrix<float>.Ones(shape[0], shape[1] + n) * -1;
			//Matrix<float> max = Matrix<float>.Ones(shape[0], shape[1] + n);
			//Matrix<float> newWeights = Matrix<float>.Random(min, max);
			Matrix<float> newWeights = Matrix<float>.RandomN(0, 1, shape[0], shape[1] + n);
			newWeights.SetSlice(weights[i-1], new int[,] { {0, shape[0] - 1}, {0, shape[1] - 1} });
			weights[i-1].Copy(newWeights);
		}

		if(i < depth - 1) {
			int[] shape = weights[i].GetShape();
			//Matrix<float> min = Matrix<float>.Ones(shape[0] + n, shape[1]) * -1;
			//Matrix<float> max = Matrix<float>.Ones(shape[0] + n, shape[1]);
			//Matrix<float> newWeights = Matrix<float>.Random(min, max);
			Matrix<float> newWeights = Matrix<float>.RandomN(0, 1, shape[0] + n, shape[1]);
			newWeights.SetSlice(weights[i], new int[,] { {0, shape[0] - 1}, {0, shape[1] - 1} });
			weights[i].Copy(newWeights);
		}

		//Biases
		if(i > 0) {
			//biases[i-1].Reshape(network[i].Count);
			biases[i-1] = Vector<float>.RandomN(0, 1, network[i].Count);
        }
	}

	//Will not remove a unit if layer width is less than or equal to 1
	public void RemoveUnit(int i, int n=1) {
		//Network
		for(int j = 0; j < n; j++) {
			if(network[i].Count <= 1) {
				break;
			}

			network[i].RemoveAt(network[i].Count - 1);
		}

		//Weights
		if(i > 0) {
			int[] shape = weights[i-1].GetShape();
			weights[i-1].Reshape(shape[0], shape[1] + n);
		}

		if(i < depth - 1) {
			int[] shape = weights[i].GetShape();
			weights[i].Reshape(shape[0] + n, shape[1]);
		}

		//Biases
		biases[i-1].Reshape(biases[i-1].length - n);
	}

	public void SetLayerWidth(int i, int n) {
		if(n <= 1) {
			return;
		}

		if(network[i].Count > n) {
			while(network[i].Count > n) {
				RemoveUnit(i);
			}
		} else if(network[i].Count < n) {
			while(network[i].Count < n) {
				AddUnit(i);
			}
		}
	}

	public int GetLayerWidth(int i) {
			return network[i].Count;
	}
	
	//Does not allow for the input layer's function to be changed
	public void SetLayerFunction(int i, Operation op) {
		int n = network[i].Count;
		for(int j = 0; j < n; j++) {
			network[i][j].op = op;
		}
	}

	public void SetLayerFunction(int i, System.Func<float, float> f) {
		Operation op = new Operation(f);
		SetLayerFunction(i, op);
    }

	public void SetWeight(int i, int x, int y, float weight) {
		weights[i][x, y] = weight;
	}

	public float GetWeight(int i, int x, int y) {
		return weights[i][x, y];
    }

	public Matrix<float> GetWeightMatrix(int i) {
		return new Matrix<float>(weights[i]);
    }

	public void SetBias(int i, int j, float bias) {
		biases[i][j] = bias;
    }

	public float GetBias(int i, int j) {
		return biases[i][j];
    }

	public Vector<float> GetBiasVector(int i) {
		return new Vector<float>(biases[i]);
    }

// ACTIVATION
//------------------------------------------------------------------------------
	public Vector<float> Activate(Vector<float> input) {
		Vector<float> v = new Vector<float>(input);

		for(int i = 0; i < depth - 1; i++) {
			Vector<float> b = new Vector<float>(biases[i]);
			v = Matrix<float>.MatVecMul(Matrix<float>.Transpose(weights[i]), v) + b;

			v.Operation(network[i+1][0].op.f);
		}
		
		return v;
	}

	public List<Matrix<float>>[] Backprop(Vector<float> x, Vector<float> y) {
		List<Matrix<float>> gradWeights = new List<Matrix<float>>();
		List<Matrix<float>> gradBiases = new List<Matrix<float>>();

		List<Vector<float>> activation = new List<Vector<float>>();
		List<Vector<float>> z = new List<Vector<float>>();
		activation.Add(x);
		for (int i = 0; i < depth - 1; i++) {
			z.Add(Matrix<float>.MatVecMul(Matrix<float>.Transpose(weights[i]), activation[i]) + biases[i]);
			activation.Add(new Vector<float>(z[i]));
			activation[i + 1].Operation(network[i + 1][0].op.f);
		}

		//delta_L
		Vector<float> costDerivative = (activation[activation.Count - 1] - y);
		Vector<float> df = new Vector<float>(z[z.Count - 1]);
		df.Operation(network[network.Count - 1][0].op.df);
		Vector<float> delta = Vector<float>.HadamardProduct(costDerivative, df);

		Matrix<float> A = new Matrix<float>(activation[activation.Count-2].GetData(), activation[activation.Count-2].length, 1);
		Matrix<float> B = new Matrix<float>(delta.GetData(), 1, delta.length);
		gradWeights.Insert(0, Matrix<float>.MatMul(A, B));
		gradBiases.Insert(0, new Matrix<float>(B));

		//delta_l
		for(int i = 2; i < depth; i++) {
			df = new Vector<float>(z[z.Count - i]);
			df.Operation(network[network.Count - i][0].op.df);
			delta = Matrix<float>.MatVecMul(weights[weights.Count - i + 1], delta);
			delta = Vector<float>.HadamardProduct(delta, df);
			A = new Matrix<float>(activation[activation.Count - i - 1].GetData(), activation[activation.Count - i - 1].length, 1);
			B = new Matrix<float>(delta.GetData(), 1, delta.length);
			gradWeights.Insert(0, Matrix<float>.MatMul(A, B));
			gradBiases.Insert(0, new Matrix<float>(B));
        }

		for(int i = 0; i < gradWeights.Count; i++) {
				//Debug.Log(weights[i].GetShape()[0].ToString() + ", " + weights[i].GetShape()[1].ToString());
				//Debug.Log(gradWeights[i].GetShape()[0].ToString() + ", " + gradWeights[i].GetShape()[1].ToString());
        }
		

		List<Matrix<float>>[] grad = new List<Matrix<float>>[2];
		grad[0] = gradWeights;
		grad[1] = gradBiases;
		return grad;
    }

	//Stochastic Gradient Descent (Training via Minibatches)
	public void SGD(List<Vector<float>> x, List<Vector<float>> y, int epochs=1, int miniBatchSize=10, float learningRate=1.00f) {
		DataSet dataSet = new DataSet(x, y);

		for(int k = 0; k < epochs; k++) {
			List<List<Data>> batches = dataSet.GetEpochBatchSet(miniBatchSize);

			for(int i = 0; i < batches.Count; i++) {
				List<Vector<float>> xBatch = new List<Vector<float>>();
				List<Vector<float>> yBatch = new List<Vector<float>>();
				for(int j = 0; j < batches[i].Count; j++) {
					xBatch.Add(batches[i][j].x);
					yBatch.Add(batches[i][j].y);
                }

				TrainMiniBatch(xBatch, yBatch, learningRate);
            }

			Debug.Log("[Neural Network] Epoch [" + (k+1).ToString() + "] / [" + epochs.ToString() + "] complete. (Batch Size: " + miniBatchSize.ToString() + ", Learning Rate: " + learningRate.ToString() + ")");
        }
    }

	public void TrainMiniBatch(List<Vector<float>> x, List<Vector<float>> y, float learningRate=1.00f) {
		List<Matrix<float>> gradWeights = new List<Matrix<float>>();
		List<Vector<float>> gradBiases = new List<Vector<float>>();
		for(int i = 0; i < weights.Count; i++) {
			gradWeights.Add(Matrix<float>.Zeros(weights[i].GetShape()[0], weights[i].GetShape()[1]));
			gradBiases.Add(Vector<float>.Zeros(biases[i].length));
        }

		//Backpropagation
		int n = Mathf.Min(x.Count, y.Count);
		for(int i = 0; i < n; i++) {
			List<Matrix<float>>[] grad = Backprop(x[i], y[i]);
			List<Matrix<float>> deltaGradWeights = grad[0];
			List<Vector<float>> deltaGradBiases = new List<Vector<float>>();
			for(int j = 0; j < biases.Count; j++) {
				deltaGradBiases.Add(new Vector<float>(grad[1][j].GetData()));
			}

			for(int j = 0; j < gradWeights.Count; j++) {
				gradWeights[j] += deltaGradWeights[j];
				gradBiases[j] += deltaGradBiases[j];
            }
        }

		//Updating Weights and Biases
		for(int i = 0; i < weights.Count; i++) {
			weights[i] -= (learningRate/n) * gradWeights[i];
			biases[i] -= (learningRate/n) * gradBiases[i];
		}
    }

// PERCEPTRON/NEURON/UNIT
//------------------------------------------------------------------------------
	public class Neuron {
			public Operation op;
			public float value;

			public Neuron(Operation op) {
				this.op = op;
				this.value = 0;
			}

			public void Activate(float input) {
				value = op.f(input);
            }

			public float Output(float input) {
				Activate(input);
				return value;
			}

			public float Derivative(float input) {
				return op.df(input);
            }
	}
}
}// END namespace adl
//==============================================================================
//==============================================================================