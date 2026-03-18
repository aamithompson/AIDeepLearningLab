//==============================================================================
// Filename: Record.cs
// Author: Aaron Thompson
// Date Created: 3/14/2026
// Last Updated: 3/18/2026
//
// Description: Data for fitness, parent/child relations, generations, etc.
//==============================================================================
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using statistics;
using Newtonsoft.Json;

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
        this.generation = generation;
        this.dateCreated = DateTime.Now;
        this.dateLastModified = DateTime.Now;

        SetGenomes(genomes);
    }

    public Record(Record record) {
        this.genomes = new List<Genome>();
        this.generation = record.GetGeneration();
        this.dateCreated = record.GetDateCreated();

        SetGenomes(genomes);
        Sort();
        UpdateStatistics();
        this.dateLastModified = record.GetDateLastModified();
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

    public DateTime GetDateCreated() {
        return dateCreated;
    }

    public DateTime GetDateLastModified() {
        return dateLastModified;
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
        dateLastModified = DateTime.Now;
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
            dateLastModified = DateTime.Now;
        }
    }

    public void RemoveGenome(Genome genome) {
        RemoveGenome(genome.id);
    }

    public void RemoveGenomes(List<string> ids) {
        HashSet<string> set = new HashSet<string>(ids);
        for(int i = 0; i < count; i++) {
            if(set.Contains(genomes[i].id)) {
                genomes.RemoveAt(i);
                i--;
                count--;
                updatedStatistics = false;
                dateLastModified = DateTime.Now;
            }
        }
    }

    public void RemoveGenomes(List<Genome> genomes) {
        List<string> ids = genomes.Select(x => x.id).ToList();
        RemoveGenomes(ids);
    }

    public void Clear() {
        genomes.Clear();

        count = 0;
        generation = 0;
        maxFitness = 0;
        minFitness = 0;
        avgFitness = 0;
        medianFitness = 0;
        stdDevFitness = 0;

        dateLastModified = DateTime.Now;
        sorted = true;
        updatedStatistics = true;
    }

    public bool HasGenome(Genome genome) {
        return GetGenomeIndex(genome.id) != -1;
    }

    public bool HasGenome(string id) {
        return GetGenomeIndex(id) != -1;
    }

    private void Sort() {
        if(!sorted) {
            genomes = genomes.OrderByDescending(g => g.fitness).ToList();
            sorted = true;
            dateLastModified = DateTime.Now;
        }
    }

    public string EncodeData(){
        Sort();
        UpdateStatistics();

        var obj = new {
            generation = generation,
            count = count,
            maxFitness = maxFitness,
            minFitness = minFitness,
            avgFitness = avgFitness,
            medianFitness = medianFitness,
            stdDevFitness = stdDevFitness,
            dateCreated = dateCreated,
            dateLastModified = dateLastModified,
            genomes = genomes.Select(g => JsonConvert.DeserializeObject(g.EncodeData())).ToList()
        };

        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }

    public void DecodeData(string json) {
        var obj = JsonConvert.DeserializeObject<RecordData>(json);

        List<Genome> decoded = new List<Genome>();
        foreach(string genomeJson in obj.genomes) {
            Genome genome = new Genome();
            genome.DecodeData(genomeJson);
            decoded.Add(genome);
        }
        SetGenomes(decoded);

        generation = obj.generation;

        maxFitness = obj.maxFitness;
        minFitness = obj.minFitness;
        avgFitness = obj.avgFitness;
        medianFitness = obj.medianFitness;
        stdDevFitness = obj.stdDevFitness;
        dateCreated = obj.dateCreated;
        dateLastModified = obj.dateLastModified;

        updatedStatistics = true;
        sorted = true;
    }

// STATISTIC FUNCTION(s)
//------------------------------------------------------------------------------ 
    private void UpdateStatistics() {
        if(!updatedStatistics) {
            List<float> fitnesses = genomes.Select(g => g.fitness).ToList();
            maxFitness = Statistics.Max(fitnesses);
            minFitness = Statistics.Min(fitnesses);
            avgFitness = Statistics.Mean(fitnesses);
            medianFitness = Statistics.Median(fitnesses);
            stdDevFitness = Statistics.StandardDeviation(fitnesses);
            updatedStatistics = true;
            dateLastModified = DateTime.Now;
        }
    }
// HELPER CLASSES
//------------------------------------------------------------------------------
    private class RecordData {
        public int generation { get; set; }
        public int count { get; set; }
        public float maxFitness { get; set; }
        public float minFitness { get; set; }
        public float avgFitness { get; set; }
        public float medianFitness { get; set; }
        public float stdDevFitness { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateLastModified { get; set; }
        public List<string> genomes { get; set; }
    }
}
} //END namespace adl.genetics
//==============================================================================
//==============================================================================
