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
using adl.genetics;

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

                Vector bestd = GA.GetBestFit()[0];
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

        Vector best = GA.GetBestFit()[0];
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

    public float NetworkScore(Vector data) {
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

        return n == GA.minValueVector.GetLength();
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
            GA.minValueVector = Vector.Zeros(nW + nB);
            GA.maxValueVector = Vector.Zeros(nW + nB);
            GA.minMutationVector = Vector.Zeros(nW + nB);
            GA.maxMutationVector = Vector.Zeros(nW + nB);
            GA.meanValueVector = new Vector();
            GA.stdDevValueVector = new Vector();
            GA.meanMutationVector = new Vector();
            GA.stdDevMutationVector = new Vector();

            for (int i = 0; i < nW; i++) {
                GA.minValueVector[i] = minWeight;
                GA.maxValueVector[i] = maxWeight;
                GA.minMutationVector[i] = minWeightMutation;
                GA.maxMutationVector[i] = maxWeightMutation;
            }

            for (int i = nB; i < nW + nB; i++) {
                GA.minValueVector[i] = minBias;
                GA.maxValueVector[i] = maxBias;
                GA.minMutationVector[i] = minBiasMutation;
                GA.maxMutationVector[i] = maxBiasMutation;
            }
        } else if(distributionType == Distribution.Gaussian) {
            GA.minValueVector = new Vector();
            GA.maxValueVector = new Vector();
            GA.minMutationVector = new Vector();
            GA.maxMutationVector = new Vector();
            GA.meanValueVector = Vector.Zeros(nW + nB);
            GA.stdDevValueVector = Vector.Zeros(nW + nB);
            GA.meanMutationVector = Vector.Zeros(nW + nB);
            GA.stdDevMutationVector = Vector.Zeros(nW + nB);

            for (int i = 0; i < nW; i++) {
                GA.meanValueVector[i] = meanWeight;
                GA.stdDevValueVector[i] = stdDevWeight;
                GA.meanMutationVector[i] = meanWeightMutation;
                GA.stdDevMutationVector[i] = stdDevWeightMutation;
            }

            for (int i = nW; i < nW + nB; i++) {
                GA.meanValueVector[i] = meanBias;
                GA.stdDevValueVector[i] = stdDevBias;
                GA.meanMutationVector[i] = meanBiasMutation;
                GA.stdDevMutationVector[i] = stdDevBiasMutation;
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
