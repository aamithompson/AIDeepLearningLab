﻿//==============================================================================
// Filename: Statistics.cs
// Author: Aaron Thompson
// Date Created: 8/13/2021
// Last Updated: 8/13/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;

namespace adl {
public class DataSet {
    private List<Data> dataSet;
    public int index { get; private set; }
    public int size { get { return dataSet.Count; } }
    public bool isEmpty { get { return size == 0; } }

    public DataSet() {
        index = 0;
        dataSet = new List<Data>();
    }

    public DataSet(List<Vector<float>> x, List<Vector<float>> y) {
        index = 0;
        dataSet = new List<Data>();
        AddSet(x, y);
    }

    public void Shuffle(){
        int n = dataSet.Count;
        while(n > 1) {
            n--;
            int k = UnityEngine.Random.Range(0, n+1);
            if(n == k) {
                continue;
            }

            Data temp = new Data(dataSet[k]);
            dataSet[k] = new Data(dataSet[n]);
            dataSet[n] = temp;
        }
        
        index = 0;
    }

    public void Add(Vector<float> x, Vector<float> y) {
        dataSet.Add(new Data(x, y));
    }

    public void AddSet(List<Vector<float>> x, List<Vector<float>> y) {
        int n = Mathf.Min(x.Count, y.Count);
        for(int i = 0; i < n; i++) {
            Add(x[i], y[i]);
        }
    }

    public List<Data> GetNextBatch(int batchSize) {
        List<Data> batch = new List<Data>();
        if(index + batchSize >= dataSet.Count) {
            Shuffle();
        }

        for(int i = 0; i < batchSize; i++) {
            batch.Add(new Data(dataSet[i]));
        }

        index += batchSize;
        return batch;
    }

    public List<List<Data>> GetEpochBatchSet(int batchSize) {
        List<List<Data>> epoch = new List<List<Data>>();

        Shuffle();
        while(index + batchSize < dataSet.Count) {
            epoch.Add(GetNextBatch(batchSize));
        }

        return epoch;
    }

    public void Clear() {
        dataSet.Clear();
        index = 0;
    }
}
public class Data {
    public Vector<float> x;
    public Vector<float> y;
    public Data(Vector<float> x, Vector<float> y) {
        this.x = new Vector<float>(x);
        this.y = new Vector<float>(y);
    }

    public Data(Data data) {
        this.x = new Vector<float>(data.x);
        this.y = new Vector<float>(data.y);
    }
}
}// END namespace adl
//==============================================================================
//==============================================================================