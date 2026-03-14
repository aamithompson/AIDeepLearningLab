//==============================================================================
// Filename: EcoTrainer.cs
// Author: Aaron Thompson
// Date Created: 7/1/2022
// Last Updated: 3/13/2026
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using adl;
using lmath;
using statistics;
using adl.genetics;
//------------------------------------------------------------------------------
[RequireComponent(typeof(EcoSpawnManager))]
[RequireComponent(typeof(SpeciesManager))]
public class EcoTrainer : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    private EcoSpawnManager ecoSpawnManager;
    private SpeciesManager speciesManager;

    private List<GeneticAlgorithm> geneAlgorithm;

    //Experiment Controls
    public bool running;
    public float sTime;

    //Whitelisting
    public string[] whitelist;
    public Dictionary<string, int> wlistDict;

    //Genetic Algorithm Settings
    public List<SpeciesAlgorithmSettings> algoSettings;
    public Dictionary<string, int> algoSettingsDict;

    public Distribution distributionType;
    public float minWeight = 0; //[UNIFORM]
    public float maxWeight = 0; //[UNIFORM]
    public float meanWeight = 0; //[GAUSSIAN]
    public float stdDevWeight = 0; //[GAUSSIAN]

    public float minBias = 0; //[UNIFORM]
    public float maxBias = 0; //[UNIFORM]
    public float meanBias = 0; //[GAUSSIAN]
    public float stdDevBias = 0; //[GAUSSIAN]

    public float minWeightMutation = 0; //[UNIFORM]
    public float maxWeightMutation = 0; //[UNIFORM]
    public float meanWeightMutation = 0; //[GAUSSIAN]
    public float stdDevWeightMutation = 0; //[GAUSSIAN]

    public float minBiasMutation = 0; //[UNIFORM]
    public float maxBiasMutation = 0; //[UNIFORM]
    public float meanBiasMutation = 0; //[GAUSSIAN]
    public float stdDevBiasMuation = 0; //[GAUSSIAN]

    //Fitness
    private int iSpec;
    private int iIndv;
    private Dictionary<int, float[]> scores;

// MONOBEHAVIOR FUNCTIONS
//------------------------------------------------------------------------------
    void Start() {
        ecoSpawnManager = GetComponent<EcoSpawnManager>();
        speciesManager = GetComponent<SpeciesManager>();

        InitalizeSettings();
        Populate();
    }

    void Update() {
        if(running) {
            sTime += Time.deltaTime;
        }
    }

// INITIALIZATION/UPDATE FUNCTIONS
//------------------------------------------------------------------------------
    public void InitalizeSettings() {
        scores = new Dictionary<int, float[]>();

        //Whitelist
        wlistDict = new Dictionary<string, int>();
        for(int i = 0; i < whitelist.Length; i++) {
            wlistDict.Add(whitelist[i], i);
        }

        //Algorithm Settings
        if(algoSettingsDict != null) {
            if (algoSettingsDict.ContainsKey("_DEFAULT")) {
                int index = algoSettingsDict["_DEFAULT"];
                algoSettings.RemoveAt(index);
            }
        }

        algoSettings.Add(new SpeciesAlgorithmSettings("_DEFAULT", distributionType,
                                              minWeight, maxWeight, meanWeight, stdDevWeight,
                                              minBias, maxBias, meanBias, stdDevBias,
                                              minWeightMutation, maxWeightMutation, meanWeightMutation, stdDevWeightMutation,
                                              minBiasMutation, maxBiasMutation, meanBiasMutation, stdDevBiasMuation));

        algoSettingsDict = new Dictionary<string, int>();
        for(int i = 0; i < algoSettings.Count; i++) {
            algoSettingsDict.Add(algoSettings[i].speciesName, i);
        }

        geneAlgorithm = new List<GeneticAlgorithm>();
        for (int i = 0; i < whitelist.Length; i++) {
            if(!speciesManager.speciesDict.ContainsKey(whitelist[i])) {
                geneAlgorithm.Add(null);
                continue;
            }

            int index = speciesManager.speciesDict[whitelist[i]];
            if(speciesManager.species[i].presets.Count == 0) {
                geneAlgorithm.Add(null);
                continue;
            }

            //Matrix Setup
            GameObject example = speciesManager.species[index].presets[0];
            UAgentCognition cognition = example.GetComponent<UAgentCognition>();
            int nW = 0;
            int nB = 0;
            if (cognition.hLayerWidth <= 0) {
                nW += cognition.inputSize * cognition.outputSize;
                nB += cognition.outputSize;
            } else {
                nW += cognition.inputSize * cognition.hLayerWidth; //in->hl weights
                nB += cognition.hLayerWidth; //in->hl biases
                nW += (cognition.hLayerDepth - 1) * (cognition.hLayerWidth * cognition.hLayerWidth); //hl->hl weights
                nB += (cognition.hLayerDepth - 1) * (cognition.hLayerWidth); //hl->hl biases;
                nW += cognition.hLayerWidth * cognition.outputSize; //hl->out weights
                nB += cognition.outputSize; //hl->out biases
            }
            int n = nW + nB;
            geneAlgorithm.Add(new GeneticAlgorithm());

            //Applying Settings
            int indexJ = algoSettingsDict["_DEFAULT"];
            if(algoSettingsDict.ContainsKey(whitelist[i])) {
                indexJ = algoSettingsDict[whitelist[i]];
            }

            if(distributionType == Distribution.Uniform) {
                geneAlgorithm[i].minValueVector = Vector.Zeros(n);
                geneAlgorithm[i].maxValueVector = Vector.Zeros(n);
                geneAlgorithm[i].minMutationVector = Vector.Zeros(n);
                geneAlgorithm[i].maxMutationVector = Vector.Zeros(n);
                geneAlgorithm[i].meanValueVector = new Vector();
                geneAlgorithm[i].stdDevValueVector = new Vector();
                geneAlgorithm[i].meanMutationVector = new Vector();
                geneAlgorithm[i].stdDevMutationVector = new Vector();

                for(int j = 0; j < nW; j++) {
                    geneAlgorithm[i].minValueVector[j] = algoSettings[indexJ].minWeight;
                    geneAlgorithm[i].maxValueVector[j] = algoSettings[indexJ].maxWeight;
                    geneAlgorithm[i].minMutationVector[j] = algoSettings[indexJ].minWeightMutation;
                    geneAlgorithm[i].maxMutationVector[j] = algoSettings[indexJ].maxWeightMutation;
                }

                for(int j = nW; j < n; j++) {
                    geneAlgorithm[i].minValueVector[j] = algoSettings[indexJ].minBias;
                    geneAlgorithm[i].maxValueVector[j] = algoSettings[indexJ].maxBias;
                    geneAlgorithm[i].minMutationVector[j] = algoSettings[indexJ].minBiasMutation;
                    geneAlgorithm[i].maxMutationVector[j] = algoSettings[indexJ].maxBiasMutation;
                }
            }

            if(distributionType == Distribution.Gaussian) {
                geneAlgorithm[i].minValueVector = new Vector();
                geneAlgorithm[i].maxValueVector = new Vector();
                geneAlgorithm[i].minMutationVector = new Vector();
                geneAlgorithm[i].maxMutationVector = new Vector();
                geneAlgorithm[i].meanValueVector = Vector.Zeros(n);
                geneAlgorithm[i].stdDevValueVector = Vector.Zeros(n);
                geneAlgorithm[i].meanMutationVector = Vector.Zeros(n);
                geneAlgorithm[i].stdDevMutationVector = Vector.Zeros(n);

                for(int j = 0; j < nW; j++) {
                    geneAlgorithm[i].meanValueVector[j] = algoSettings[indexJ].minWeight;
                    geneAlgorithm[i].stdDevValueVector[j] = algoSettings[indexJ].maxWeight;
                    geneAlgorithm[i].meanMutationVector[j] = algoSettings[indexJ].minWeightMutation;
                    geneAlgorithm[i].stdDevMutationVector[j] = algoSettings[indexJ].maxWeightMutation;
                }

                for(int j = nW; j < n; j++) {
                    geneAlgorithm[i].meanValueVector[j] = algoSettings[indexJ].minBias;
                    geneAlgorithm[i].stdDevValueVector[j] = algoSettings[indexJ].maxBias;
                    geneAlgorithm[i].meanMutationVector[j] = algoSettings[indexJ].minBiasMutation;
                    geneAlgorithm[i].stdDevMutationVector[j] = algoSettings[indexJ].maxBiasMutation;
                }
            }

            geneAlgorithm[i].populationCount = ecoSpawnManager.maxPopulations[index];
            geneAlgorithm[i].fitnessFunction = FitnessFunction;
            scores.Add(i, new float[geneAlgorithm[i].populationCount]);
        }
    }

// AGENT FUNCTIONS
//------------------------------------------------------------------------------
    public void InstantiateAgents() {
        speciesManager.Initialize();

        for(int i = 0; i < geneAlgorithm.Count; i++) {
            if(geneAlgorithm[i] == null) {
                continue;
            }

            int index = speciesManager.speciesDict[whitelist[i]];

            ecoSpawnManager.SpawnPopulation(index);
        }
    }

    public void UpdateAgents() {
        for(int i = 0; i < geneAlgorithm.Count; i++) {
            if(geneAlgorithm[i] == null) {
                continue;
            }

            int index = speciesManager.speciesDict[whitelist[i]];

            UAgentCognition[] cognitions = speciesManager.groups[index].GetComponentsInChildren<UAgentCognition>();
            for(int j = 0; j < cognitions.Length; j++) {
                cognitions[j].SetConfiguration(geneAlgorithm[i].GetIndividual(j));
            }
        }
    }

    public void ClearAgents() {
        speciesManager.ClearCreatures();
    }

// TRAINNING FUNCTIONS
//------------------------------------------------------------------------------
    public void Populate() {
        for (int i = 0; i < geneAlgorithm.Count; i++) {
            geneAlgorithm[i].Populate();
        }

        InstantiateAgents();
        UpdateAgents();
    }

    public void Step() {
        iSpec = 0;
        for (int i = 0; i < geneAlgorithm.Count; i++) {
            if (geneAlgorithm[i] == null) {
                iSpec++;
                continue;
            }

            iIndv = 0;
            geneAlgorithm[i].Fit();
            geneAlgorithm[i].Crossover();
            geneAlgorithm[i].Mutate();
            iSpec++;
        }

        ClearAgents();
        InstantiateAgents();
        UpdateAgents();
        sTime = 0;
    }

    public void Initialize() {
        running = true;

        InitalizeSettings();
        Populate();
    }

    public void Pause() {
        running = false;
    }

    public void Resume() {
        running = true;
        
        if(sTime == 0) {
            Step();
        }
    }

    public void Stop() {
        running = false;
        sTime = 0;

        ClearAgents();
    }

// SAVING/LOADING
//------------------------------------------------------------------------------
    public void Save() {

    }

    public void Load() {

    }

// FITNESS FUNCTION
//------------------------------------------------------------------------------
    public float FitnessFunction(Vector data) {
        float score = scores[iSpec][iIndv];
        iIndv++;
        return score;
    }

// STRUCT/CLASEES
//------------------------------------------------------------------------------
    [System.Serializable]
    public struct KeyValueIntPair{
        public int key;
        public int value;

        public KeyValueIntPair(int key, int value) {
            this.key = key;
            this.value = value;
        }
    }

    [System.Serializable]
    public struct SpeciesAlgorithmSettings {
        public string speciesName;
        public Distribution distributionType;
        public float minWeight; //[UNIFORM]
        public float maxWeight; //[UNIFORM]
        public float meanWeight; //[GAUSSIAN]
        public float stdDevWeight; //[GAUSSIAN]

        public float minBias; //[UNIFORM]
        public float maxBias; //[UNIFORM]
        public float meanBias; //[GAUSSIAN]
        public float stdDevBias; //[GAUSSIAN]

        public float minWeightMutation; //[UNIFORM]
        public float maxWeightMutation; //[UNIFORM]
        public float meanWeightMutation; //[GAUSSIAN]
        public float stdDevWeightMutation; //[GAUSSIAN]

        public float minBiasMutation; //[UNIFORM]
        public float maxBiasMutation; //[UNIFORM]
        public float meanBiasMutation; //[GAUSSIAN]
        public float stdDevBiasMuation; //[GAUSSIAN]

        public SpeciesAlgorithmSettings(string speciesName, Distribution distributionType, 
        float minWeight, float maxWeight, float meanWeight, float stdDevWeight,
        float minBias, float maxBias, float meanBias, float stdDevBias,
        float minWeightMutation, float maxWeightMutation, float meanWeightMutation, float stdDevWeightMutation,
        float minBiasMutaion, float maxBiasMutation, float meanBiasMutation, float stdDevBiasMutation) {
            this.speciesName = speciesName;
            this.distributionType = distributionType;

            this.minWeight = minWeight;
            this.maxWeight = maxWeight;
            this.meanWeight = meanWeight;
            this.stdDevWeight = stdDevWeight;

            this.minBias = minBias;
            this.maxBias = maxBias;
            this.meanBias = meanBias;
            this.stdDevBias = stdDevBias;

            this.minWeightMutation = minWeightMutation;
            this.maxWeightMutation = maxWeightMutation;
            this.meanWeightMutation = meanWeightMutation;
            this.stdDevWeightMutation = stdDevWeightMutation;

            this.minBiasMutation = minBiasMutaion;
            this.maxBiasMutation = maxBiasMutation;
            this.meanBiasMutation = meanBiasMutation;
            this.stdDevBiasMuation = stdDevBiasMutation;

        }
    }
}
//==============================================================================
//==============================================================================