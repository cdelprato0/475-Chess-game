using System;
using System.Text;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.View;

namespace Cecs475.BoardGames.Chess.View {
	/// <summary>
	/// A chess game view for string-based console input and output.
	/// </summary>
	public class ChessConsoleView : IConsoleView {
		private static char[] LABELS = { '.', 'P', 'R', 'N', 'B', 'Q', 'K' };
		
		// Public methods.
		public string BoardToString(ChessBoard board) {
			StringBuilder str = new StringBuilder();

			for (int i = 0; i < ChessBoard.BoardSize; i++) {
				str.Append(8 - i);
				str.Append(" ");
				for (int j = 0; j < ChessBoard.BoardSize; j++) {
					var space = board.GetPieceAtPosition(new BoardPosition(i, j));
					if (space.PieceType == ChessPieceType.Empty)
						str.Append(". ");
					else if (space.Player == 1)
						str.Append($"{LABELS[(int)space.PieceType]} ");
					else
						str.Append($"{char.ToLower(LABELS[(int)space.PieceType])} ");
				}
				str.AppendLine();
			}
			str.AppendLine("  a b c d e f g h");
			return str.ToString();
		}

		/// <summary>
		/// Converts the given ChessMove to a string representation in the form
		/// "(start, end)", where start and end are board positions in algebraic
		/// notation (e.g., "a5").
		/// 
		/// If this move is a pawn promotion move, the selected promotion piece 
		/// must also be in parentheses after the end position, as in 
		/// "(a7, a8, Queen)".
		/// </summary>
		public string MoveToString(ChessMove move) {
			string strMove = "(";
			strMove += PositionToString(move.StartPosition);
			strMove += ", ";
			strMove += PositionToString(move.EndPosition);

			if (move.MoveType == ChessMoveType.Normal)
			{
				strMove += ")";
			}
			else
			{
				strMove += ", ";
				strMove += move.PieceType;
				strMove += ")";
			}
			return strMove;
		}

		public string PlayerToString(int player) {
			return player == 1 ? "White" : "Black";
		}

		/// <summary>
		/// Converts a string representation of a move into a ChessMove object.
		/// Must work with any string representation created by MoveToString.
		/// </summary>
		public ChessMove ParseMove(string moveText) {
			//gets rid of all the spaces in the string
			string noSpaces = moveText.Replace(" ", string.Empty);
			//trims the parenthesis off
			string trim = noSpaces.Trim(new char[] { '(', ')' });
			//sets to delimiter to the ,
			char delimiter = ',';		
			
			//stores the result of the split
			string[] result;
			//splits at the ,
			result = trim.Split(delimiter);

			//gets the position by parsing the posistion with the given result
			ChessMove move = new ChessMove(ParsePosition(result[0]), ParsePosition(result[1]));

			//gets the case for the pawn promotion
			if(result.Length >= 3){
				//converts to lower case
				result[2] = result[2].ToLower();
				ChessPieceType promote = ChessPieceType.Pawn;
				//determines which pawn promote to do
				if (result[2] == "queen")
				{
					promote = ChessPieceType.Queen;
				}
				else if (result[2] == "bishop")
				{
					promote = ChessPieceType.Bishop;
				}
				else if (result[2] == "knight")
				{
					promote = ChessPieceType.Knight;
				}
				else if (result[2] == "rook")
				{
					promote = ChessPieceType.Rook;
				}
				move = new ChessMove(ParsePosition(result[0]), ParsePosition(result[1]), promote);
			}
			return move;
		}

		public static BoardPosition ParsePosition(string pos) {
			return new BoardPosition(8 - (pos[1] - '0'), pos[0] - 'a');
		}

		public static string PositionToString(BoardPosition pos) {
			return $"{(char)(pos.Col + 'a')}{8 - pos.Row}";
		}

		#region Explicit interface implementations
		// Explicit method implementations. Do not modify these.
		string IConsoleView.BoardToString(IGameBoard board) {
			return BoardToString(board as ChessBoard);
		}

		string IConsoleView.MoveToString(IGameMove move) {
			return MoveToString(move as ChessMove);
		}

		IGameMove IConsoleView.ParseMove(string moveText) {
			return ParseMove(moveText);
		}
		#endregion
	}
}
