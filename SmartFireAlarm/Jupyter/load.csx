using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;

public static class Loader
{
    public static void LoadContext()
    {
        MLContext mlContext = new MLContext(seed: 1);

        Console.WriteLine("mlContext created...");

        const string DATASET_PATH = "./sensors_data.csv";
        IDataView data = mlContext.Data.LoadFromTextFile<ModelInput>(
            path: DATASET_PATH,
            hasHeader: true,
            separatorChar: ',');

        Console.WriteLine("dataset loaded...");

        var shuffledData = mlContext.Data.ShuffleRows(data, seed: 0);
        var split = mlContext.Data.TrainTestSplit(shuffledData, testFraction: 0.2);
        var trainingData = split.TrainSet;
        var testingData = split.TestSet;

        Console.WriteLine("training and testing dataset created...");
    }
}


