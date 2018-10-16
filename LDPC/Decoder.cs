using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDPC
{
	class Decoder
	{
		public int[,] Decode(int[,] codewords, int[,] parityMatrix, int checksCount, float rank)
		{
			Console.WriteLine("Декодирование");

			int[,] decodedMessages = new int[codewords.GetLength(0), (int)(codewords.GetLength(1) * rank)];

			for (int row = 0; row < codewords.GetLength(0); row++)
			{
				Console.WriteLine("Строка " + row);

				int[] codeword = new int[codewords.GetLength(1)];

				for (int column = 0; column < codeword.Length; column++)
				{
					codeword[column] = codewords[row, column];
				}

				for (int i = 0; i < checksCount; i++)
				{
					Console.WriteLine("Проверка " + i);

					int[] originalMessage;

					bool isDecodedWithMisatakes = DecodeMessage(codeword, parityMatrix, rank, out originalMessage);

					if (isDecodedWithMisatakes)
					{
						if (i == checksCount - 1)
						{
							for (int j = 0; j < originalMessage.Length / 2; j++)
							{
								decodedMessages[row, j] = originalMessage[j];
							}
						}
						else
						{
							for (int j = 0; j < originalMessage.Length; j++)
							{
								codeword[j] = originalMessage[j];
							}
						}
					}
					else
					{
						for (int j = 0; j < originalMessage.Length; j++)
						{
							decodedMessages[row, j] = originalMessage[j];
						}

						break;
					}

					
				}
			}

			return decodedMessages;
		}

		private bool DecodeMessage(int[] codeword, int[,] parityMatrix, float rank, out int[] originalMessage)
		{
			int[] finalCodeWord = new int[codeword.Length];

			bool isSindromConfirmed = false;

			int[,] parityMatrixTransposed = MatrixOperation.Transponse(parityMatrix);

			int[] syndrome = MatrixOperation.LogicMultiplicateVectorMatrix(codeword, parityMatrixTransposed);

			for (int i = 0; i < syndrome.Length; i++)
			{
				if (syndrome[i] == 1)
				{
					isSindromConfirmed = true;

					break;
				}
			}

			Console.WriteLine("Синдром = " + isSindromConfirmed);

			if (!isSindromConfirmed)
			{
				originalMessage = new int[(int)(codeword.Length * rank)];

				for (int i = 0; i < originalMessage.Length; i++)
				{
					originalMessage[i] = codeword[i];
				}

				return isSindromConfirmed;
			}

			int[] F = MatrixOperation.MultiplicateVectorMatrix(syndrome, parityMatrix);

			int threshold = 0;

			int[] codewordCopy = (int[])codeword.Clone();

			for (int i = 0; i < F.Length; i++)
			{
				if (threshold < F[i])
				{
					threshold = F[i];
				}
			}

			Console.WriteLine("Порог " + threshold);

			for (int i = 0; i < F.Length; i++)
			{
				if (F[i] >= threshold)
				{
					Console.WriteLine("Исправление в {0} элементе", i);

					codewordCopy[i] = (codewordCopy[i] + 1) % 2;

					break;
				}
			}

			originalMessage = codewordCopy;

			Console.WriteLine("Новое кодовое слово");

			for (int i = 0; i < codewordCopy.Length; i++)
			{
				Console.Write(codeword[i] + " ");
			}
			Console.WriteLine();

			return isSindromConfirmed;
		}
	}
}
