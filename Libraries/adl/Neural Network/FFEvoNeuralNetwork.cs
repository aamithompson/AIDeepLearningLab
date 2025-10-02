//==============================================================================
// Filename: FFEvoNeuralNetwork.cs
// Author: Aaron Thompson
// Date Created: 8/26/2021
// Last Updated: 1/12/2022
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using lmath;
using statistics;

namespace adl {
public class FFEvoNeuralNetwork {
    public FFNeuralNetwork NN;
    public GeneticAlgorithm GA;
    private List<Vector> xBatch;
    private List<Vector> yBatch;
    public List<Vector> xTest;
    public List<Vector> yTest;
    
    //Weight Value Settings
    public float minWeight; //[UNIFORM]
    public float maxWeight; //[UNIFORM]
    public float meanWeight; //[GAUSSIAN]
    public float stdDevWeight; //[GAUSSIAN]
    
    //Bias Value Settings
    public float minBias; //[UNIFORM]
    public float maxBias; //[UNIFORM]
    public float meanBias; //[GAUSSIAN]
    public float stdDevBias; //[GAUSSIAN]

    //Mutation Weight Settings
    public float minWeightMutation; //[UNIFORM]
    public float maxWeightMutation; //[UNIFORM]
    public float meanWeightMutation; //[GAUSSIAN]
    public float stdDevWeightMutation; //[GAUSSIAN]

    //Mutation Bias Settings
    public float minBiasMutation; //[UNIFORM]
    public float maxBiasMutation; //[UNIFORM]
    public float meanBiasMutation; //[GAUSSIAN]
    public float stdDevBiasMutation; //[GAUSSIAN]

    public float mutationRate;
    public int crossPoints;
    public int crossOffset;
    public int population;
    public Distribution distributionType;

    public FFEvoNeuralNetwork() {
        NN = new FFNeuralNetwork();
        GA = new GeneticAlgorithm();
        GA.fitnessFunction = NetworkScore;
        UpdateGA();
    }

    public FFEvoNeuralNetwork(int sizeX, int sizeY) {
        NN = new FFNeuralNetwork(sizeX, sizeY);
        GA = new GeneticAlgorithm();
        GA.fitnessFunction = NetworkScore;
        UpdateGA();
    }

    
    public FFEvoNeuralNetwork(Operation op, int sizeX, int sizeY) {
        NN = new FFNeuralNetwork(op, sizeX, sizeY);
        GA = new GeneticAlgorithm();
        GA.fitnessFunction = NetworkScore;
        UpdateGA();
    }

    public Vector Activate(Vector input) {
        return NN.Activate(input);
    }

    public void SGD(List<Vector> x, List<Vector> y, int epochs=1, int miniBatchSize=10, bool parallel=true) {
        DataSet dataSet = new DataSet(x, y);

        if (!GASizeCorrect()) {
            UpdateGA();
        }

        for(int k = 0; k < epochs; k++) {
			List<List<Data>> batches = dataSet.GetEpochBatchSet(miniBatchSize);

			for(int i = 0; i < batches.Count; i++) {
				xBatch = new List<Vector>();
				yBatch = new List<Vector>();
				for(int j = 0; j < batches[i].Count; j++) {
					xBatch.Add(batches[i][j].x);
					yBatch.Add(batches[i][j].y);
                }

				GA.Continue(1);

                Matrix bestd = GA.GetBestFit()[0];
                int indexd = 0;
                for(int l = 0; l < NN.weights.Count; l++) {
                    for(int j = 0; j < NN.weights[l].GetLength(); j++) {
                        NN.weights[l][j] = bestd[indexd];
                        indexd++;
                    }

                    for(int j = 0; j < NN.biases[l].GetLength(); j++) {
                        NN.biases[l][j] = bestd[indexd];
                        indexd++;
                    }
                }

                List<Vector> yData = new List<Vector>();
                for (int j = 0; j < yTest.Count; j++) {
                    yData.Add(NN.Activate(xTest[j]));
                }
            }


			Debug.Log("[Neural Network] Epoch [" + (k+1).ToString() + "] / [" + epochs.ToString() + "] complete. (Batch Size: " + miniBatchSize.ToString() + ")");
        }

        Matrix best = GA.GetBestFit()[0];
        int index = 0;
        for(int i = 0; i < NN.weights.Count; i++) {
            for(int j = 0; j < NN.weights[i].GetLength(); j++) {
                NN.weights[i][j] = best[index];
                index++;
            }

            for(int j = 0; j < NN.biases[i].GetLength(); j++) {
                NN.biases[i][j] = best[index];
                index++;
            }
        }
    }

    public float NetworkScore(Matrix data) {
        float score = 0;

        int index = 0;
        for(int i = 0; i < NN.weights.Count; i++) {
            for(int j = 0; j < NN.weights[i].GetLength(); j++) {
                NN.weights[i][j] = data[index];
                index++;
            }

            for(int j = 0; j < NN.biases[i].GetLength(); j++) {
                NN.biases[i][j] = data[index];
                index++;
            }
        }

        for(int i = 0; i < xBatch.Count; i++) {
            Vector y = NN.Activate(xBatch[i]);
            Vector y_actual = new Vector(yBatch[i]);
            float MSE = 0;
            for(int j = 0; j < y.length; j++) {
                MSE += Mathf.Pow((y_actual[j] - y[j]), 2);
            }
            MSE = MSE / (y.length);
            score += MSE;
        }

        return 1/(1 + score);
    }

    public bool GASizeCorrect() {
        int n = 0;

        for(int i = 0; i < NN.weights.Count; i++) {
            n += NN.weights[i].GetLength();
            n += NN.biases[i].GetLength();
        }

        return n == GA.minValueMatrix.GetLength();
    }

    public void UpdateGA() {
        int nW = 0;
        int nB = 0;

        for (int i = 0; i < NN.weights.Count; i++) {
            nW += NN.weights[i].GetLength();
            nB += NN.biases[i].GetLength();
        }

        GA.distributionType = distributionType;
        if(distributionType == Distribution.Uniform) {
            GA.minValueMatrix = Matrix.Zeros(nW + nB, 1);
            GA.maxValueMatrix = Matrix.Zeros(nW + nB, 1);
            GA.minMutationMatrix = Matrix.Zeros(nW + nB, 1);
            GA.maxMutationMatrix = Matrix.Zeros(nW + nB, 1);
            GA.meanValueMatrix = new Matrix();
            GA.stdDevValueMatrix = new Matrix();
            GA.meanMutationMatrix = new Matrix();
            GA.stdDevMutationMatrix = new Matrix();

            for (int i = 0; i < nW; i++) {
                GA.minValueMatrix[i] = minWeight;
                GA.maxValueMatrix[i] = maxWeight;
                GA.minMutationMatrix[i] = minWeightMutation;
                GA.maxMutationMatrix[i] = maxWeightMutation;
            }

            for (int i = nB; i < nW + nB; i++) {
                GA.minValueMatrix[i] = minBias;
                GA.maxValueMatrix[i] = maxBias;
                GA.minMutationMatrix[i] = minBiasMutation;
                GA.maxMutationMatrix[i] = maxBiasMutation;
            }
        } else if(distributionType == Distribution.Gaussian) {
            GA.minValueMatrix = new Matrix();
            GA.maxValueMatrix = new Matrix();
            GA.minMutationMatrix = new Matrix();
            GA.maxMutationMatrix = new Matrix();
            GA.meanValueMatrix = Matrix.Zeros(nW + nB, 1);
            GA.stdDevValueMatrix = Matrix.Zeros(nW + nB, 1);
            GA.meanMutationMatrix = Matrix.Zeros(nW + nB, 1);
            GA.stdDevMutationMatrix = Matrix.Zeros(nW + nB, 1);

            for (int i = 0; i < nW; i++) {
                GA.meanValueMatrix[i] = meanWeight;
                GA.stdDevValueMatrix[i] = stdDevWeight;
                GA.meanMutationMatrix[i] = meanWeightMutation;
                GA.stdDevMutationMatrix[i] = stdDevWeightMutation;
            }

            for (int i = nW; i < nW + nB; i++) {
                GA.meanValueMatrix[i] = meanBias;
                GA.stdDevValueMatrix[i] = stdDevBias;
                GA.meanMutationMatrix[i] = meanBiasMutation;
                GA.stdDevMutationMatrix[i] = stdDevBiasMutation;
            }
        }

        GA.mutationRate = mutationRate;
        GA.crossPoints = crossPoints;
        GA.crossOffset = crossOffset;
        GA.populationCount = population;

        GA.Populate();
    }
}
}// END namespace adl
//==============================================================================
//==============================================================================
