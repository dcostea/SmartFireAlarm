using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public static class Helpers
{
    /// <summary>
    /// Compute Pearson correlation of the matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static double[,] GetZAxis(List<List<double>> matrix) 
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
