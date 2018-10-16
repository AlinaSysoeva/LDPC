using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDPC
{
	class Facade
	{
		public Facade()
		{
			_rank = 0.5f;
		}

		public async Task<int[,]> Encode(int[,] matrix, int rank)
		{
			if (_parityMatrix == null)
			{
				_parityMatrix = await ParityMatrixCreator.Create(matrix.GetLength(0), matrix.GetLength(1), 8, 4, _rank);
			}

			Console.WriteLine("Проверочная");

			MatrixOperation.ShowMatrix(_parityMatrix);

			return await new EncoderCPU().Encode(matrix, _parityMatrix, ParityMatrixCreator.Gap);
		}

		public void CreateMistakes(int[,] matrix)
		{
			Random rand = new Random(DateTime.Now.Millisecond);

			for (int i = 0; i < matrix.GetLength(0) * 2; i++)
			{
				int randomRow = rand.Next(matrix.GetLength(0));
				int randomColumn = rand.Next(matrix.GetLength(1));

				matrix[randomRow, randomColumn] = matrix[randomRow, randomColumn] == 1 ? 0 : 1;

				Console.WriteLine("Зашумил в {0} строке {1} столбце", randomRow, randomColumn);
			}
		}

		public async Task<int[,]> Decode(int[,] encodedMatrix)
		{
			if (_parityMatrix == null)
			{
				throw new ArgumentNullException("Не задана проверочная матрица для декодирования");
			}

			return new Decoder().Decode(encodedMatrix, _parityMatrix, 10, _rank);
		}

		private int[,] _parityMatrix;
		private float _rank;
	}
}
