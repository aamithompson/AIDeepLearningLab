//==============================================================================
// Filename: GeneticAlgorithm.cs
// Author: Aaron Thompson
// Date Created: 6/19/2020
// Last Updated: 8/4/2020
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;

namespace adl {
public class GeneticAlgorithm {
// VARIABLES
//------------------------------------------------------------------------------
	private List<Individual> population;

	public int populationCount;
	public Matrix<float> minValueMatrix;
	public Matrix<float> maxValueMatrix;

	public System.Func<Matrix<float>, float> fitnessFunction;

	public float minCrossoverPercent;
	public float maxCrossoverPercent;

	public float mutationRate;
	public Matrix<float> minMutationMatrix; //current value +/- mutation value
	public Matrix<float> maxMutationMatrix; //min and maxes out at value matrices
	

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public GeneticAlgorithm() {
		populationCount = 0;
		minValueMatrix = Matrix<float>.Zeros(1, 1);
		maxValueMatrix = Matrix<float>.Zeros(1, 1);
		minCrossoverPercent = 0f;
		maxCrossoverPercent = 0f;
		mutationRate = 0f;
		minMutationMatrix = Matrix<float>.Zeros(1, 1);
		maxMutationMatrix = Matrix<float>.Zeros(1, 1);
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
			EvaluatePopulation();
			k--;
		}
	}
	
	//INITIALIZE POPULATION
	public void Populate() {
		population = new List<Individual>();
		for(int i = 0; i < populationCount; i++) {
			Individual individual = new Individual();
			individual.data = Matrix<float>.Random(minValueMatrix, maxValueMatrix);
			individual.fitnessScore = 0.0f;
			population.Add(individual);
		}

		EvaluatePopulation();
	}

	//FITNESS FUNCTION
	public void Fit() {
		for(int i = 0; i < populationCount; i++) {
			EvalutateIndividual(i);

			if(i > 0) {
				Individual individual = new Individual();
				individual.data = population[i].data;
				individual.fitnessScore = population[i].fitnessScore + population[i - 1].fitnessScore;
				population[i] = individual;
			}
		}

		List<Individual> fitPopulation = new List<Individual>();
		float minValue = 0;
		float maxValue = population[populationCount - 1].fitnessScore;
		for(int i = 0; i < populationCount; i++) {
			float score = UnityEngine.Random.Range(minValue, maxValue);
			for(int j = 0; j < populationCount; j++) {
				if(score <= population[j].fitnessScore) {
					fitPopulation.Add(population[j]);
					break;
				}
			}
		}

		population = fitPopulation;
	}

	public void EvaluatePopulation() {
		for(int i = 0; i < populationCount; i++) {
			EvalutateIndividual(i);
		}

		population.Sort((x, y) => x.fitnessScore.CompareTo(y.fitnessScore));
		population.Reverse();
	}

	private void EvalutateIndividual(int index) {
		float score = fitnessFunction(population[index].data);
		Individual individual = new Individual();
		individual.data = population[index].data;
		individual.fitnessScore = score;
		population[index] = individual;
	}

	//CROSSOVER
	public void Crossover() {
		for(int i = 0; i < populationCount - 1; i += 2) {
			Individual[] offspring = Offspring(population[i], population[i + 1]);
			population[i] = offspring[0];
			population[i + 1] = offspring[1];
		}
	}
	
	//TODO : change return type to individual type
	private Individual[] Offspring(Individual parentA, Individual parentB){
		Individual[] children = new Individual[2];
		Individual childA = new Individual();
		Individual childB = new Individual();
		Matrix<float> dataA = new Matrix<float>(parentB.data);
		Matrix<float> dataB = new Matrix<float>(parentA.data);
		float percent = UnityEngine.Random.Range(minCrossoverPercent, maxCrossoverPercent);
		int crosspt = (int) (parentA.data.GetLength() * percent);

		for(int i = 0; i < crosspt; i++) {
			dataA[i] = parentA.data[i];	
			dataB[i] = parentB.data[i];	
		}

		childA.data = dataA;
		childA.fitnessScore = 0;
		childB.data = dataB;
		childB.fitnessScore = 0;
		children[0] = childA;
		children[1] = childB;

		return children;
	}

	//MUTATION
	public void Mutate() {
		for(int i = 0; i < populationCount; i++) {
			population[i] = MutateIndividual(population[i]);
		}
	}
	
	//TODO : change return type to individual type
	//TODO : implement mutation min/max offset matrices
	private Individual MutateIndividual(Individual individual) {
		Individual mutIndividual = new Individual();
		Matrix<float> data = new Matrix<float>(individual.data);
		
		for(int i = 0; i < data.GetLength(); i++) {
			float rvalue = UnityEngine.Random.value;
			
			if(rvalue < mutationRate) {
					float minValue;
					float maxValue;
					if (minMutationMatrix.Norm() == 0f && maxMutationMatrix.Norm() == 0f) {
						minValue = minValueMatrix[i];
						maxValue = maxValueMatrix[i];
					} else {
						minValue = data[i] + minMutationMatrix[i];
						if (minValue < minValueMatrix[i]) {
							minValue = minValueMatrix[i];
						}
					
						maxValue = data[i] + maxMutationMatrix[i];
						if (maxValue > maxValueMatrix[i]) {
							maxValue = maxValueMatrix[i];
						}
				}

				data[i] = UnityEngine.Random.Range(minValue, maxValue);
			}
		}

		mutIndividual.data = data;
		mutIndividual.fitnessScore = 0;
		return mutIndividual;
	}

// OPERATIONS
//------------------------------------------------------------------------------
	public Matrix<float>[] GetPopulation() {
		Matrix<float>[] population = new Matrix<float>[populationCount];
		for(int i = 0; i < populationCount; i++) {
			population[i] = GetIndividual(i);
		}

		return population;
	}

	public Matrix<float> GetIndividual(int i) {
		return new Matrix<float>(population[i].data);
	}

	public Matrix<float>[] GetBestFit(int n=1) {
		Matrix<float>[] candidates = new Matrix<float>[n];

		for(int i = 0; i < n; i++) {
			candidates[i] = GetIndividual(i);
			MonoBehaviour.print(population[i].fitnessScore);
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
		s += "GENETIC ALGORTIH PRINTOUT";
		s += "\n--------------------------------";
		s += "\nminValueMatrix:\n\t";
		s += minValueMatrix.ToString();
		s += "\nmaxValueMatrix:\n\t";
		s += maxValueMatrix.ToString();
		s += "\nminCrossoverPercent:\t" + (minCrossoverPercent * 200).ToString() + "%";
		s += "\nmaxCrossoverPercent:\t" + (maxCrossoverPercent * 100).ToString() + "%";
		s += "\nminMutationMatrix:\n\t";
		s += minMutationMatrix.ToString();
		s += "\nmaxMutationMatrix:\n\t";
		s += maxMutationMatrix.ToString();
		s += "\n--------------------------------";
		s += "\npopulationCount:\t" + populationCount.ToString() + " total";
		MonoBehaviour.print(s);
		for(int i = 0; i < populationCount; i++) {
			PrintIndividual(i);
		}
	}

	private void PrintIndividual(int i) {
		string s = "";
		s += "Individual[ " + i.ToString() + " ]:\n\t";
		s += population[i].data.ToString() + ", ";
		s += "F: " + population[i].fitnessScore.ToString() + "\n\t";
		
		MonoBehaviour.print(s);
	}

// HELPER FUNCTIONS
//------------------------------------------------------------------------------
	private struct Individual {
		public Matrix<float> data;
		public float fitnessScore;
	}
}
}
//==============================================================================
//==============================================================================