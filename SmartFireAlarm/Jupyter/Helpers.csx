using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive.Formatting;
using System.Diagnostics;
using System.Security;
using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

public static class Helpers
{
    public static PocketView GetPredictionPerClass(ModelOutput predicted, DataViewSchema schema)
    {
        var scoreEntries = GetScoreNames(schema).ToArray();

        var scores = new Dictionary<float, string>();
        for (int i = 0; i < predicted.Score.Length; i++)
        {
            scores.Add(predicted.Score[i], scoreEntries[i]);
        }

        var headers = new List<IHtmlContent>
        {
            th(b("Prediction per class")),
            th(b("Class"))
        };

        var rows = new List<List<IHtmlContent>>();
        foreach (var score in scores.OrderByDescending(s => s.Key))
        {
            var cells = new List<IHtmlContent>();
            cells.Add(td($"{score.Key:F4}"));
            cells.Add(td(score.Value));
            rows.Add(cells);
        }

        return table(
            thead(
                headers),
            tbody(
                rows.Select(
                    r => tr(r))));
    }

    public static (IDictionary<string, IEnumerable<float>> weights, IDictionary<string, float> biases) GetModelParameters(TransformerChain<MulticlassPredictionTransformer<LinearMulticlassModelParameters>> modelForContributions, string[] categories) 
    {
        var modelParameters = modelForContributions.Last() as MulticlassPredictionTransformer<LinearMulticlassModelParameters>;

        VBuffer<float>[] weights = default;
        modelParameters.Model.GetWeights(ref weights, out int numClasses);

        var weightsDictionary = new Dictionary<string, IEnumerable<float>>();
        var i = 0;
        foreach (var weight in weights)
        {
            weightsDictionary.Add(categories[i++], (weight as VBuffer<float>?).Value.DenseValues());
        }

        var biases = modelParameters.Model.GetBiases();
        var biasesDictionary = new Dictionary<string, float>();
        i = 0;
        foreach (var bias in biases)
        {
            biasesDictionary.Add(categories[i++], bias);
        }

        return (weightsDictionary, biasesDictionary);
    }

    private static IEnumerable<string> GetScoreNames(DataViewSchema schema)
    {
        var column = schema.GetColumnOrNull("Score");
        var slotNames = new VBuffer<ReadOnlyMemory<char>>();
        column.Value.GetSlotNames(ref slotNames);

        var names = new List<string>();

        foreach (var denseValue in slotNames.DenseValues())
        {
            names.Add(denseValue.ToString());
        }

        return names;
    }

    /// <summary>
    /// Compute Pearson correlation of the matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static double[,] GetPearsonCorrelation(List<List<double>> matrix) 
    {
        var length = matrix.Count();

        var z = new double[length, length];
        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < length - 1 - x; ++y)
            {
                var seriesA = matrix[x];
                var seriesB = matrix[length - 1 - y];

                var value = Correlation.Pearson(seriesA, seriesB);

                z[x, y] = value;
                z[length - 1 - y, length - 1 - x] = value;
            }

            z[x, length - 1 - x] = 1;
        }

        return z;
    }
}
