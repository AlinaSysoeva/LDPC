using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDPC
{
	class EncoderCPU
	{
		public async Task<int[,]> Encode(int[,] originalMatrix, int[,] parityMatrix, int gap)
		{
			int[,] result = new int[originalMatrix.GetLength(0), parityMatrix.GetLength(1)];

			for (int i = 0; i < originalMatrix.GetLength(0); i++)
			{
				int[] message = new int[originalMatrix.GetLength(1)];

				for (int j = 0; j < message.Length; j++)
				{
					message[j] = originalMatrix[i, j];
				}

				int[] encodedMessage = EncodeMessage(parityMatrix, message, gap, gap);

				for (int j = 0; j < result.GetLength(1); j++)
				{
					result[i, j] = encodedMessage[j];
				}
			}

			return result;
		}

		private int[] EncodeMessage(int[,] parityMatrix, int[] message, int gap, int oldGap)
		{
			int[,] p1 = null;

			int[,] messageArray = new int[1, message.Length];

			for (int i = 0; i < message.Length; i++)
			{
				messageArray[0, i] = message[i];
			}

			if (gap != 0)
			{
				int startRow = parityMatrix.GetLength(0) - gap;

				int[,] DMatrix = new int[gap, gap];

				int[,] CMatrix = new int[gap, parityMatrix.GetLength(1) - DMatrix.GetLength(1) - parityMatrix.GetLength(0) + gap];

				for (int i = startRow; i < parityMatrix.GetLength(0); i++)
				{
					for (int j = 0; j < parityMatrix.GetLength(1) - parityMatrix.GetLength(0) + gap; j++)
					{
						if (j < CMatrix.GetLength(1))
						{
							CMatrix[i % startRow, j] = parityMatrix[i, j];
						}
						else
						{
							DMatrix[i % startRow, j % CMatrix.GetLength(1)] = parityMatrix[i, j];
						}
					}
				}

				int[,] DCMatrix = MatrixOperation.LogicMultiplicateMatrixes(MatrixOperation.LogicGetInverseMatrix(DMatrix), CMatrix);

				p1 = MatrixOperation.Transponse(MatrixOperation.LogicMultiplicateMatrixes(DCMatrix, MatrixOperation.Transponse(messageArray)));
			}
			else
			{
				p1 = new int[1, oldGap];
			}

			int[,] BMatrix = new int[parityMatrix.GetLength(0) - gap, oldGap];

			int[,] TMatrix = new int[parityMatrix.GetLength(0) - gap, parityMatrix.GetLength(0) - gap];

			int[,] AMatrix = new int[BMatrix.GetLength(0), parityMatrix.GetLength(1) - BMatrix.GetLength(1) - TMatrix.GetLength(1)];

			for (int i = 0; i < AMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < parityMatrix.GetLength(1); j++)
				{
					if (j < AMatrix.GetLength(1))
					{
						AMatrix[i, j] = parityMatrix[i, j];
					}
					else if (j < AMatrix.GetLength(1) + BMatrix.GetLength(1))
					{
						BMatrix[i, j % AMatrix.GetLength(1)] = parityMatrix[i, j];
					}
					else
					{
						TMatrix[i, j % (AMatrix.GetLength(1) + BMatrix.GetLength(1))] = parityMatrix[i, j];
					}
				}
			}

			int[,] AuMatrix = MatrixOperation.LogicMultiplicateMatrixes(AMatrix, MatrixOperation.Transponse(messageArray));

			int[,] addedMatrixes = null;

			int[,] Bp1Matrix = MatrixOperation.LogicMultiplicateMatrixes(BMatrix, MatrixOperation.Transponse(p1));

			addedMatrixes = MatrixOperation.LogicAddUpMatrixes(AuMatrix, Bp1Matrix);

			int[,] p2 = MatrixOperation.Transponse(MatrixOperation.LogicMultiplicateMatrixes(MatrixOperation.LogicGetInverseMatrix(TMatrix), addedMatrixes));

			int[] codeword = new int[message.Length + p1.GetLength(1) + p2.GetLength(1)];

			for (int i = 0; i < codeword.Length; i++)
			{
				if (i < message.Length)
				{
					codeword[i] = message[i];
				}
				else if (i < message.Length + p1.GetLength(1))
				{
					codeword[i] = p1[0, i % message.Length];
				}
				else
				{
					codeword[i] = p2[0, i % (message.Length + p1.GetLength(1))];
				}
			}

			return codeword;
		}

		private int[,] RemoveLinearDependence(int[,] matrix, ref int gap)
		{
			int[,] tempMatrix = (int[,])matrix.Clone();

			List<int> linearDependentRows = new List<int>();

			for (int i = 0; i < tempMatrix.GetLength(0); i++)
			{
				int count = 0; 

				for (int j = 0; j < tempMatrix.GetLength(1); j++)
				{
					count += tempMatrix[i, j];
				}

				if (count == 0 || count == tempMatrix.GetLength(1))
				{
					linearDependentRows.Add(i);
				}
			}

			linearDependentRows.Reverse();

			foreach (var row in linearDependentRows)
			{
				if (row == tempMatrix.GetLength(0))
				{
					continue;
				}

				MatrixOperation.PermuteRow(tempMatrix, row, tempMatrix.GetLength(0)-1);
			}

			int[,] result = new int[tempMatrix.GetLength(0) - linearDependentRows.Count, tempMatrix.GetLength(1)];

			for (int i = 0; i < result.GetLength(0); i++)
			{
				for (int j = 0; j < tempMatrix.GetLength(1); j++)
				{
					result[i, j] = tempMatrix[i, j];
				}
			}

			gap -= linearDependentRows.Count;

			return result;
		}

		private int _wc;
		private int _wr;
	}
}
