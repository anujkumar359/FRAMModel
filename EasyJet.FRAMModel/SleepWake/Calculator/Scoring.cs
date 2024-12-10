using EasyJet.FRAMModel.SleepWake.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyJet.FRAMModel.SleepWake.Calculator
{
    internal class Scoring
    {

        /// <summary>
        /// Calculates the score based on the provided data.
        /// </summary>
        /// <param name="dt">The data table containing the metrics.</param>
        /// <returns>An array of scores.</returns>
        public string[] CalculateScore(DataTable dt)
        {
            dt = dt.AsEnumerable()
           .OrderBy(row => row["RowIndex"]).CopyToDataTable();

            var scorers = new Dictionary<string, double>
                            {
                                { "FRAMWorkloadScore", 0.96 },
                                { "AlertnessBeforeDuty", 1.36 },
                                { "SumOfDiffs_cum_0.5", -0.91 },
                                { "AlertnessWhenSleep", 0.83 },
                                { "SleepLenghtBeforeOperating_cum_0.9", -1.85 }
                            };

            // Extract keys and values into separate lists
            var metrics = new List<string>(scorers.Keys);
            var weights = new List<double>(scorers.Values);

            int rowCount = dt.Rows.Count;
            int colCount = metrics.Count;
            double[,] dataArray = new double[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    dataArray[i, j] = Convert.ToDouble(dt.Rows[i][metrics[j]]);
                }
            }

            return CalculateDotProduct(dataArray, weights.ToArray());
        }

        /// <summary>
        /// Calculates the dot product of each row in a matrix with a vector.
        /// </summary>
        /// <param name="matrix">A 2D array representing the matrix. Each row is a data point, and each column is a feature.</param>
        /// <param name="vector">A 1D array representing the vector of weights. Its length must match the number of columns in the matrix.</param>
        /// <returns>A 1D array containing the dot product of each row of the matrix with the vector.</returns>
        /// <exception cref="ArgumentException">Thrown if the number of columns in the matrix does not match the length of the vector.</exception>
        private string[] CalculateDotProduct(double[,] matrix, double[] vector)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (vector.Length != cols)
                throw new ArgumentException("The number of columns in the matrix must match the size of the vector.");

            string[] result = new string[rows];

            for (int i = 0; i < rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < cols; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }
                result[i] = sum.ToString("N2");
            }

            return result;
        }
    }
}