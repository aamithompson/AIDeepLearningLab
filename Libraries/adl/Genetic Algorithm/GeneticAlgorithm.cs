//==============================================================================
// Filename: GeneticAlgorithm.cs
// Author: Aaron Thompson
// Date Created: 6/19/2020
// Last Updated: 10/2/2025
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using lmath;
using statistics;

namespace adl {
public class GeneticAlgorithm {
// VARIABLES
//------------------------------------------------------------------------------
	private List<Individual> population;
	private List<Individual> newPopulation;

	//General Settings
	public Distribution distributionType;

	//Population Settings
	public int populationCount;
	public int eliteCount;
	public Matrix minValueMatrix; //[UNIFORM]
	public Matrix maxValueMatrix; //[UNIFORM]
	public Matrix meanValueMatrix; //[GAUSSIAN]
	public Matrix stdDevValueMatrix; //[GAUSSIAN]

	//Fitness Settings
	public System.Func<Matrix, float> fitnessFunction;
	public Vector fitnessProportions;

	//Crossover Settings
	public int crossPoints;
	public int crossOffset;

	//Mutation Settings
	public float mutationRate;
	public Matrix minMutationMatrix; //[UNIFORM] //current value +/- mutation value
	public Matrix maxMutationMatrix; //[UNIFORM] //min and maxes out at value matrices
	public Matrix meanMutationMatrix; //[GAUSSIAN] //current value +/- mutation value
	public Matrix stdDevMutationMatrix; //[GAUSSIAN] //min and maxes out at value matrices

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public GeneticAlgorithm() {
		distributionType = Distribution.Uniform;
		populationCount = 0;
		minValueMatrix = Matrix.Zeros(1, 1);
		maxValueMatrix = Matrix.Zeros(1, 1);
		crossPoints = 1;
		crossOffset = 0;
		mutationRate = 0f;
		minMutationMatrix = Matrix.Zeros(1, 1);
		maxMutationMatrix = Matrix.Zeros(1, 1);
	}

// ALGORITHM
//------------------------------------------------------------------------------
	//GENERATION CONTROL
	public void Run(int k = 1) {
		Populate();
		Continue(k);
	}

	public void Continue(int k = 1) {
		while(k > 0) {
			Fit();
			Crossover();
			Mutate();
			ReplacePopulation();
			k--;
		}

		Fit();
	}
	
	//INITIALIZE POPULATION
	public void Populate() {
		population = new List<Individual>();
		fitnessProportions = Vector.Zeros(populationCount);
		for(int i = 0; i < populationCount; i++) {
			if(distributionType == Distribution.Uniform) {
				population.Add(new Individual(Matrix.Random(minValueMatrix, maxValueMatrix)));
            } else if (distributionType == Distribution.Gaussian) {
				population.Add(new Individual(Matrix.RandomN(meanValueMatrix, stdDevValueMatrix)));
            }
		}
	}
	

	//FITNESS FUNCTION
	public void Fit() {
		EvaluatePopulation();
		population.Sort((x, y) => x.fitnessScore.CompareTo(y.fitnessScore));
		population.Reverse();

		float min = population[populationCount - 1].fitnessScore;
		float shift = (min < 0) ? -min + 0.01f : 0.0f; //0.01f = epsilon

		//Probability Calculation
		float sum = 0.0f;
		for (int i = 0; i < populationCount; i++) {
			sum += population[i].fitnessScore + shift;
			fitnessProportions[i] = sum;
		}
		fitnessProportions *= (1.0f / sum);
	}

	public void EvaluatePopulation(bool currentPopulation=true) {
		for(int i = 0; i < populationCount; i++) {
			EvalutateIndividual(i, currentPopulation);
		}
	}

	private void EvalutateIndividual(int index, bool currentPopulation = true) {
		if (currentPopulation) {
			float score = fitnessFunction(population[index].data);
			Individual individual = new Individual(population[index].data, score);
			population[index] = individual;
		} else {
				float score = fitnessFunction(newPopulation[index].data);
				Individual individual = new Individual(newPopulation[index].data, score);
				newPopulation[index] = individual;
		}
	}

	//SELECTION
	private Individual SelectIndividual() {
		float selection = Statistics.NextFloat();

		for(int i = 0; i < populationCount; i++) {
			if(selection <= fitnessProportions[i]) {
				return population[i];
            }
        }

		return population[populationCount - 1];
    }

	//CROSSOVER
	public void Crossover() {
		newPopulation = new List<Individual>();
		for(int i = 0; i < populationCount - 1; i += 2) {
				Individual parentA = SelectIndividual();
				Individual parentB = SelectIndividual();
				while(parentA == parentB) {
					parentB = SelectIndividual();
                }

				Individual[] offspring = Offspring(parentA, parentB);
				newPopulation.Add(offspring[0]);
				newPopulation.Add(offspring[1]);
		}

		if(populationCount%2 == 1) {
			Individual parentA = SelectIndividual();
			Individual parentB = SelectIndividual();
			while (parentA == parentB) {
				parentB = SelectIndividual();
			}

			newPopulation.Add(Offspring(parentA, parentB)[0]);
		}
	}
	
	private Individual[] Offspring(Individual parentA, Individual parentB, bool parallel=true){
		Individual[] children = new Individual[2];
		Individual childA = new Individual(parentB.data);
		Individual childB = new Individual(parentA.data);

		int n = childA.data.GetLength();
		if(crossPoints == 0) {
			for(int i = 0; i < n; i++) {
				float value = (parallel) ? ParallelRandom.NextFloat() : UnityEngine.Random.value;
				bool flip = (value > 0.5f);
				if(flip){
					childA.data[i] = parentA.data[i];
					childB.data[i] = parentB.data[i];
				}
            }
        } else {
			crossPoints = System.Math.Min(crossPoints, n - 1);
			int[] points = new int[crossPoints];
			int distance = n/crossPoints;
			int a = 0;
			for(int i = 0; i < crossPoints; i++) {
				a += distance;
				points[i] = a;
			}

			bool flip = true;
			int pointIndex = 0;
			for(int i = 0; i < n; i++) {
				if(pointIndex < crossPoints && i >= points[pointIndex]) {
					flip = !flip;
					pointIndex++;
                }

				if(flip) {
					childA.data[(i + crossOffset) % n] = parentA.data[i];
					childB.data[(i + crossOffset) % n] = parentB.data[i];
				}
            }
        }

		children[0] = childA;
		children[1] = childB;

		return children;
	}

	//MUTATION
	public void Mutate() {
		if(newPopulation == null || newPopulation.Count == 0){
			newPopulation = new List<Individual>(population);
		}

		for(int i = 0; i < populationCount; i++) {
			MutateIndividual(i);
        }
	}
	
	private void MutateIndividual(int index) {
		for(int i = 0; i < newPopulation[index].data.GetLength(); i++) {
			float rvalue = Statistics.NextFloat();

			if (rvalue < mutationRate) {
				if(distributionType == Distribution.Uniform) {
					float minValue;
					float maxValue;
					if (minMutationMatrix.Norm() == 0f && maxMutationMatrix.Norm() == 0f)
					{
						minValue = minValueMatrix[i];
						maxValue = maxValueMatrix[i];
					} else {
						minValue = newPopulation[index].data[i] + minMutationMatrix[i];
						if (minValue < minValueMatrix[i])
						{
							minValue = minValueMatrix[i];
						}

						maxValue = newPopulation[index].data[i] + maxMutationMatrix[i];
						if (maxValue > maxValueMatrix[i])
						{
							maxValue = maxValueMatrix[i];
						}
					}

					newPopulation[index].data[i] = Statistics.NextFloat(minValue, maxValue);
				}
			} else if (distributionType == Distribution.Gaussian) {
				newPopulation[index].data[i] += Statistics.randomN(meanMutationMatrix[i], stdDevMutationMatrix[i]);
			}
		}
	}

	//REPLACEMENT
	public void ReplacePopulation() {
		EliteSelection();
		population = new List<Individual>(newPopulation);
    }

	public void EliteSelection() {
		EvaluatePopulation(false);
		newPopulation.Sort((x, y) => x.fitnessScore.CompareTo(y.fitnessScore));
		for(int i = 0; i < eliteCount; i++) {
			newPopulation[i] = population[i];
		}
    }

// OPERATIONS
//------------------------------------------------------------------------------
	public Matrix[] GetPopulation() {
		Matrix[] population = new Matrix[populationCount];
		for(int i = 0; i < populationCount; i++) {
			population[i] = GetIndividual(i);
		}

		return population;
	}

	public Matrix GetIndividual(int i) {
		return new Matrix(population[i].data);
	}

	public Matrix[] GetBestFit(int n=1) {
		Matrix[] candidates = new Matrix[n];

		for(int i = 0; i < n; i++) {
			candidates[i] = GetIndividual(i);
			UnityEngine.MonoBehaviour.print(population[i].fitnessScore);
		}

		return candidates;
	}

	public void Print(int i=-1) {
		if(i == -1) {
			PrintPopulation();
		} else {
			PrintIndividual(i);
		}
	}
	
	private void PrintPopulation() {
		string s = "";
		s += "GENETIC ALGORITHM PRINTOUT";
		s += "\n--------------------------------";
		s += "\nminValueMatrix:\n\t";
		s += minValueMatrix.ToString();
		s += "\nmaxValueMatrix:\n\t";
		s += maxValueMatrix.ToString();
		s += "\ncrossPoints:\t" + crossPoints.ToString();
		s += "\ncrossPoints:\t" + crossOffset.ToString();
		s += "\nminMutationMatrix:\n\t";
		s += minMutationMatrix.ToString();
		s += "\nmaxMutationMatrix:\n\t";
		s += maxMutationMatrix.ToString();
		s += "\n--------------------------------";
		s += "\npopulationCount:\t" + populationCount.ToString() + " total";
		UnityEngine.MonoBehaviour.print(s);
		for(int i = 0; i < populationCount; i++) {
			PrintIndividual(i);
		}
	}

	private void PrintIndividual(int i) {
		string s = "";
		s += "Individual[ " + i.ToString() + " ]:\n\t";
		s += population[i].data.ToString() + ", ";
		s += "F: " + population[i].fitnessScore.ToString() + "\n\t";
		
		UnityEngine.MonoBehaviour.print(s);
	}

// HELPER FUNCTIONS
//------------------------------------------------------------------------------
	private class Individual {
		public Matrix data;
		public float fitnessScore;

		public Individual() { 
			this.data = new Matrix();
		}
		
		public Individual(Matrix data) {
			this.data = new Matrix(data);
			this.fitnessScore = 0;
        }

		public Individual(Matrix data, float fitnessScore) {
			this.data = new Matrix(data);
			this.fitnessScore = fitnessScore;
		}
	}
}
}
//==============================================================================
//==============================================================================