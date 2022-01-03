//==============================================================================
// Filename: FFEvoNeuralNetwork.cs
// Author: Aaron Thompson
// Date Created: 8/26/2021
// Last Updated: 9/2/2021
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
    
    public float minWeight;
    public float maxWeight;
    public float minBias;
    public float maxBias;
    public float minWeightMutation;
    public float maxWeightMutation;
    public float minBiasMutation;
    public float maxBiasMutation;
    public float mutationRate;
    //public float minCrossoverPercent;
    //public float maxCrossoverPercent;
    public int crossPoints;
    public int crossOffset;
    public int population;

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
        //StreamWriter writer = new StreamWriter("D:\\Users\\aamit\\Documents\\Programming\\Artificial Intelligence\\AI Deep Learning Lab V3\\Data" + "\\test.txt", true);
        //writer.AutoFlush = true;

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

				GA.Continue(1, parallel);

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

                //string line = "[" + (k*batches[0].Count + i + 1).ToString() + "] Accuracy: " + Statistics.Accuracy(yData, yTest).ToString() + "%";
                //writer.WriteLine(line);
            }


			Debug.Log("[Neural Network] Epoch [" + (k+1).ToString() + "] / [" + epochs.ToString() + "] complete. (Batch Size: " + miniBatchSize.ToString() + ")");
        }

        //writer.Close();

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

            GA.minValueMatrix = Matrix.Zeros(nW + nB, 1);
            GA.maxValueMatrix = Matrix.Zeros(nW + nB, 1);
            GA.minMutationMatrix = Matrix.Zeros(nW + nB, 1);
            GA.maxMutationMatrix = Matrix.Zeros(nW + nB, 1);

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

            GA.mutationRate = mutationRate;
            //GA.minCrossoverPercent = minCrossoverPercent;
            //GA.maxCrossoverPercent = maxCrossoverPercent;
            GA.crossPoints = crossPoints;
            GA.crossOffset = crossOffset;
            GA.populationCount = population;

            GA.Populate();
        }
}
}// END namespace adl
//==============================================================================
//==============================================================================
