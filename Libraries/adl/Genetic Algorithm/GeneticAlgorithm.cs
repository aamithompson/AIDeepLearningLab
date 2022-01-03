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
using System.Threading.Tasks;
using lmath;
using ncomp;

namespace adl {
public class GeneticAlgorithm {
// VARIABLES
//------------------------------------------------------------------------------
	private List<Individual> population;

	public int populationCount;
	public Matrix minValueMatrix;
	public Matrix maxValueMatrix;
	//TODO implement ENUM for guassian vs uniform
	public Matrix meanValueMatrix;
	public Matrix stdDevValueMatrix;

	public System.Func<Matrix, float> fitnessFunction;
	public Vector fitnessProportions;

	public int crossPoints;
	public int crossOffset;

	public float mutationRate;
	public Matrix minMutationMatrix; //current value +/- mutation value
	public Matrix maxMutationMatrix; //min and maxes out at value matrices
	

// CONSTRUCTORS
//------------------------------------------------------------------------------
	public GeneticAlgorithm() {
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
	public void Run(int k = 1, bool parallel=true) {
		Populate();
		Continue(k, parallel);
	}

	public void Continue(int k = 1, bool parallel=true) {
		while(k > 0) {
			Fit(parallel);
			Crossover(parallel);
			Mutate(parallel);
			k--;
		}

		EvaluatePopulation(parallel);
	}
	
	//INITIALIZE POPULATION
	public void Populate() {
		population = new List<Individual>();
		fitnessProportions = Vector.Zeros(populationCount);
		for(int i = 0; i < populationCount; i++) {
			Individual individual = new Individual(Matrix.Random(minValueMatrix, maxValueMatrix));
			population.Add(individual);
		}
	}
	

	//FITNESS FUNCTION
	public void Fit(bool parallel=true) {
		List<Individual> fitPopulation = new List<Individual>();
		EvaluatePopulation(parallel);

		for(int i = 0; i < populationCount; i++) {
			float score = UnityEngine.Random.value;
			int index = 0;
			for(index = populationCount - 1; index >= 0; index--) {
				if(score <= fitnessProportions[index]) {
					break;
				}
			}

			fitPopulation.Add(population[index]);
		}

		population = fitPopulation;
	}

	public void EvaluatePopulation(bool parallel=true) {
		if(parallel) {
			Parallel.For(0, populationCount - 1, i => {
				EvalutateIndividual(i);
			});
        } else {
			for(int i = 0; i < populationCount - 1; i++) {
				EvalutateIndividual(i);
            }
        }

		EvalutateIndividual(populationCount - 1, true);

		population.Sort((x, y) => x.fitnessScore.CompareTo(y.fitnessScore));
		population.Reverse();
	}

	private void EvalutateIndividual(int index, bool update=false) {
		float score = fitnessFunction(population[index].data);
		Individual individual = new Individual(population[index].data, score);
		population[index] = individual;

		if (update) {
			population.Sort((x, y) => x.fitnessScore.CompareTo(y.fitnessScore));
			population.Reverse();

			float sum = 0.0f;
			for (int i = 0; i < populationCount; i++) {
				fitnessProportions[i] = population[i].fitnessScore;
				sum += population[i].fitnessScore;
			}
			fitnessProportions *= (1.0f / sum);

			for (int i = populationCount - 2; i >= 0; i--) {
				fitnessProportions[i] += fitnessProportions[i+1];
            }
        }
	}

	//CROSSOVER
	public void Crossover(bool parallel=true) {
		if(parallel) {
			Parallel.For(0, (populationCount - 1)/2, i => {
				int index = i * 2;
				Individual[] offspring = Offspring(population[index], population[index + 1]);
				population[index] = offspring[0];
				population[index + 1] = offspring[1];
			});
        } else {
			for(int i = 0; i < populationCount - 1; i += 2) {
				Individual[] offspring = Offspring(population[i], population[i + 1]);
				population[i] = offspring[0];
				population[i + 1] = offspring[1];
			}
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
			bool flip = true;
			float distance = n/(float)(crossPoints + 1);
			float a = distance - 1;
			int i = 0;
			while(crossPoints > 0) {
				int point = UnityEngine.Mathf.RoundToInt(a);
				while(i <= point && i < n) {
					if (flip) {
						childA.data[(i + crossOffset) % n] = parentA.data[i];
						childB.data[(i + crossOffset) % n] = parentB.data[i];
                    }

					i++;
                }

				flip = !flip;
				a += distance;
				crossPoints--;
            }
        }

		children[0] = childA;
		children[1] = childB;

		return children;
	}

	//MUTATION
	public void Mutate(bool parallel=true) {
		if (parallel) {
			Parallel.For(0, populationCount, i => {
				MutateIndividual(i, true);
			});
        } else {
			for(int i = 0; i < populationCount; i++) {
				MutateIndividual(i, false);
			}
        }
	}
	
	private void MutateIndividual(int index, bool parallel=true) {
		for(int i = 0; i < population[index].data.GetLength(); i++) {
			float rvalue = (parallel) ? ParallelRandom.NextFloat() : UnityEngine.Random.value;

				if (rvalue < mutationRate) {
					float minValue;
					float maxValue;
					if (minMutationMatrix.Norm() == 0f && maxMutationMatrix.Norm() == 0f) {
						minValue = minValueMatrix[i];
						maxValue = maxValueMatrix[i];
					} else {
						minValue = population[index].data[i] + minMutationMatrix[i];
						if (minValue < minValueMatrix[i]) {
							minValue = minValueMatrix[i];
						}
					
						maxValue = population[index].data[i] + maxMutationMatrix[i];
						if (maxValue > maxValueMatrix[i]) {
							maxValue = maxValueMatrix[i];
						}
				}

				population[index].data[i] = (parallel) ? ParallelRandom.NextFloat(minValue, maxValue) : UnityEngine.Random.Range(minValue, maxValue);
			}
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