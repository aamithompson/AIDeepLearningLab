//==============================================================================
// Filename: Population.cs
// Author: Aaron Thompson
// Date Created: 3/14/2026
// Last Updated: 3/14/2026
//
// Description: A collection of genomes which contains metadata and records for
// analysis.
//==============================================================================
using System;
using System.Collections.Generic;

namespace adl.genetics {
public class Population{
// VARIABLES
//------------------------------------------------------------------------------
    private List<Genome> genomes;
    private List<Record> history;

// CONSTRUCTORS
//------------------------------------------------------------------------------
    public Population() {
        genomes = new List<Genome>();
    }

    public Population(List<Genome> genomes) {
    }

    public Population(Population population) {
    }

// GETTER/SETTER FUNCTION(s)
//------------------------------------------------------------------------------
    public Genome GetGenome(int index) {
        return new Genome();
    }

    public void SetGenome(int index) {

    }

    public List<Genome> GetGenomes() {
        return new List<Genome>(); 
    }

    public void SetGenomes(List<Genome> genomes) {
    }

    //i = -1 retrieves the last record.
    public Record GetRecord(int index = -1) {
        return new Record();
    }

    //All Records
    public List<Record> GetHistory() {
        return new List<Record>();
    }

    //Inclusive - [start, . . ., end]
    public List<Record> GetHistory(int start, int end) {
        return new List<Record>();
    }

// GENERATION FUNCTION(s)
//------------------------------------------------------------------------------
    //Records current generation with history then clears current genomes.
    public void NewGeneration(string path="", bool write=false, bool replace=false) {

    }

    public void NewGeneration(List<Genome> genomes, string path="", bool replace=false) {

    }

// FILE FUNCTION(s)
//------------------------------------------------------------------------------
    public void WriteRecord(string path, int i=-1, bool replace=false) {

    }

    public void ReadRecord(string path) {

    }

    public void WriteHistory(string path, bool replace=false) {

    }

    public void ReadHistory(string path) {

    }
}
} //END namespace adl.genetics
//==============================================================================
//==============================================================================