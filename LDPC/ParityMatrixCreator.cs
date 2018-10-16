using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDPC
{
	static class ParityMatrixCreator
	{
		private class SetInfo
		{
			public SetInfo(int[,] matrix, int number)
			{
				Matrix = matrix;
				Number = number;
			}

			public int[,] Matrix
			{
				get;
				private set;
			}
			public int Number
			{
				get;
				private set;
			}
		}

		public static int Gap { get; private set; }

		public static async Task<int[,]> Create(int width, int height, int wr, int wc, float rate)
		{
			_wr = wr;
			_wc = wc;

			int[,] parityMatrix = await GetGallagerParityMatrix(width, (int)(height / rate));

			int gap;

			int[,] encodeMatrix = GetEncodeMatrix((int[,])parityMatrix.Clone(), out gap);

			Gap = gap;

			return encodeMatrix;
		}

		private static async Task<int[,]> GetGallagerParityMatrix(int rows, int columns)
		{
			int blocksCount = _wc;

			int[,] firstSetOfRows = new int[rows / blocksCount, columns];

			for (int i = 0; i < firstSetOfRows.GetLength(0); i++)
			{
				for (int j = i * _wr; j < (i + 1) * _wr; j++)
				{
					firstSetOfRows[i, j] = 1;
				}
			}



			Console.WriteLine("firstSetOfRows");

			MatrixOperation.ShowMatrix(firstSetOfRows);

			TaskFactory<SetInfo> taskFactory = new TaskFactory<SetInfo>();

			Task<SetInfo>[] tasks = new Task<SetInfo>[blocksCount];

			tasks[0] = taskFactory.StartNew(() => { return new SetInfo(firstSetOfRows, 0); });

			for (int i = 1; i < blocksCount; i++)
			{
				int counter = i;

				tasks[i] = taskFactory.StartNew(() => CreateSet(firstSetOfRows, counter));
			}

			SetInfo parityMatrixinfo = await taskFactory.ContinueWhenAll(tasks, completedTasks =>
			{

				int[,] parityCheckMatrix = new int[rows, columns];

				foreach (var task in completedTasks)
				{
					SetInfo set = task.Result;

					int matrixCounter = 0;

					for (int i = set.Matrix.GetLength(0) * set.Number; i < set.Matrix.GetLength(0) * (set.Number + 1); i++)
					{
						for (int j = 0; j < set.Matrix.GetLength(1); j++)
						{
							parityCheckMatrix[i, j] = set.Matrix[matrixCounter, j];
						}

						matrixCounter++;
					}
				}

				return new SetInfo(parityCheckMatrix, 0);
			});

			return parityMatrixinfo.Matrix;
		}

		private static SetInfo CreateSet(int[,] initialArray, int setNumber)
		{
			int[,] matrix = (int[,])initialArray.Clone();

			Random rand = new Random(DateTime.Now.Millisecond);

			int columns = initialArray.GetLength(1);

			int permutations = rand.Next(columns, columns * 2);

			for (int i = 0; i < permutations; i++)
			{
				int randColumn1 = rand.Next(columns);

				int randColumn2 = rand.Next(columns);

				ChangeColumns(matrix, randColumn1, randColumn2);
			}

			return new SetInfo(matrix, setNumber);
		}

		private static void ChangeColumns(int[,] matrix, int firstColumn, int secondColumn)
		{
			for (int j = 0; j < matrix.GetLength(0); j++)
			{
				int tmp = matrix[j, secondColumn];

				matrix[j, secondColumn] = matrix[j, firstColumn];

				matrix[j, firstColumn] = tmp;
			}
		}

		private static int[,] GetEncodeMatrix(int[,] parityMatrix, out int gap)
		{
			int[,] parityMatrixCopy = (int[,])parityMatrix.Clone();

			int permutations = 0;

			for (int i = 0; i < parityMatrixCopy.GetLength(0) - permutations; i++)
			{
				if (parityMatrixCopy[i, parityMatrixCopy.GetLength(1) - 1] == 1)
				{
					MatrixOperation.PermuteRow(parityMatrixCopy, i, parityMatrixCopy.GetLength(0) - 1);

					permutations++;

					i = 0;
				}
			}

			gap = permutations - 1;

			//  номер столбца, с которым мы работаем
			int p = parityMatrixCopy.GetLength(1) - 2;

			int j = 2;

			for (; ; )
			{
				int diagonalRow = parityMatrixCopy.GetLength(0) - (gap + j);

				if (diagonalRow < 0)
				{
					break;
				}

				int columnWithMinOnes = 0;
				int minOnesInColumn = Int32.MaxValue;

				for (int column = 0; column <= p; column++)
				{
					int onesInColumn = 0;

					for (int row = 0; row <= diagonalRow; row++)
					{
						if (parityMatrixCopy[row, column] == 1)
						{
							onesInColumn++;
						}
					}

					if (onesInColumn < minOnesInColumn && onesInColumn > 0)
					{
						minOnesInColumn = onesInColumn;
						columnWithMinOnes = column;
					}
				}

				MatrixOperation.PermuteColumn(parityMatrixCopy, columnWithMinOnes, p);

				int rowWithOne = 0;

				for (int row = 0; row <= diagonalRow; row++)
				{
					if (parityMatrixCopy[row, p] == 1)
					{
						rowWithOne = row;

						break;
					}
				}

				MatrixOperation.PermuteRow(parityMatrixCopy, rowWithOne, diagonalRow);

				if (minOnesInColumn > 1)
				{
					for (int i = 0; i < minOnesInColumn - 1; i++)
					{
						rowWithOne = 0;

						for (int row = 0; row < diagonalRow; row++)
						{
							if (parityMatrixCopy[row, p] == 1)
							{
								rowWithOne = row;

								break;
							}
						}

						MatrixOperation.PermuteRow(parityMatrixCopy, rowWithOne, parityMatrixCopy.GetLength(0) - 1);

						gap++;

						diagonalRow--;
					}
				}

				j++;
				p--;
			}

			int[,] gaussMatrix = GetGaussMatrix(parityMatrixCopy, gap);

			parityMatrixCopy = MatrixOperation.LogicMultiplicateMatrixes(gaussMatrix, parityMatrixCopy);



			Console.WriteLine("Проверочная");

			MatrixOperation.ShowMatrix(parityMatrixCopy);

			return parityMatrixCopy;
		}

		private static int[,] GetGaussMatrix(int[,] parityMatrix, int gap)
		{
			int[,] resultMatrix = new int[parityMatrix.GetLength(0), parityMatrix.GetLength(0)];

			int[,] EMatrix = new int[gap, parityMatrix.GetLength(0) - gap];

			int[,] TMatrix = new int[parityMatrix.GetLength(0) - gap, parityMatrix.GetLength(0) - gap];

			int matrixRow = 0;

			for (int row = 0; row < parityMatrix.GetLength(0); row++)
			{
				int matrixColumn = 0;

				for (int column = parityMatrix.GetLength(1) - TMatrix.GetLength(1); column < parityMatrix.GetLength(1); column++)
				{
					if (row < parityMatrix.GetLength(0) - gap)
					{
						TMatrix[row, matrixColumn] = parityMatrix[row, column];
					}
					else
					{
						EMatrix[matrixRow, matrixColumn] = parityMatrix[row, column];
					}

					matrixColumn++;
				}

				if (row >= parityMatrix.GetLength(0) - gap)
				{
					matrixRow++;
				}
			}

			int[,] ETMatrix = MatrixOperation.LogicMultiplicateMatrixes(EMatrix, MatrixOperation.LogicGetInverseMatrix(TMatrix));

			matrixRow = 0;

			for (int row = 0; row < resultMatrix.GetLength(0); row++)
			{
				for (int column = 0; column < resultMatrix.GetLength(1); column++)
				{
					if (row < (resultMatrix.GetLength(0) - gap))
					{
						if (row == column)
						{
							resultMatrix[row, column] = 1;
						}
					}
					else
					{
						if (column < resultMatrix.GetLength(0) - gap)
						{
							resultMatrix[row, column] = ETMatrix[matrixRow, column];
						}
						else
						{
							if (row == column)
							{
								resultMatrix[row, column] = 1;
							}
						}
					}
				}

				if (row >= (resultMatrix.GetLength(0) - gap))
				{
					matrixRow++;
				}
			}

			return resultMatrix;
		}

		private static int _wc;
		private static int _wr;
	}
}
