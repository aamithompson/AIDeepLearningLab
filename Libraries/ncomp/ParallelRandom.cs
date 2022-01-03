//==============================================================================
// Filename: ParallelRandom.cs
// Author: Aaron Thompson
// Date Created: 12/30/2020
// Last Updated: 12/30/2020
//
// Description: https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
//==============================================================================
namespace ncomp {
public static class ParallelRandom {
    private static System.Random _global = new System.Random();
    [System.ThreadStatic]
    private static System.Random _local;

    //Returns a number from 0 to System.Int32.MaxValue
    public static int Next() {
        System.Random instance = _local;
        if(instance == null) {
            int seed;
            lock (_global) seed = _global.Next();
            _local = instance = new System.Random(seed);
        }

        return instance.Next();
    }

    public static int Next(int min, int max) {
        return Next() * (max - min) + min;
    }
    
    //Returns a number from 0.0 to 1.0
    public static double NextDouble() {
        System.Random instance = _local;
        if(instance == null) {
            int seed;
            lock (_global) seed = _global.Next();
            _local = instance = new System.Random(seed);
        }

        return instance.NextDouble();
    }

    public static double NextDouble(double min, double max) {
        return NextDouble() * (max - min) + min;
    }

    //Returns a number from 0.0f to 1.0f
    public static float NextFloat() {
        return (float)NextDouble();
    }

    public static float NextFloat(float min, float max) {
        return NextFloat() * (max - min) + min;
    }
}
}// END namespace ncomp
//==============================================================================
//==============================================================================
