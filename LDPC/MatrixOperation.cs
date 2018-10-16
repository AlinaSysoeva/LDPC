using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDPC
{
	static class MatrixOperation
	{
		public static void PermuteColumn(int[,] matrix, int from, int to)
		{
			if (from == to)
			{
				return;
			}

			if (from > to || from < 0 || to < 0 || from > matrix.GetLength(1) - 1 || to > matrix.GetLength(1) - 1)
			{
				throw new ArgumentException("Неправильное значение to или from.");
			}

			int[] column = new int[matrix.GetLength(0)];

			for (int i = 0; i < column.Length; i++)
			{
				column[i] = matrix[i, from];
			}

			for (int j = from + 1; j <= to; j++)
			{
				for (int i = 0; i < matrix.GetLength(0); i++)
				{
					matrix[i, j - 1] = matrix[i, j];
				}
			}

			for (int i = 0; i < column.Length; i++)
			{
				matrix[i, to] = column[i];
			}
		}

		public static void PermuteRow(int[,] matrix, int from, int to)
		{
			if (from == to)
			{
				return;
			}

			if (from > to || from < 0 || to < 0 || from > matrix.GetLength(0) - 1 || to > matrix.GetLength(0) - 1)
			{
				throw new ArgumentException("Неправильное значение to или from.");
			}

			int[] row = new int[matrix.GetLength(1)];

			for (int i = 0; i < row.Length; i++)
			{
				row[i] = matrix[from, i];
			}

			for (int i = from + 1; i <= to; i++)
			{
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					matrix[i - 1, j] = matrix[i, j];
				}
			}

			for (int i = 0; i < row.Length; i++)
			{
				matrix[to, i] = row[i];
			}
		}

		public static int[,] LogicMultiplicateMatrixes(int[,] firstMatrix, int[,] secondMatrix)
		{
			if (firstMatrix.GetLength(1) != secondMatrix.GetLength(0))
			{
				throw new ArgumentException("Матрицы невозможно перемножить.");
			}

			int[,] resultMatrix = new int[firstMatrix.GetLength(0), secondMatrix.GetLength(1)];

			for (int i = 0; i < firstMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < secondMatrix.GetLength(1); j++)
				{
					for (int k = 0; k < secondMatrix.GetLength(0); k++)
					{
						resultMatrix[i, j] ^= firstMatrix[i, k] & secondMatrix[k, j];
					}
				}
			}

			return resultMatrix;
		}

		public static int[,] LogicGetInverseMatrix(int[,] matrix)
		{
			if (matrix.GetLength(0) != matrix.GetLength(1))
			{
				throw new ArgumentException("Обратная матрица может вычисляться только для квадратной");
			}

			int[,] result = new int[matrix.GetLength(0), matrix.GetLength(1) * 2];

			for (int row = 0; row < result.GetLength(0); row++)
			{
				for (int column = 0; column < result.GetLength(1); column++)
				{
					if (column < matrix.GetLength(1))
					{
						result[row, column] = matrix[row, column];
					}
					else
					{
						if (row == column % matrix.GetLength(1))
						{
							result[row, column] = 1;
						}
					}
				}
			}

			for (int column = 0; column < matrix.GetLength(1); column++)
			{
				if (result[column, column] != 1)
				{
					for (int row = 0; row < result.GetLength(0); row++)
					{
						if (result[row, column] == 1)
						{
							LogicMatrixAddUpRows(result, row, column);

							break;
						}
					}
				}

				for (int row = 0; row < result.GetLength(0); row++)
				{
					if (row == column)
					{
						continue;
					}

					if (result[row, column] == 1)
					{
						LogicMatrixAddUpRows(result, column, row);
					}
				}
			}

			int[,] inverseMatrix = new int[matrix.GetLength(0), matrix.GetLength(1)];

			for (int row = 0; row < matrix.GetLength(0); row++)
			{
				for (int column = result.GetLength(1) / 2; column < result.GetLength(1); column++)
				{
					inverseMatrix[row, column % matrix.GetLength(1)] = result[row, column];
				}
			}

			return inverseMatrix;
		}

		public static void LogicMatrixAddUpRows(int[,] matrix, int rowToAdd, int objectiveRow)
		{
			for (int column = 0; column < matrix.GetLength(1); column++)
			{
				matrix[objectiveRow, column] ^= matrix[rowToAdd, column];
			}
		}

		public static int[,] LogicAddUpMatrixes(int[,] firstMatrix, int[,] secondMatrix)
		{
			if (firstMatrix.GetLength(0) != secondMatrix.GetLength(0) || firstMatrix.GetLength(1) != secondMatrix.GetLength(1))
			{
				throw new ArgumentException("Матрицы не совпадают!");
			}

			int[,] result = new int[firstMatrix.GetLength(0), firstMatrix.GetLength(1)];

			for (int i = 0; i < firstMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < firstMatrix.GetLength(1); j++)
				{
					result[i, j] = firstMatrix[i, j] ^ secondMatrix[i, j];
				}
			}

			return result;
		}

		public static int[] LogicMultiplicateVectorMatrix(int[] vector, int[,] matrix)
		{
			if (vector.Length != matrix.GetLength(0))
			{
				throw new ArgumentException("Матрицы невозможно перемножить.");
			}

			int[] result = new int[matrix.GetLength(1)];

			for (int i = 0; i < matrix.GetLength(1); i++)
			{
				for (int j = 0; j < matrix.GetLength(0); j++)
				{
					result[i] ^= vector[j] & matrix[j, i];
				}
			}

			return result;
		}

		public static int[] MultiplicateVectorMatrix(int[] vector, int[,] matrix)
		{
			if (vector.Length != matrix.GetLength(0))
			{
				throw new ArgumentException("Матрицы невозможно перемножить.");
			}

			int[] result = new int[matrix.GetLength(1)];

			for (int i = 0; i < matrix.GetLength(1); i++)
			{
				for (int j = 0; j < matrix.GetLength(0); j++)
				{
					result[i] += vector[j] * matrix[j, i];
				}
			}

			return result;
		}

		public static int[,] Transponse(int[,] matrix)
		{
			int[,] result = new int[matrix.GetLength(1), matrix.GetLength(0)];

			for (int i = 0; i < matrix.GetLength(0); i++)
			{
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					result[j, i] = matrix[i, j];
				}
			}

			return result;
		}

		public static void ShowMatrix(int[,] matrix)
		{
			Console.Write(GetSpacesString(matrix.GetLength(0).ToString().Length + 1));

			for (int i = 0; i < matrix.GetLength(1); i++)
			{
				Console.Write(i + " ");
			}

			Console.WriteLine();

			for (int i = 0; i < matrix.GetLength(0); i++)
			{
				Console.Write(i + GetSpacesString(matrix.GetLength(0).ToString().Length - i.ToString().Length + 1));

				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					Console.Write(matrix[i, j] + GetSpacesString(j.ToString().Length));
				}

				Console.WriteLine();
			}

			Console.WriteLine();
		}

		public static string GetSpacesString(int spacesCount)
		{
			string result = "";

			for (int i = 0; i < spacesCount; i++)
			{
				result += " ";
			}

			return result;
		}
	}
}
