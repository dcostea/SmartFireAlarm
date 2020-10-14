using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;

public class ModelInput
{
    [LoadColumn(0)]
    public float Temperature { get; set; }

    [LoadColumn(1)]
    public float Luminosity { get; set; }

    [LoadColumn(2)]
    public float Infrared { get; set; }

    [LoadColumn(3)]
    public float Distance { get; set; }

    [LoadColumn(4)]
    public float PIR { get; set; }

    [LoadColumn(5)]
    public float Humidity { get; set; }

    [LoadColumn(6)]
    public string CreatedAt { get; set; }

    [ColumnName("Label"), LoadColumn(7)]
    public string Source { get; set; }
}

public class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; }

    [ColumnName("Score")]
    public float[] Score { get; set; }
}

public class CustomInputRow
{
    public string CreatedAt { get; set; }
}

public class CustomOutputRow
{
    public float Hour { get; set; }
    public float Day { get; set; }
}

[CustomMappingFactoryAttribute(nameof(CustomMappings.IncomeMapping))]
public class CustomMappings : CustomMappingFactory<CustomInputRow, CustomOutputRow>
{
    public static void IncomeMapping(CustomInputRow input, CustomOutputRow output)
    {
        output.Hour = DateTime.Parse(input.CreatedAt).Hour;
        output.Day = DateTime.Parse(input.CreatedAt).DayOfYear;
    }

    // This factory method will be called when loading the model to get the mapping operation.
    public override Action<CustomInputRow, CustomOutputRow> GetMapping()
    {
        return IncomeMapping;
    }
}

