using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

public static class Formatters
{
    public static string[] Categories { get; set; }

    public static void Register<T>(object[] parameters = null)
    {
        switch (typeof(T))
        {
            case Type dfType when dfType == typeof(DataFrame):
                RegisterDataFrame();
                RegisterDataFrameColumn<SingleDataFrameColumn>();
                RegisterDataFrameColumn<StringDataFrameColumn>();
                Console.WriteLine("DataFrame formatter loaded.");
                break;

            case Type cfType when cfType == typeof(ConfusionMatrix):
                RegisterConfusionMatrix(parameters);
                break;

            case Type cfdwType when cfdwType == typeof(ConfusionMatrixDisplayView):
                RegisterConfusionMatrixDisplayView();
                break;

            case Type mcmType when mcmType == typeof(MulticlassClassificationMetrics):
                RegisterMulticlassClassificationMetrics(parameters);
                break;

            case Type mcmdwType when mcmdwType == typeof(MulticlassClassificationMetricsDisplayView):
                RegisterMulticlassClassificationMetricsDisplayView();
                break;

            case Type lmcmType when lmcmType == typeof(List<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>>):
                RegisterListMulticlassClassificationMetrics();
                break;

            default:
                break;
        }
    }

    private static void RegisterListMulticlassClassificationMetrics()
    {
        Formatter<List<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>>>.Register((crossValidationResults, writer) =>
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

        Console.WriteLine("List<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>> formatter loaded.");
    }

    private static void RegisterMulticlassClassificationMetrics(object[] parameters)
    {
        Formatter<MulticlassClassificationMetrics>.Register((m, writer) =>
        {
            if (parameters?.Length == m.PerClassLogLoss.Count)
            {
                string[] categories = new string[m.PerClassLogLoss.Count];

                for (int i = 0; i < parameters.Count(); i++)
                {
                    categories[i] = parameters[i].ToString();
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
                td($"{categories[i]}"),
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
            }
            else 
            {
                writer.Write($"The number of classes by Correlation Matrix ({m.PerClassLogLoss.Count}) does not match the number of categories argument ({parameters?.Length})");
            }
        }, "text/html");

        Console.WriteLine("MulticlassClassificationMetrics formatter loaded.");
    }

    private static void RegisterMulticlassClassificationMetricsDisplayView()
    {
        Formatter<MulticlassClassificationMetricsDisplayView>.Register((m, writer) =>
        {
            if (m.Categories?.Length == m.Metrics.PerClassLogLoss.Count)
            {
                string[] categories = new string[m.Metrics.PerClassLogLoss.Count];

                for (int i = 0; i < m.Categories.Count(); i++)
                {
                    categories[i] = m.Categories[i].ToString();
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
            td($"{m.Metrics.MacroAccuracy:0.000}"),
            td(oneMessage)
        };
                rows.Add(cells);

                cells = new List<IHtmlContent>
        {
            td(b("MicroAccuracy")),
            td(""),
            td($"{m.Metrics.MicroAccuracy:0.000}"),
            td(oneMessage)
        };
                rows.Add(cells);

                cells = new List<IHtmlContent>
        {
            td(b("LogLoss")),
            td(""),
            td($"{m.Metrics.LogLoss:0.000}"),
            td(zeroMessage)
        };
                rows.Add(cells);

                cells = new List<IHtmlContent>
        {
            td[rowspan: $"{m.Metrics.PerClassLogLoss.Count + 1}"](b("LogLoss per Class"))
        };
                rows.Add(cells);

                for (int i = 0; i < m.Metrics.PerClassLogLoss.Count; i++)
                {
                    cells = new List<IHtmlContent>
            {
                td($"{categories[i]}"),
                td($"{m.Metrics.PerClassLogLoss[i]:0.000}"),
                td(zeroMessage)
            };
                    rows.Add(cells);
                }

                var t = table(
                    thead(headers),
                    tbody(rows.Select(r => tr(r))));
                writer.Write(t);
            }
            else
            {
                writer.Write($"The number of classes by Correlation Matrix ({m.Metrics.PerClassLogLoss.Count}) does not match the number of categories argument ({m.Categories?.Length})");
            }
        }, "text/html");

        Console.WriteLine("MulticlassClassificationMetrics formatter loaded.");
    }

    private static void RegisterConfusionMatrixDisplayView()
    {
        Formatter<ConfusionMatrixDisplayView>.Register((cm, writer) =>
        {
            if (cm.Categories?.Length == cm.ConfusionMatrix.NumberOfClasses)
            {
                string[] categories = new string[cm.ConfusionMatrix.NumberOfClasses];

                for (int i = 0; i < cm.Categories.Count(); i++)
                {
                    categories[i] = cm.Categories[i].ToString();
                }

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
            td[colspan: cm.ConfusionMatrix.Counts.Count, style: cssTitle + cssCenterAlign]("Predicted"),
            td[style: cssTitle]("")
        };
                rows.Add(tr[style: cssTransparent](cells));

                // features header
                cells = new List<IHtmlContent>();
                for (int j = 0; j < cm.ConfusionMatrix.Counts.Count; j++)
                {
                    cells.Add(td[style: cssHeader](categories.ToList()[j]));
                }
                rows.Add(tr[style: cssTransparent](cells));
                cells.Add(td[style: cssTitle]("Recall"));

                // values
                for (int i = 0; i < cm.ConfusionMatrix.NumberOfClasses; i++)
                {
                    cells = new List<IHtmlContent>();
                    if (i == 0)
                    {
                        cells.Add(td[rowspan: cm.ConfusionMatrix.Counts.Count, style: cssTitle]("Truth"));
                    }
                    cells.Add(td[style: cssHeader](categories.ToList()[i]));
                    for (int j = 0; j < cm.ConfusionMatrix.NumberOfClasses; j++)
                    {
                        cells.Add(td[style: cssCount](cm.ConfusionMatrix.Counts[i][j]));
                    }
                    cells.Add(td[style: cssFormula](Math.Round(cm.ConfusionMatrix.PerClassRecall[i], 4)));
                    rows.Add(tr[style: cssTransparent](cells));
                }

                //footer
                cells = new List<IHtmlContent>
        {
            td[colspan: 2, style: cssTitle]("Precision")
        };
                for (int j = 0; j < cm.ConfusionMatrix.Counts.Count; j++)
                {
                    cells.Add(td[style: cssFormula](Math.Round(cm.ConfusionMatrix.PerClassPrecision[j], 4)));
                }
                cells.Add(td[style: cssFormula]("total = " + cm.ConfusionMatrix.Counts.Sum(x => x.Sum())));
                rows.Add(tr[style: cssTransparent](cells));

                writer.Write(table[style: cssTable](tbody(rows)));
            }
            else
            {
                writer.Write($"The number of classes in the Confusion Matrix ({cm.ConfusionMatrix.NumberOfClasses}) does not match the number of categories argument ({cm.Categories?.Length})");
            }

        }, "text/html");

        Console.WriteLine("ConfusionMatrix formatter loaded.");
    }

    private static void RegisterConfusionMatrix(object[] parameters)
    {
        Formatter<ConfusionMatrix>.Register((cm, writer) =>
        {
            if (parameters?.Length == cm.NumberOfClasses)
            {
                string[] categories = new string[cm.NumberOfClasses];

                for (int i = 0; i < parameters.Count(); i++)
                {
                    categories[i] = parameters[i].ToString();
                }

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
                    cells.Add(td[style: cssHeader](categories.ToList()[j]));
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
                    cells.Add(td[style: cssHeader](categories.ToList()[i]));
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
            }
            else
            {
                writer.Write($"The number of classes in the Confusion Matrix ({cm.NumberOfClasses}) does not match the number of categories argument ({parameters?.Length})");
            }

        }, "text/html");

        Console.WriteLine("ConfusionMatrix formatter loaded.");
    }

    private static void RegisterDataFrameColumn<T>() where T : DataFrameColumn
    {
        const int MAX = 10000;
        const int SIZE = 10;

        Formatter<T>.Register((columnRows, writer) =>
        {
            var uniqueId = DateTime.Now.Ticks;
            var maxRows = Math.Min(MAX, columnRows.Length);

            if (columnRows.Length > SIZE)
            {
                var maxMessage = columnRows.Length > MAX ? $" (showing a max of {MAX} rows)" : string.Empty;
                var title = h3[style: "text-align: center;"]($"DataFrame - {columnRows.Length} rows {maxMessage}");

                var header = BuildTableHeader(columnRows);

                // table body
                List<List<IHtmlContent>> rows = BuildTableBody(columnRows, maxRows);

                //navigator      
                List<IHtmlContent> footer = BuildNavigator(SIZE, uniqueId, maxRows);

                //table
                var t = table[id: $"table_{uniqueId}"](
                    caption(title),
                    thead(tr(header)),
                    tbody(rows.Select(r => tr[style: "display: none"](r))),
                    tfoot(tr(td[style: "text-align: center;"](footer)))
                );
                writer.Write(t);

                //show first page
                writer.Write($"<script>{BuildPageScript(uniqueId, SIZE)}</script>");
            }
            else
            {
                List<List<IHtmlContent>> rows = BuildTableBody(columnRows, maxRows);

                //table
                var t = table[id: $"table_{uniqueId}"](
                    thead(tr(header)),
                    tbody(rows.Select(r => tr(r)))
                );
                writer.Write(t);
            }
        }, "text/html");
    }

    private static List<IHtmlContent> BuildNavigator(int SIZE, long uniqueId, long maxRows)
    {
        var footer = new List<IHtmlContent>();
        BuildHideRowsScript(uniqueId);

        var paginateScriptFirst = BuildHideRowsScript(uniqueId) + GotoPageIndex(uniqueId, 0) + BuildPageScript(uniqueId, SIZE);
        footer.Add(button[style: "margin: 2px;", onclick: paginateScriptFirst]("⏮"));

        var paginateScriptPrevTen = BuildHideRowsScript(uniqueId) + UpdatePageIndex(uniqueId, -10, (maxRows - 1) / SIZE) + BuildPageScript(uniqueId, SIZE);
        footer.Add(button[style: "margin: 2px;", onclick: paginateScriptPrevTen]("⏪"));

        var paginateScriptPrev = BuildHideRowsScript(uniqueId) + UpdatePageIndex(uniqueId, -1, (maxRows - 1) / SIZE) + BuildPageScript(uniqueId, SIZE);
        footer.Add(button[style: "margin: 2px;", onclick: paginateScriptPrev]("◀️"));

        footer.Add(b[style: "margin: 2px;"]("Page"));
        footer.Add(b[id: $"page_{uniqueId}", style: "margin: 2px;"]("1"));

        var paginateScriptNext = BuildHideRowsScript(uniqueId) + UpdatePageIndex(uniqueId, 1, (maxRows - 1) / SIZE) + BuildPageScript(uniqueId, SIZE);
        footer.Add(button[style: "margin: 2px;", onclick: paginateScriptNext]("▶️"));

        var paginateScriptNextTen = BuildHideRowsScript(uniqueId) + UpdatePageIndex(uniqueId, 10, (maxRows - 1) / SIZE) + BuildPageScript(uniqueId, SIZE);
        footer.Add(button[style: "margin: 2px;", onclick: paginateScriptNextTen]("⏩"));

        var paginateScriptLast = BuildHideRowsScript(uniqueId) + GotoPageIndex(uniqueId, (maxRows - 1) / SIZE) + BuildPageScript(uniqueId, SIZE);
        footer.Add(button[style: "margin: 2px;", onclick: paginateScriptLast]("⏭️"));
        return footer;
    }

    private static List<List<IHtmlContent>> BuildTableBody(DataFrameColumn columnRows, long maxRows)
    {
        var rows = new List<List<IHtmlContent>>();
        for (var index = 0; index < maxRows; index++)
        {
            var cells = new List<IHtmlContent>
                {
                    td(i((index)))
                };
            cells.Add(td(columnRows[index]));
            rows.Add(cells);
        }

        return rows;
    }

    private static List<IHtmlContent> BuildTableHeader(DataFrameColumn columnRows)
    {
        var header = new List<IHtmlContent>
            {
                th(i("index"))
            };
        header.Add((IHtmlContent)th(columnRows.Name));
        return header;
    }

    private static void RegisterDataFrame()
    {
        const int MAX = 10000;
        const int SIZE = 10;

        Formatter<DataFrame>.Register((df, writer) =>
        {
            var uniqueId = DateTime.Now.Ticks;
            var maxRows = Math.Min(MAX, df.Rows.Count);

            var header = new List<IHtmlContent>
            {
                th(i("index"))
            };
            header.AddRange(df.Columns.Select(c => (IHtmlContent)th(c.Name)));

            if (df.Rows.Count > SIZE)
            {
                var maxMessage = df.Rows.Count > MAX ? $" (showing a max of {MAX} rows)" : string.Empty;
                var title = h3[style: "text-align: center;"]($"DataFrame - {df.Rows.Count} rows {maxMessage}");

                // table body
                var rows = new List<List<IHtmlContent>>();
                for (var index = 0; index < maxRows; index++)
                {
                    var cells = new List<IHtmlContent>
                    {
                        td(i((index)))
                    };
                    foreach (var obj in df.Rows[index])
                    {
                        cells.Add(td(obj));
                    }
                    rows.Add(cells);
                }

                //navigator      
                var footer = BuildNavigator(SIZE, uniqueId, maxRows);

                //table
                var t = table[id: $"table_{uniqueId}"](
                    caption(title),
                    thead(tr(header)),
                    tbody(rows.Select(r => tr[style: "display: none"](r))),
                    tfoot(tr(td[colspan: df.Columns.Count + 1, style: "text-align: center;"](footer)))
                );
                writer.Write(t);

                //show first page
                writer.Write($"<script>{BuildPageScript(uniqueId, SIZE)}</script>");
            }
            else
            {
                var rows = new List<List<IHtmlContent>>();
                for (var index = 0; index < df.Rows.Count; index++)
                {
                    var cells = new List<IHtmlContent>
                    {
                        td(i((index)))
                    };
                    foreach (var obj in df.Rows[index])
                    {
                        cells.Add(td(obj));
                    }
                    rows.Add(cells);
                }

                //table
                var t = table[id: $"table_{uniqueId}"](
                    thead(tr(header)),
                    tbody(rows.Select(r => tr(r)))
                );
                writer.Write(t);
            }
        }, "text/html");
    }

    private static string BuildHideRowsScript(long uniqueId)
    {
        var script = $"var allRows = document.querySelectorAll('#table_{uniqueId} tbody tr:nth-child(n)'); ";
        script += "for (let i = 0; i < allRows.length; i++) { allRows[i].style.display='none'; } ";
        return script;
    }

    private static string BuildPageScript(long uniqueId, int size)
    {
        var script = $"var page = parseInt(document.querySelector('#page_{uniqueId}').innerHTML) - 1; ";
        script += $"var pageRows = document.querySelectorAll(`#table_{uniqueId} tbody tr:nth-child(n + ${{page * {size} + 1 }})`); ";
        script += $"for (let j = 0; j < {size}; j++) {{ pageRows[j].style.display='table-row'; }} ";
        return script;
    }

    private static string GotoPageIndex(long uniqueId, long page)
    {
        var script = $"document.querySelector('#page_{uniqueId}').innerHTML = {page + 1}; ";
        return script;
    }

    private static string UpdatePageIndex(long uniqueId, int step, long maxPage)
    {
        var script = $"var page = parseInt(document.querySelector('#page_{uniqueId}').innerHTML) - 1; ";
        script += $"page = parseInt(page) + parseInt({step}); ";
        script += $"page = page < 0 ? 0 : page; ";
        script += $"page = page > {maxPage} ? {maxPage} : page; ";
        script += $"document.querySelector('#page_{uniqueId}').innerHTML = page + 1; ";
        return script;
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
