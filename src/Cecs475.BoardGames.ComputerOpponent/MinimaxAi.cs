using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.ComputerOpponent
{
	internal struct MinimaxBestMove
	{
		public long Weight { get; set; }
		public IGameMove Move { get; set; }
	}

	public class MinimaxAi : IGameAi
	{
		private int mMaxDepth;
		public MinimaxAi(int maxDepth)
		{
			mMaxDepth = maxDepth;
		}

		public IGameMove FindBestMove(IGameBoard b)
		{
			return FindBestMove(b,
				true ? long.MinValue : long.MaxValue,
				true ? long.MaxValue : long.MinValue,
				mMaxDepth).Move;
		}

		private static MinimaxBestMove FindBestMove(IGameBoard b, long alpha, long beta, int depthLeft)
		{
			if (depthLeft == 0 || b.IsFinished)
			{
				return new MinimaxBestMove()
				{
					Move = null,
					Weight = b.BoardWeight
				};
			}
			bool isMaximizing = (b.CurrentPlayer == 1) ? true : false;
			IGameMove bestMove = null;
			foreach (var move in b.GetPossibleMoves())
			{
				
				b.ApplyMove(move);
				var w = FindBestMove(b, alpha, beta, depthLeft - 1).Weight;
				b.UndoLastMove();
				if (isMaximizing && w > alpha)
				{
					alpha = w;
					bestMove = move;
				}
				else if (!isMaximizing && w < beta)
				{
					beta = w;
					bestMove = move;
				}
				if (alpha >= beta)
				{
					return new MinimaxBestMove
					{
						
						Weight = (isMaximizing) ? beta : alpha,
						Move = bestMove
					};
				}
			}
			return new MinimaxBestMove
			{
				Weight = (isMaximizing) ? alpha : beta,
				Move = bestMove
			};
		}

	}
}