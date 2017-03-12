using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAnalyzer;

namespace TextAnalyzerTest
{
    [TestClass]
    public class MatrixUnitTest
    {
        [TestMethod]
        public void MultiplyTest()
        {
            Matrix matrix = new Matrix();

            int[,] a = new int[,] { { 1, 4 }, { 0, 3 } };
            int[,] b = new int[,] { { 0, 3, 0 }, { 2, 1, 4 } };

            int[,] ab = new int[,] { { 8, 7, 16 }, { 6, 3, 12 } };

            var output = matrix.Multiply(a, b);
            CollectionAssert.AreEqual(ab, output);
        }

        [TestMethod]
        public void TransposeTest()
        {
            Matrix matrix = new Matrix();

            double[,] a = new double[,] { { 1, 4 }, { 0, 3 } };

            double[,] aTranspose = new double[,] { { 1, 0 }, { 4, 3 } };

            double[,] output = matrix.Transpose(a);
            CollectionAssert.AreEqual(aTranspose, output);
        }

        [TestMethod]
        public void DivisionTest()
        {
            Matrix matrix = new Matrix();

            int[,] a = new int[,] { { 1, 4, 1 }, { 4, 5, 6 } };
            int[,] b = new int[,] { { 5, 2, 4 }, { 4, 1, 2 } };

            double[,] ab = new double[,] { { 0.2, 2, 0.25 }, { 1, 5, 3 } };

            double[,] output = matrix.SeqDivision(a, b);
            CollectionAssert.AreEqual(ab, output, output[0, 0].ToString());
        }

        [TestMethod]
        public void FactorizeTest()
        {
            Matrix matrix = new Matrix();

            double[,] a = new double[,] { { 1, 2, 3 }, { 4, 5, 6 } };
            double[,] b = new double[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } };

            double[,] ab = new double[,] { { 22, 28 }, { 49, 64 } };

            var abf = matrix.Factorize(matrix.Multiply(a, b));
            double[,] output = matrix.Multiply(abf.Item1, abf.Item2);
            for (int i = 0; i < output.GetLength(0); i++)
            {
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    output[i, j] = Math.Round(output[i, j]);
                }
            }

            CollectionAssert.AreEqual(ab, output);

        }
    }
}
