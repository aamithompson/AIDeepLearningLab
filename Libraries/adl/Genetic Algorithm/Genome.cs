//==============================================================================
// Filename: Genome.cs
// Author: Aaron Thompson
// Date Created: 3/9/2026
// Last Updated: 3/14/2026
//
// Description: A class for storing neural network data. Functions as an
// intermediary between raw text data and in-program data.
//==============================================================================
using lmath;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

//TODO:
//CHANGE valdiation check of data vector to compare against flat data size function

namespace adl.genetics {
public class Genome {
// VARIABLES
//------------------------------------------------------------------------------
    //Private Network Data - Accessed through public API
    private List<Matrix> weights;
    private List<Vector> biases;
    private List<int> layerWidths;
    private Vector flatData; //1D flat representation of weights and biases together
    bool isFlattened; //flag for if data is flatten properly since last data manipulation
    private int depth;

    //Public Evolutionary Metadata
    public string model;
    public string id;
    public List<string> parentIDs;
    public string species;
    public int generation;
    public float fitness;

    public int Depth {
        get { return depth; }
        set { SetDepth(value); }
    }

// CONSTRUCTOR(s)
//------------------------------------------------------------------------------
    public Genome() {
        weights = new List<Matrix>();
        biases = new List<Vector>();
        layerWidths = new List<int>();
        flatData = new Vector();
        isFlattened = true;
        depth = 0;
    }

    public Genome(Genome other) {
        weights = new List<Matrix>();
        biases = new List<Vector>();
        layerWidths = new List<int>();
        flatData = new Vector();
        isFlattened = true;
        depth = 0;

        SetDepth(other.depth);
        for(int i = 0; i < depth; i++) {
            SetLayerWidth(i, other.GetLayerWidth(i));
        }

        for(int i = 0; i < depth - 1; i++) {
            SetWeightMatrix(i, other.GetWeightMatrix(i));
            SetBiasVector(i+1, other.GetBiasVector(i+1));
        }

        Flatten();
    }

// GETTER/SETTER FUNCTION(s)
//------------------------------------------------------------------------------
    public int GetDepth() {
        return depth;
    }

    public void SetDepth(int depth) {
        int diff = depth - this.depth;
        if (diff == 0) {
            return;
        }

        if (diff < 0) {
            while (weights.Count > Math.Max(depth - 1, 1)) {
                weights.RemoveAt(weights.Count - 1);
                biases.RemoveAt(biases.Count - 1);
                layerWidths.RemoveAt(layerWidths.Count - 1);
            }

            if (depth == 0) {
                layerWidths.RemoveAt(layerWidths.Count - 1);
            }
        }

        if (diff > 0) {
            if (this.depth == 0) {
                layerWidths.Add(0);
            }

            while (weights.Count < depth - 1) {
                weights.Add(new Matrix());
                biases.Add(new Vector());
                layerWidths.Add(0);
            }
        }

        isFlattened = false;
        this.depth = depth;
    }

    public int GetLayerWidth(int layer) {
        if(layer < 0) {
            layer = depth + layer;
        }
        ValidateLayerLessThan(layer, depth);

        return layerWidths[layer];
    }

    public void SetLayerWidth(int layer, int width) {
        ValidateWidth(width);
        if(layer < 0) {
            layer = depth + layer;
        }
        ValidateLayerLessThan(layer, depth);

        if (layer < depth - 1) {
            weights[layer].Reshape(width, weights[layer].GetShape()[1]);
        }

        if (layer > 0) {
            weights[layer - 1].Reshape(weights[layer - 1].GetShape()[0], width);
            biases[layer - 1].Reshape(width);
        }

        isFlattened = false;
        layerWidths[layer] = width;
    }

    public float GetBiasValue(int layer, int i) {
        if(layer < 0) {
            layer = (depth - 1) + layer;
        }
        ValidateLayerLessThan(layer, depth);
        if(i < 0) {
            i = layerWidths[layer] + i;
        }
        ValidateBiasIndex(layer, i);

        //Bias indexing "starts" at 1, because the first bias vector applies to the vector after the input vector.
        //Thus the layer specified is the layer the bias is applying to, however biases is still a d-1 size container
        //which requires to set the indexing back to not waste space.
        return biases[layer - 1][i];
    }
    public void SetBiasValue(int layer, int i, float bias) {
        if(layer < 0) {
            layer = (depth - 1) + layer;
        }
        ValidateLayerLessThan(layer, depth);
        if(i < 0) {
            i = layerWidths[layer] + i;
        }
        ValidateBiasIndex(layer, i);

        isFlattened = false;
        biases[layer - 1][i] = bias;
    }
    public Vector GetBiasVector(int layer) {
        if(layer < 0) {
            layer = (depth - 1) + layer;
        }
        ValidateLayerLessThan(layer, depth);

        return new Vector(biases[layer - 1]);
    }

    public void SetBiasVector(int layer, Vector v) {
        if(layer < 0) {
            layer = (depth - 1) + layer;
        }
        ValidateLayerLessThan(layer, depth);
        ValidateBiasVector(layer, v);

        isFlattened = false;
        biases[layer - 1].Copy(v);
    }

    public void SetWeightValue(int layer, int from, int to, float weight) {
        if (layer < 0) {
            layer = (depth - 1) + layer;
        }
        ValidateLayerLessThan(layer, depth - 1);
        if(from < 0) {
            from = layerWidths[layer] + from;
        }
        if(to < 0) {
            to = layerWidths[layer + 1] + to;
        }
        ValidateWeightIndex(layer, from, to);

        isFlattened = false;
        weights[layer].SetElement(weight, from, to);
    }

    public Matrix GetWeightMatrix(int layer) {
        if (layer < 0) {
            layer = (depth - 1) + layer;
        }
        ValidateLayerLessThan(layer, depth - 1);

        return new Matrix(weights[layer]);
    }

    public void SetWeightMatrix(int layer, Matrix A) {
        if (layer < 0) {
            layer = (depth - 1) + layer;
        }
        ValidateLayerLessThan(layer, depth - 1);
        ValidateWeightMatrix(layer, A);

        isFlattened = false;
        weights[layer].Copy(A);
    }

    public Vector GetData() {
        if (!isFlattened) {
            Flatten();
        }

        return new Vector(flatData);
    }

    public void SetData(Vector data) {
        ValidateData(data);

        flatData.Copy(data);
        Unflatten();
    }

// CONVERSION FUNCTION(s)
//------------------------------------------------------------------------------
    //Flat data format is as follows where wi = weight matrix i and bi = bias vector i:
    //w0, b0, w1, b1, . . .

    //Syncs flat data to layered data
    private void Flatten() {
        int flatSize = GetFlatSize();
        if(flatData.length != flatSize) {
            flatData.Reshape(flatSize);
        }

        int index = 0;
        for(int i = 0; i < depth - 1; i++) {
            //Weights
            for(int j = 0; j < layerWidths[i]; j++) {
                for(int k = 0; k < layerWidths[i+1]; k++) {
                    flatData[index] = weights[i].GetElement(j, k);
                    index++;
                }
            }

            //Biases
            for (int j = 0; j < layerWidths[i + 1]; j++) {
                flatData[index] = biases[i][j];
                index++;
            }
        }

        isFlattened = true;
    }

    //Syncs layered data to flat data
    private void Unflatten() {
        int flatSize = GetFlatSize();
        if (flatData.length != flatSize) {
            flatData.Reshape(flatSize);
        }

        int index = 0;
        for(int i = 0; i < depth - 1; i++) {
            //Weights
            for(int j = 0; j < layerWidths[i]; j++) {
                for(int k = 0; k < layerWidths[i+1]; k++) {
                    weights[i].SetElement(flatData[index], j, k);
                    index++;
                }
            }

            //Biases
            for(int j = 0; j < layerWidths[i+1]; j++) {
                biases[i][j] = flatData[index];
                index++;
            }
        }

    }

    public string EncodeData() {
        if(!isFlattened) {
            Flatten();
        }

        //JSON data
        var obj = new
        {
            model = model,
            id = id,
            parentIDs = parentIDs,
            species = species,
            generation = generation,
            fitness = fitness,
            depth = depth,
            layerWidths = layerWidths,
            data = flatData.GetData()
        };

        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }

    public void DecodeData(string json) {
        GenomeData gData = JsonConvert.DeserializeObject<GenomeData>(json);

        model = gData.model;
        id = gData.id;
        parentIDs = gData.parentIDs;
        species = gData.species;
        generation = gData.generation;
        fitness = gData.fitness;

        SetDepth(gData.depth);
        for(int i = 0; i < depth; i++) {
            SetLayerWidth(i, gData.layerWidths[i]);
        }
        SetData(new Vector(gData.data));
    }

// HELPER FUNCTION(s)
//------------------------------------------------------------------------------
    private int GetFlatSize() {
        int size = 0;
        for(int i = 0; i < depth - 1; i++) {
            size += layerWidths[i] * layerWidths[i + 1]; //Weights
            size += layerWidths[i + 1]; //Biases
        }

        return size;
    }

    private class GenomeData {
        public string model { get; set; }
        public string id { get; set; }
        public List<string> parentIDs { get; set; }
        public string species { get; set; }
        public int generation { get; set; }
        public float fitness { get; set; }
        public int depth { get; set; }
        public List<int> layerWidths { get; set; }
        public float[] data { get; set; }
    }

// VALDIDATION FUNCTION(s)
//------------------------------------------------------------------------------
    private void ValidateLayerLessThan(int layer, int restriction) {
        if(layer >= restriction) {
            throw new System.ArgumentOutOfRangeException(nameof(layer), layer, $"Layer {layer} is greater than or equal to allowed layer {restriction}.");
        }
    }

    private void ValidateBiasIndex(int layer, int index) {
        //ValidateLayerLessThan(layer, depth - 1);
        if(layer == 0) {
            throw new System.ArgumentOutOfRangeException(nameof(layer), layer, $"Layer {layer} is the layer of the input vector and is not allowed. Biases exist only on layers indices > 0.");
        }

        if(index >= layerWidths[layer]) {
            throw new System.ArgumentOutOfRangeException(nameof(layer), layer, $"Index {index} for bias vector in layer {layer} is greater than allowed width {layerWidths[layer] - 1}.");
        }
    }

    private void ValidateBiasVector(int layer, Vector v) {
        if(v.length != layerWidths[layer]) {
            throw new System.ArgumentException($"Vector size {v.length} does not equal expected size {layerWidths[layer]} at layer {layer}");
        }
    }

    private void ValidateWeightIndex(int layer, int from, int to) {
        if(from >= layerWidths[layer]) {
            throw new System.ArgumentException($"From index {from} is greater than allowed width {layerWidths[layer] - 1}");
        }

        if (to >= layerWidths[layer + 1]) {
            throw new System.ArgumentException($"To index {to} is greater than allowed width {layerWidths[layer + 1] - 1}");
        }
    }

    private void ValidateWeightMatrix(int layer, Matrix A) {
        if(A.GetShape()[0] != layerWidths[layer]) {
            throw new System.ArgumentException($"Matrix width {A.GetShape()[0]} does not equal expected size {layerWidths[layer]} at layer {layer}");
        }

        if(A.GetShape()[1]  != layerWidths[layer + 1]) {
            throw new System.ArgumentException($"Matrix height {A.GetShape()[1]} does not equal expected size {layerWidths[layer+1]} at layer {layer+1}");
        }
    }

    private void ValidateWidth(int width) {
        if(width < 0) {
            throw new System.ArgumentException($"Width {width} is a negative value. Width must be non-negative.");
        }
    }

    private void ValidateData(Vector data) {
        int flatSize = GetFlatSize();
        if(data.length != flatSize) {
            throw new System.ArgumentException($"Data size {data.length} does not equal expected size {flatSize}.");
        }
    }
}
} //END namespace adl.genetics
//==============================================================================
//==============================================================================
