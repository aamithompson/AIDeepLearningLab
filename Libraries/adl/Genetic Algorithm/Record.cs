//==============================================================================
// Filename: Record.cs
// Author: Aaron Thompson
// Date Created: 3/14/2026
// Last Updated: 3/16/2026
//
// Description: Data for fitness, parent/child relations, generations, etc.
//==============================================================================
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace adl.genetics {
public class Record {
// VARIABLES
//------------------------------------------------------------------------------
    private List<Genome> genomes;
    private int count;
    private int generation;
    private float maxFitness;
    private float minFitness;
    private float avgFitness;
    private float medianFitness;
    private float stdDevFitness;
    private DateTime dateCreated;
    private DateTime dateLastModified;

    private bool sorted;
    private bool updatedStatistics;

// CONSTRUCTOR(s)/DESTRUCTOR(s)
//------------------------------------------------------------------------------
    public Record() {
        this.genomes = new List<Genome>();
        this.count = 0;
        this.generation = 0;
        this.maxFitness = 0;
        this.minFitness = 0;
        this.avgFitness = 0;
        this.medianFitness = 0;
        this.stdDevFitness = 0;
        this.dateCreated = DateTime.Now;
        this.dateLastModified = DateTime.Now;
        
        this.sorted = true;
        this.updatedStatistics = true;
    }

    public Record(List<Genome> genomes, int generation=0) {
        this.genomes = new List<Genome>();
        this.dateCreated = DateTime.Now;
        this.dateLastModified = DateTime.Now;

        SetGenomes(genomes);
    }

// GETTER(s)/SETTER(s)
//------------------------------------------------------------------------------
    public int GetCount() {
        return count;
    }

    public int GetGeneration() {
        return generation;
    }

    public Genome GetGenome(string id) {
        int index = GetGenomeIndex(id);
        if(index == -1) {
            return null;
        }

        return new Genome(genomes[index]);
    }

    public List<Genome> GetGenomes() {
        return GetBestGenomes(count);
    }

    public void SetGenomes(List<Genome> genomes) { 
        Clear();
        AddGenomes(genomes);
    }

    public Genome GetBestGenome() {
        if(count == 0) {
            return null;
        }

        Sort();
        return new Genome(genomes[0]);
    }

    public List<Genome> GetBestGenomes(int n = 1) {
        List<Genome> result = new List<Genome>();
        if(count > 0) {
            Sort();
            for(int i = 0; i < n; i++) {
                    result.Add(new Genome(genomes[i]));
            }
        }

        return result;
    }

    public void SetGeneration(int generation) {
        this.generation = generation;
    }

    public float GetMaxFitness() {
        UpdateStatistics();
        return maxFitness;
    }

    public float GetMinFitness() {
        UpdateStatistics();
        return minFitness;
    }

    public float GetAverageFitness() {
        UpdateStatistics();
        return avgFitness;
    }

    public float GetMedianFitness() {
        UpdateStatistics();
        return medianFitness;
    }

    public float GetStandardDeviation() { 
        UpdateStatistics();
        return stdDevFitness;
    }

    private int GetGenomeIndex(string id) {
        for(int i = 0; i < count; i++) {
            if(genomes[i].id == id) {
                return i;
            }
        }

        return -1;
    }

// DATA MANIPULATION FUNCTION(s)
//------------------------------------------------------------------------------
    public void AddGenome(Genome genome) {
        int index = GetGenomeIndex(genome.id);
        if(index != -1) {
            genomes[index] = genome;
        } else {
            genomes.Add(genome);
            count++;
        }

        sorted = false;
        updatedStatistics = false;
    }

    public void AddGenomes(List<Genome> genomes) {
        for(int i = 0; i < genomes.Count; i++) {
            AddGenome(genomes[i]);
        }
    }

    public void RemoveGenome(string id) {
        int index = GetGenomeIndex(id);
        if(index != -1) {
            genomes.RemoveAt(index);
            count--;
            updatedStatistics = false;
        }
    }

    public void RemoveGenome(Genome genome) {
        RemoveGenome(genome.id);
    }

    public void RemoveGenomes(List<string> id) {

    }

    public void RemoveGenomes(List<Genome> genomes) {

    }

    public void Clear() {

    }

    public bool HasGenome(Genome genome) {
        return GetGenomeIndex(genome.id) != -1;
    }

    public bool HasGenome(string id) {
        return GetGenomeIndex(id) != -1;
    }

    //Needs Implementation
    private void Sort() {
        if(!sorted) {
            sorted = true;
        }
    }
// STATISTIC FUNCTION(s)
//------------------------------------------------------------------------------ 
    //Needs Implementation
    private void UpdateStatistics() {
        if(!updatedStatistics) {
            updatedStatistics = true;
        }
    }
}
} //END namespace adl.genetics
//==============================================================================
//==============================================================================
