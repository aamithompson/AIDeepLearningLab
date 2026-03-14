//==============================================================================
// Filename: GeneticAlgorithm.cs
// Author: Aaron Thompson
// Date Created: 6/19/2020
// Last Updated: 3/14/2026
//
// Description: An implementation of a genetic algorithm class which can be
// configured and uses vectors from the lmath library for representation of
// the data. Designed to take multiple individuals with the possibility of
// running multiple generations with an assigned fitness function. Can be ran
// through each step or generation or ran through multiple generation at a time.
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using lmath;
using statistics;

namespace adl.genetics {
public class GeneticAlgorithm {
	public static int MAX_ATTEMPTS = 16;
// VARIABLES
//------------------------------------------------------------------------------
	private List<Individual> population;
	private List<Individual> newPopulation;

	//General Settings
	public Distribution distributionType;

	//Population Settings
	public int populationCount;
	public int eliteCount;
	public Vector minValueVector; //[UNIFORM]
	public Vector maxValueVector; //[UNIFORM]
	public Vector meanValueVector; //[GAUSSIAN]
	public Vector stdDevValueVector; //[GAUSSIAN]

	//Fitness Settings
	public System.Func<Vector, float> fitnessFunction;
	public Vector fitnessProportions;

	//Crossover Settings
	public int crossPoints;
	public int crossOffset;

	//Mutation Settings
	public float mutationRate;
	public Vector minMutationVector; //[UNIFORM] //current value +/- mutation value
	public Vector maxMutationVector; //[UNIFORM] //min and maxes out at value vectors
	public Vector meanMutationVector; //[GAUSSIAN] //current value +/- mutation value
	public Vector stdDevMutationVector; //[GAUSSIAN] //min and maxes out at value vectors

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public GeneticAlgorithm() {
		distributionType = Distribution.Uniform;
		populationCount = 0;
		minValueVector = Vector.Zeros(1);
		maxValueVector = Vector.Zeros(1);
		crossPoints = 1;
		crossOffset = 0;
		mutationRate = 0f;
		minMutationVector = Vector.Zeros(1);
		maxMutationVector = Vector.Zeros(1);
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
				population.Add(new Individual(Vector.Random(minValueVector, maxValueVector)));
            } else if (distributionType == Distribution.Gaussian) {
				population.Add(new Individual(Vector.RandomN(meanValueVector, stdDevValueVector)));
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
				int attempts = 0;
				while(parentA == parentB && attempts < MAX_ATTEMPTS) {
					parentB = SelectIndividual();
					attempts++;
                }

				Individual[] offspring = Offspring(parentA, parentB);
				newPopulation.Add(offspring[0]);
				newPopulation.Add(offspring[1]);
		}

		if(populationCount%2 == 1) {
			Individual parentA = SelectIndividual();
			Individual parentB = SelectIndividual();
			int attempts = 0;
			while (parentA == parentB && attempts < MAX_ATTEMPTS) {
				parentB = SelectIndividual();
				attempts++;
			}

			newPopulation.Add(Offspring(parentA, parentB)[0]);
		}
	}
	
	private Individual[] Offspring(Individual parentA, Individual parentB, bool parallel=true){
		Individual[] children = new Individual[2];
		Individual childA = new Individual(parentB.data);
		Individual childB = new Individual(parentA.data);

		int n = childA.data.GetLength();
		if(crossPoints == 0 || n <= 1) {
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
					if (minMutationVector.Norm() == 0f && maxMutationVector.Norm() == 0f) {
						minValue = minValueVector[i];
						maxValue = maxValueVector[i];
					} else {
						minValue = newPopulation[index].data[i] + minMutationVector[i];
						if (minValue < minValueVector[i]) {
							minValue = minValueVector[i];
						}

						maxValue = newPopulation[index].data[i] + maxMutationVector[i];
						if (maxValue > maxValueVector[i]) {
							maxValue = maxValueVector[i];
						}
					}

					newPopulation[index].data[i] = Statistics.NextFloat(minValue, maxValue);
				}
			} else if (distributionType == Distribution.Gaussian) {
				newPopulation[index].data[i] += Statistics.randomN(meanMutationVector[i], stdDevMutationVector[i]);
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
	public Vector[] GetPopulation() {
		Vector[] population = new Vector[populationCount];
		for(int i = 0; i < populationCount; i++) {
			population[i] = GetIndividual(i);
		}

		return population;
	}

	public Vector GetIndividual(int i) {
		return new Vector(population[i].data);
	}

	public Vector[] GetBestFit(int n=1) {
		Vector[] candidates = new Vector[n];

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
		s += "\nminValueVector:\n\t";
		s += minValueVector.ToString();
		s += "\nmaxValueVector:\n\t";
		s += maxValueVector.ToString();
		s += "\ncrossPoints:\t" + crossPoints.ToString();
		s += "\ncrossPoints:\t" + crossOffset.ToString();
		s += "\nminMutationVector:\n\t";
		s += minMutationVector.ToString();
		s += "\nmaxMutationVector:\n\t";
		s += maxMutationVector.ToString();
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
		public Vector data;
		public float fitnessScore;

		public Individual() { 
			this.data = new Vector();
		}
		
		public Individual(Vector data) {
			this.data = new Vector(data);
			this.fitnessScore = 0;
        }

		public Individual(Vector data, float fitnessScore) {
			this.data = new Vector(data);
			this.fitnessScore = fitnessScore;
		}
	}
}
} //END namespace adl.genetics
//==============================================================================
//==============================================================================