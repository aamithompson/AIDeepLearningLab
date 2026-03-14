using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEGACY : MonoBehaviour
{
    /*public void Print() {
        MonoBehaviour.print(ToString());
    }

    public void Randomize(LArray min, LArray max)
    {
        float[] minData = min.AccessData();
        float[] maxData = max.AccessData();
        for (int i = 0; i < data.Length; i++)
        {
            float minValue = minData[i];
            float maxValue = maxData[i];
            data[i] = UnityEngine.Random.Range(minValue, maxValue);
        }
    }

    public void Randomize(float min, float max)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = UnityEngine.Random.Range(min, max);
        }
    }

    public static float randomN(float mean, float stdDev, bool parallel = false)
    {
        if (hasSpare)
        {
            hasSpare = false;
            return spare * stdDev + mean;
        }

        float u, v, s;
        do
        {
            if (parallel)
            {
                u = ParallelRandom.NextFloat(-1.0f, 1.0f);
                v = ParallelRandom.NextFloat(-1.0f, 1.0f);
            }
            else
            {
                u = UnityEngine.Random.Range(-1.0f, 1.0f);
                v = UnityEngine.Random.Range(-1.0f, 1.0f);
            }

            s = u * u + v * v;
        } while (s >= 1 || s <= 0.0000000001f);

        s = System.MathF.Sqrt(-2 * Mathf.Log(s) / s);
        spare = v * s;
        hasSpare = true;

        return mean + stdDev * u * s;
    }*/
}
