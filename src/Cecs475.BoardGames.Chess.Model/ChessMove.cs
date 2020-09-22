using System;
using Cecs475.BoardGames.Model;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents a single move to be applied to a chess board.
	/// </summary>
	public class ChessMove : IGameMove, IEquatable<ChessMove> {
		// You can add additional fields, properties, and methods as you find
		// them necessary, but you cannot MODIFY any of the existing implementations.

		/// <summary>
		/// The starting position of the move.
		/// </summary>
		public BoardPosition StartPosition { get; }

		/// <summary>
		/// The ending position of the move.
		/// </summary>
		public BoardPosition EndPosition { get; }

		/// <summary>
		/// The type of move being applied.
		/// </summary>
		public ChessMoveType MoveType { get; }

		/// <summary>
		/// The type of piece being move
		/// </summary>
		public ChessPieceType PieceType { get; set; }

		/// <summary>
		/// keeps track of the captured piece
		/// </summary>
		public ChessPiece Captured { get; set; }

		/// <summary>
		/// if the piece has been captured
		/// </summary>
		public bool IsCaptured { get; set; }

		/// <summary>
		/// if the piece has been promoted
		/// </summary>
		public ChessPieceType PawnPromoted { get; set; }

		// You must set this property when applying a move.
		public int Player { get; set; }

		/// <summary>
		/// Constructs a ChessMove that moves a piece from one position to another
		/// </summary>
		/// <param name="start">the starting position of the piece to move</param>
		/// <param name="end">the position where the piece will end up</param>
		/// <param name="moveType">the type of move represented</param>
		public ChessMove(BoardPosition start, BoardPosition end, ChessMoveType moveType = ChessMoveType.Normal) {
			StartPosition = start;
			EndPosition = end;
			MoveType = moveType;
		}

		/// <summary>
		/// Constructs a ChessMove that moves a piece from one position to another with a pawn promotion
		/// </summary>
		/// <param name="start">the starting position of the piece to move</param>
		/// <param name="end">the position where the piece will end up</param>
		/// <param name="pieceType">the type of the piece, used for pawn promotion</param>
		public ChessMove(BoardPosition start, BoardPosition end, ChessPieceType pieceType){
			StartPosition = start;
			EndPosition = end;
			MoveType = ChessMoveType.PawnPromote;
			PawnPromoted = pieceType;
			PieceType = pieceType;
		}

		
		public virtual bool Equals(ChessMove other) {
			// Most chess moves are equal to each other if they have the same start and end position.
			// PawnPromote moves must also be promoting to the same piece type.

			//returns false if the start positions are not the same
			if (!StartPosition.Equals(other.StartPosition))
			{
				return false;
			}
			//if end positions are not the same
			if (!EndPosition.Equals(other.EndPosition))
			{
				return false;
			}
			//if the move is a pawn promote
			if(MoveType == ChessMoveType.PawnPromote){
				//then checks if the two pieces are the same
				if (!PawnPromoted.Equals(other.PawnPromoted)){
					return false;
				}
			}
			return true;
		}

		// Equality methods.
		bool IEquatable<IGameMove>.Equals(IGameMove other) {
			ChessMove m = other as ChessMove;
			return this.Equals(m);
		}

		public override bool Equals(object other) {
			return Equals(other as ChessMove);
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = StartPosition.GetHashCode();
				hashCode = (hashCode * 397) ^ EndPosition.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)MoveType;
				return hashCode;
			}
		}

		public override string ToString() {
			return $"{StartPosition} to {EndPosition}";
		}
	}
}
