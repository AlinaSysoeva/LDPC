using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDPC
{
	interface IEncoder
	{
		Task<int[,]> Encode(int[,] matrix, int wr, int wc, float rate);
	}
}
