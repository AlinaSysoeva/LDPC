using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LDPC
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Facade facade = new Facade();

			int[,] matrix = new int[28, 28];

			Random rand = new Random(0);

			for (int i = 0; i < matrix.GetLength(0); i++)
			{
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					matrix[i, j] = rand.Next(100) < 50 ? 0 : 1;
				}
			}

			try
			{
				int[,] encodedMatrix = facade.Encode(matrix, 0).Result;

				Console.WriteLine("Закодированная");

				MatrixOperation.ShowMatrix(encodedMatrix);

				facade.CreateMistakes(encodedMatrix);

				Console.WriteLine("Зашумленная");

				MatrixOperation.ShowMatrix(encodedMatrix);

				int[,] decodedMatrix = facade.Decode(encodedMatrix).Result;

				Console.WriteLine("Декодированная");

				MatrixOperation.ShowMatrix(decodedMatrix);

				Console.WriteLine("Исходная");

				MatrixOperation.ShowMatrix(matrix);

				for (int i = 0; i < decodedMatrix.GetLength(0); i++)
				{
					for (int j = 0; j < decodedMatrix.GetLength(1); j++)
					{
						if (decodedMatrix[i,j] != matrix[i, j])
						{
							Console.WriteLine("Различия в {0} строке {1} столбце", i, j);
						}
					}
				}
			}
			catch (Exception ex)
			{
				
			}

			System.Threading.Thread.Sleep(3600 * 1000);
		}
	}
}
