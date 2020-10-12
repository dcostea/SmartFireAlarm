using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.Data.Analysis;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

public class Formatters
{
    public static string[] Categories { get; set; }

    public static void Register<T>()
    {
        switch (typeof(T))
        {
            case Type dfType when dfType == typeof(DataFrame):
                RegisterDataFrame();
                break;

            case Type cfType when cfType == typeof(ConfusionMatrix):
                RegisterConfusionMatrix();
                break;

            case Type mcmType when mcmType == typeof(MulticlassClassificationMetrics):
                RegisterClassificationMetrics();
                break;

            case Type lmcmType when lmcmType == typeof(List<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>>):
                RegisterListMulticlassClassificationMetrics();
                break;

            default:
                break;
        }
    }

    private static void RegisterDataFrame()
    {
        Formatter.Register<DataFrame>((df, writer) =>
        {
            var headers = new List<IHtmlContent>
            {
                th(i("index"))
            };
            headers.AddRange(df.Columns.Select(c => (IHtmlContent)th(c.Name)));
            var rows = new List<List<IHtmlContent>>();
            var take = 10;
            for (var i = 0; i < Math.Min(take, df.Rows.Count); i++)
            {
                var cells = new List<IHtmlContent>
                {
                    td(i)
                };
                foreach (var obj in df.Rows[i])
                {
                    cells.Add(td(obj));
                }
                rows.Add(cells);
            }

            var t = table(
                thead(
                    headers),
                tbody(
                    rows.Select(
                        r => tr(r))));

            writer.Write(t);
        }, "text/html");
    }

    private static void RegisterListMulticlassClassificationMetrics()
    {
        Formatter.Register<List<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>>>((crossValidationResults, writer) =>
        {
            var metricsInMultipleFolds = crossValidationResults.Select(r => r.Metrics);
            var microAccuracyValues = ExtractMetrics(metricsInMultipleFolds.Select(m => m.MicroAccuracy));
            var macroAccuracyValues = ExtractMetrics(metricsInMultipleFolds.Select(m => m.MacroAccuracy));
            var logLossValues = ExtractMetrics(metricsInMultipleFolds.Select(m => m.LogLoss));
            var logLossReductionValues = ExtractMetrics(metricsInMultipleFolds.Select(m => m.LogLossReduction));

            var headers = new List<IHtmlContent>
            {
                th(b("CROSS-VALIDATION: multi-class classification")),
                th(b("Average")),
                th(b("Standard deviation")),
                th(b("Confidence interval (95%)"))
            };

            var rows = new List<List<IHtmlContent>>();

            var cells = new List<IHtmlContent>
            {
                td(b("MacroAccuracy")),
                td($"{macroAccuracyValues.average:0.000}"),
                td($"{macroAccuracyValues.stdDev:0.000}"),
                td($"{macroAccuracyValues.confInt:0.000}")
            };
            rows.Add(cells);

            cells = new List<IHtmlContent>
            {
                td(b("MicroAccuracy")),
                td($"{microAccuracyValues.average:0.000}"),
                td($"{microAccuracyValues.stdDev:0.000}"),
                td($"{microAccuracyValues.confInt:0.000}")
            };
            rows.Add(cells);

            cells = new List<IHtmlContent>
            {
                td(b("LogLoss")),
                td($"{logLossValues.average:0.000}"),
                td($"{logLossValues.stdDev:0.000}"),
                td($"{logLossValues.confInt:0.000}")
            };
            rows.Add(cells);

            cells = new List<IHtmlContent>
            {
                td(b("LogLossReduction")),
                td($"{logLossReductionValues.average:0.000}"),
                td($"{logLossReductionValues.stdDev:0.000}"),
                td($"{logLossReductionValues.confInt:0.000}")
            };
            rows.Add(cells);

            var t = table(
                thead(
                    headers),
                tbody(
                    rows.Select(
                        r => tr(r))));
            writer.Write(t);
        }, "text/html");
    }

    private static void RegisterClassificationMetrics()
    {
        Formatter.Register<MulticlassClassificationMetrics>((m, writer) =>
        {
            if (Categories == null)
            {
                Categories = new string[m.PerClassLogLoss.Count];
                for (int i = 0; i < Categories.Count(); i++)
                {
                    Categories[i] = i.ToString();
                }
            }

            var oneMessage = "the closer to 1, the better";
            var zeroMessage = "the closer to 0, the better";

            var headers = new List<IHtmlContent>
            {
                th(b("EVALUATION: multi-class classification")),
                th(b("Class")),
                th(b("Value")),
                th(b("Note"))
            };

            var rows = new List<List<IHtmlContent>>();

            var cells = new List<IHtmlContent>
            {
                td(b("MacroAccuracy")),
                td(""),
                td($"{m.MacroAccuracy:0.000}"),
                td(oneMessage)
            };
            rows.Add(cells);

            cells = new List<IHtmlContent>
            {
                td(b("MicroAccuracy")),
                td(""),
                td($"{m.MicroAccuracy:0.000}"),
                td(oneMessage)
            };
            rows.Add(cells);

            cells = new List<IHtmlContent>
            {
                td(b("LogLoss")),
                td(""),
                td($"{m.LogLoss:0.000}"),
                td(zeroMessage)
            };
            rows.Add(cells);

            cells = new List<IHtmlContent>
            {
                td[rowspan: $"{m.PerClassLogLoss.Count + 1}"](b("LogLoss per Class"))
            };
            rows.Add(cells);

            for (int i = 0; i < m.PerClassLogLoss.Count; i++)
            {
                cells = new List<IHtmlContent>
                {
                    td($"{Categories[i]}"),
                    td($"{m.PerClassLogLoss[i]:0.000}"),
                    td(zeroMessage)
                };
                rows.Add(cells);
            }

            var t = table(
                thead(
                    headers),
                tbody(
                    rows.Select(
                        r => tr(r))));
            writer.Write(t);
        }, "text/html");
    }

    private static void RegisterConfusionMatrix()
    {
        Formatter.Register<ConfusionMatrix>((cm, writer) =>
        {
            var cssFirstColor = "background-color: lightsteelblue; ";
            var cssSecondColor = "background-color: #E3EAF3; ";
            var cssTransparent = "background-color: transparent";
            var cssBold = "font-weight: bold; ";
            var cssPadding = "padding: 8px; ";
            var cssCenterAlign = "text-align: center; ";
            var cssTable = "margin: 50px; ";
            var cssTitle = cssPadding + cssFirstColor;
            var cssHeader = cssPadding + cssBold + cssSecondColor;
            var cssCount = cssPadding;
            var cssFormula = cssPadding + cssSecondColor;

            var rows = new List<IHtmlContent>();

            // header
            var cells = new List<IHtmlContent>
            {
                td[rowspan: 2, colspan: 2, style: cssTitle + cssCenterAlign]("Confusion Matrix"),
                td[colspan: cm.Counts.Count, style: cssTitle + cssCenterAlign]("Predicted"),
                td[style: cssTitle]("")
            };
            rows.Add(tr[style: cssTransparent](cells));

            // features header
            cells = new List<IHtmlContent>();
            for (int j = 0; j < cm.Counts.Count; j++)
            {
                cells.Add(td[style: cssHeader](Categories.ToList()[j]));
            }
            rows.Add(tr[style: cssTransparent](cells));
            cells.Add(td[style: cssTitle]("Recall"));

            // values
            for (int i = 0; i < cm.NumberOfClasses; i++)
            {
                cells = new List<IHtmlContent>();
                if (i == 0)
                {
                    cells.Add(td[rowspan: cm.Counts.Count, style: cssTitle]("Truth"));
                }
                cells.Add(td[style: cssHeader](Categories.ToList()[i]));
                for (int j = 0; j < cm.NumberOfClasses; j++)
                {
                    cells.Add(td[style: cssCount](cm.Counts[i][j]));
                }
                cells.Add(td[style: cssFormula](Math.Round(cm.PerClassRecall[i], 4)));
                rows.Add(tr[style: cssTransparent](cells));
            }

            //footer
            cells = new List<IHtmlContent>
            {
                td[colspan: 2, style: cssTitle]("Precision")
            };
            for (int j = 0; j < cm.Counts.Count; j++)
            {
                cells.Add(td[style: cssFormula](Math.Round(cm.PerClassPrecision[j], 4)));
            }
            cells.Add(td[style: cssFormula]("total = " + cm.Counts.Sum(x => x.Sum())));
            rows.Add(tr[style: cssTransparent](cells));

            writer.Write(table[style: cssTable](tbody(rows)));
        }, "text/html");
    }

    private static (double average, double stdDev, double confInt) ExtractMetrics(IEnumerable<double> accuracyValues)
    {
        var average = accuracyValues.Average();
        var stdDev = CalculateStandardDeviation(accuracyValues);
        var confInt = CalculateConfidenceInterval95(accuracyValues);

        return (average, stdDev, confInt);
    }

    private static double CalculateStandardDeviation(IEnumerable<double> values)
    {
        double average = values.Average();
        double sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
        double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / (values.Count() - 1));

        return standardDeviation;
    }

    private static double CalculateConfidenceInterval95(IEnumerable<double> values)
    {
        double confidenceInterval95 = 1.96 * CalculateStandardDeviation(values) / Math.Sqrt(values.Count() - 1);

        return confidenceInterval95;
    }
}

