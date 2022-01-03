//==============================================================================
// Filename: Statistics.cs
// Author: Aaron Thompson
// Date Created: 7/20/2021
// Last Updated: 9/4/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;
//------------------------------------------------------------------------------
namespace statistics {
public static class Statistics {
// CORRECT/ERROR
//------------------------------------------------------------------------------
    //True Positive
    public static int TP(List<bool> yData, List<bool> yTarget) {
        int count = 0;
        for(int i = 0; i < yData.Count; i++) {
            if(yData[i] && yTarget[i]) {
                count++;
            }
        }

        return count;
    }

    //False Positive
    public static int FP(List<bool> yData, List<bool> yTarget) {
        int count = 0;
        for(int i = 0; i < yData.Count; i++) {
            if(yData[i] && !yTarget[i]) {
                count++;
            }
        }

        return count;
    }

    //True Negative
    public static int TN(List<bool> yData, List<bool> yTarget) {
        int count = 0;
        for(int i = 0; i < yData.Count; i++) {
            if(!yData[i] && !yTarget[i]) {
                count++;
            }
        }

        return count;
    }

    //False Negative
    public static int FN(List<bool> yData, List<bool> yTarget) {
        int count = 0;
        for(int i = 0; i < yData.Count; i++) {
            if(!yData[i] && yTarget[i]) {
                count++;
            }
        }

        return count;
    }

// CORRECT/ERROR
//------------------------------------------------------------------------------
    public static float Precision(List<bool> yData, List<bool> yTarget) { 
        float tp = TP(yData, yTarget);
        float fp = FP(yData, yTarget);
        return tp / (tp + fp);
    }

    public static float Recall(List<bool> yData, List<bool> yTarget) {
        float tp = TP(yData, yTarget);
        float fn = FN(yData, yTarget);
        return tp / (tp + fn);
    }

    public static float Accuracy(List<bool> yData, List<bool> yTarget) {
        float tp = TP(yData, yTarget);
        float tn = TN(yData, yTarget);
        float fp = FP(yData, yTarget);
        float fn = FN(yData, yTarget);
        return (tp + tn) / (tp + tn + fp + fn);
    }

    public static float Accuracy(List<Vector> yData, List<Vector> yTarget) {
            int correct = 0;
            int n = Mathf.Min(yData.Count, yTarget.Count);
            int l = yTarget[0].length;
            for (int i = 0; i < n; i++) {
                int best_yPredicted = 0;
                int best_yActual = 0;
                for (int j = 1; j < l; j++) {
                    if (yData[i][j] > yData[i][best_yPredicted]) { best_yPredicted = j; }
                    if (yTarget[i][j] > yTarget[i][best_yActual]) { best_yActual = j; }
                }
                if (best_yPredicted == best_yActual) { correct++; }
            }

            return (100 * ((float)correct / (float)n));
        }

    public static float F1(List<bool> yData, List<bool> yTarget) {
        return FB(yData, yTarget, 1);
    }

    public static float FB(List<bool> yData, List<bool> yTarget, int B) {
        float precision = Precision(yData, yTarget);
        float recall = Recall(yData, yTarget);
        return (precision * recall)/((B * B) * precision + recall);
    }

// DISTRIBUTIONS
//------------------------------------------------------------------------------
    //GAUSSIAN/NORMAL
    //Marsaglia Polar Method
    private static float spare;
    private static bool hasSpare = false;
    public static float randomN(float mean, float stdDev) {
        if(hasSpare) {
            hasSpare = false;
            return spare * stdDev + mean;
        }

        float u, v, s;
        do {
            u = UnityEngine.Random.Range(-1.0f, 1.0f);
            v = UnityEngine.Random.Range(-1.0f, 1.0f);
            s = u * u + v * v;
        } while (s >= 1 || s <= 0.0000000001f);

        s = Mathf.Sqrt(-2 * Mathf.Log(s) / s);
        spare = v * s;
        hasSpare = true;

        return mean + stdDev * u * s;
    }
}
}// END namespace adl
//==============================================================================
//==============================================================================
