//==============================================================================
// Filename: Population.cs
// Author: Aaron Thompson
// Date Created: 3/14/2026
// Last Updated: 3/18/2026
//
// Description: A collection of genomes which contains metadata and records for
// analysis.
//==============================================================================
using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using Newtonsoft.Json;
using UnityEngine.XR;

namespace adl.genetics {
public class Population{
// VARIABLES
//------------------------------------------------------------------------------
    private List<Genome> genomes;
    private List<Record> history;

    private int gCount;
    private int rCount;

// CONSTRUCTORS
//------------------------------------------------------------------------------
    public Population() {
        genomes = new List<Genome>();
        history = new List<Record>();
        gCount = 0;
        rCount = 0;
    }

    public Population(List<Genome> genomes) {
        genomes = new List<Genome>();
        history = new List<Record>();
        gCount = 0;
        rCount = 0;

        SetGenomes(genomes);
    }

    public Population(Population population) {
        genomes = new List<Genome>();
        history = new List<Record>();
        gCount = 0;
        rCount = 0;

        SetGenomes(population.GetGenomes());
        SetHistory(population.GetHistory());
    }

// GETTER/SETTER FUNCTION(s)
//------------------------------------------------------------------------------
    public Genome GetGenome(int index) {
        return new Genome(genomes[index]);
    }

    public Genome GetGenome(string id) {
        int index = GetGenomeIndex(id);
        if(index == -1) {
                return null;
        }

        return GetGenome(index);
    }

    public void SetGenome(int index, Genome genome) {
        genomes[index] = new Genome(genome);
    }

    public void SetGenome(string id, Genome genome) {
        int index = GetGenomeIndex(id);
        if(index == -1) {
                return;
        }

        SetGenome(index, genome);
    }

    public List<Genome> GetGenomes() {
        List<Genome> output = new List<Genome>();
        for(int i = 0; i < genomes.Count; i++) {
            output.Add(GetGenome(i));
        }

        return output;
    }

    public void SetGenomes(List<Genome> genomes) {
        this.genomes.Clear();

        for(int i = 0; i < genomes.Count; i++) {
            AddGenome(genomes[i]);
        }
    }

    //i = -1 retrieves the last record.
    public Record GetRecord(int index = -1) {
        if(index == -1) { 
            index = history.Count - 1;
        }

        return new Record(history[index]);
    }

    //All Records
    public List<Record> GetHistory() {
        List<Record> output = new List<Record>();
        for(int i = 0; i < history.Count; i++) {
            output.Add(GetRecord(i));
        }

        return output;
    }

    //Inclusive - [start, . . ., end]
    public List<Record> GetHistory(int start, int end) {
        List<Record> output = new List<Record>();
        for(int i = start; i <= end; i++) {
            output.Add(GetRecord(i));
        }

        return output;
    }

    private void SetHistory(List<Record> history) {
        this.history.Clear();

        for(int i = 0; i < history.Count; i++) {
            this.history.Add(new Record(history[i]));
        }

        rCount = history.Count;
    }

    public int GetGenomeCount() {
        return gCount;
    }

    public int GetRecordCount() {
        return rCount;
    }

    private int GetGenomeIndex(string id) {
        for(int i = 0; i < genomes.Count; i++) {
            if(genomes[i].id == id) {
                return i;
            }
        }

        return -1;
    }

// DATA MANIPULATION FUNCTION(s)
//------------------------------------------------------------------------------
    public void AddGenome(Genome genome) {
        genomes.Add(new Genome(genome));
        gCount++;
    }

    public void RemoveGenome(int index) { 
        genomes.RemoveAt(index);
        gCount--;
    }

    public void RemoveGenome(string id) {
        int index = GetGenomeIndex(id);
        if(index == -1) {
            return;
        }

        RemoveGenome(index);
    }

// GENERATION FUNCTION(s)
//------------------------------------------------------------------------------
    //Records current generation with history then clears current genomes.
    public void NewGeneration(string path="", bool write=false) {
        int generation = (rCount > 0) ? GetRecord(-1).GetGeneration() + 1 : 1;
        Record record = new Record(genomes, generation);

        rCount++;
        history.Add(record);
        rCount++;
        if(write) {
            WriteRecord(path, -1);
        }

        genomes.Clear();
        gCount = 0;
    }

    public void NewGeneration(List<Genome> newGenomes, string path="", bool write=false) {
        int generation = (rCount > 0) ? GetRecord(-1).GetGeneration() + 1 : 1;
        Record record = new Record(genomes, generation);

        history.Add(record);
        rCount++;
        if(write) {
            WriteRecord(path, -1);
        }

        genomes.Clear();
        gCount = 0;
        SetGenomes(newGenomes);
    }

// FILE FUNCTION(s)
//------------------------------------------------------------------------------
    public void WriteRecord(string path, int index=-1) {
        if(index < 0) {
            index = history.Count + index;
        }

        string rJson = history[index].EncodeData();
        
        List<object> records;
        if(System.IO.File.Exists(path)) {
            string s = System.IO.File.ReadAllText(path);
            records = JsonConvert.DeserializeObject<List<object>>(s);
        } else {
            records = new List<object>();
        }

        records.Add(JsonConvert.DeserializeObject(rJson));
        System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(records, Formatting.Indented));
    }

    public void WriteHistory(string path, bool replace=false) {
        List<string> rJsons = new List<string>();
        for(int i = 0; i < rCount; i++) {
            rJsons.Add(history[i].EncodeData());
        }

        List<object> records;
        if(!replace && System.IO.File.Exists(path)) {
            string s = System.IO.File.ReadAllText(path);
            records = JsonConvert.DeserializeObject<List<object>>(s);
        } else {
            records = new List<object>();
        }

        for(int i = 0; i < rCount; i++) {
            records.Add(JsonConvert.DeserializeObject(rJsons[i]));
        }

        System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(records, Formatting.Indented));
    }

    public void ReadHistory(string path) {
        string s = System.IO.File.ReadAllText(path);
        List<object> rJsons = JsonConvert.DeserializeObject<List<object>>(s);
        List<Record> records = new List<Record>();
        for(int i = 0; i < rJsons.Count; i++) {
            Record record = new Record();
            record.DecodeData(JsonConvert.SerializeObject(rJsons[i]));
            records.Add(record);
        }

        SetHistory(records);
    }
}
} //END namespace adl.genetics
//==============================================================================
//==============================================================================