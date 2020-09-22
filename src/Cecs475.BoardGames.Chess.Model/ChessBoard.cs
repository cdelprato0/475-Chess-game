//Chaz Del Prato
//Christine Doung
using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Model;
using System.Linq;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents the board state of a game of chess. Tracks which squares of the 8x8 board are occupied
	/// by which player's pieces.
	/// </summary>
	public class ChessBoard : IGameBoard 
	{
		#region Member fields.
		// The history of moves applied to the board.
		private List<ChessMove> mMoveHistory = new List<ChessMove>();

		public const int BoardSize = 8;
		
		//creates a virutal board of the initial state of the chess board.
		//uses hex to represent two squares
		private byte[] cboard = new byte[32] {
			0xAB, //10101011 black rook, black knight RANK 8
			0xCD, //11001101 black bishop, black queen RANK 8
			0xEC, //11101100 black king, black bishop RANK 8
			0xBA, //10111010 black knight, black rook RANK 8
			0x99, //10011001 black pawn, black pawn RANK 7
			0x99, //10011001 black pawn, black pawn RANK 7
			0x99, //10011001 black pawn, black pawn RANK 7
			0x99, //10011001 black pawn, black pawn RANK 7
			0x00, //00000000 empty square, empty square RANK 6
			0x00, //00000000 empty square, empty square RANK 6
			0x00, //00000000 empty square, empty square RANK 6 
			0x00, //00000000 empty square, empty square RANK 6
			0x00, //00000000 empty square, empty square RANK 5
			0x00, //00000000 empty square, empty square RANK 5
			0x00, //00000000 empty square, empty square RANK 5
			0x00, //00000000 empty square, empty square RANK 5
			0x00, //00000000 empty square, empty square RANK 4
			0x00, //00000000 empty square, empty square RANK 4
			0x00, //00000000 empty square, empty square RANK 4
			0x00, //00000000 empty square, empty square RANK 4
			0x00, //00000000 empty square, empty square RANK 3
			0x00, //00000000 empty square, empty square RANK 3
			0x00, //00000000 empty square, empty square RANK 3
			0x00, //00000000 empty square, empty square RANK 3
			0x11, //00010001 white pawn, white pawn RANK 2
			0x11, //00010001 white pawn, white pawn RANK 2
			0x11, //00010001 white pawn, white pawn RANK 2
			0x11, //00010001 white pawn, white pawn RANK 2
			0x23, //00100011 white rook, white knight RANK 1
			0x45, //01000101 white bishop, white queen RANK 1
			0x64, //01100100 white king, white bishop RANK 1
			0x32, //00110010 white knight, white rook RANK 1
		};

		/// <summary>
		/// Property used to check if the game is in check. Check is when a king is under attack.
		/// </summary>
		private bool GameInCheck { get; set; }
		/// <summary>
		/// Property used to check if the game is finished
		/// </summary>
		private bool GameIsFinished { get; set; }
		/// <summary>
		/// Property used to keep a count on the amount of plays that have been made with out any
		/// progress.
		/// </summary>
		private List<int> DrawCount { get; set; }
		/// <summary>
		/// Property used to keep track of the current player
		/// </summary>
		private int currentPlayer { get; set; }
		/// <summary>
		/// Property used to keep track of the current advantage on the board
		/// </summary>
		private GameAdvantage Advantage { get; set; }

		#endregion

		#region Properties.
		// You can choose to use auto properties, computed properties, or normal properties 
		// using a private field to back the property.

		// You can add set bodies if you think that is appropriate, as long as you justify
		// the access level (public, private).
		/// <summary>
		/// Will check the board and see if the game is finished
		/// </summary>
		public bool IsFinished { 
			get {
				if (IsStalemate)
				{
					return true;
				}
				if (IsDraw)
				{
					return true;
				}
				return GameIsFinished; 
			} 
		}

		/// <summary>
		/// Will return the current player
		/// </summary>
		public int CurrentPlayer { 
			get {
				return currentPlayer;
			} 
		}

		/// <summary>
		/// Will return the current advantage of the game
		/// </summary>
		public GameAdvantage CurrentAdvantage { get { return Advantage; } }
		
		/// <summary>
		/// Returns a read only list of Move History
		/// </summary>
		public IReadOnlyList<ChessMove> MoveHistory => mMoveHistory;
		
		/// <summary>
		/// Will check the board and see if the game is in check
		/// </summary>
		public bool IsCheck {
			get
			{
				//temp variable to return if the game is in check
				bool isInCheck = false;
				//get the enemy at the current moment
				int enemy = currentPlayer % 2 + 1;
				//gets the positions of all the kings on the board for the current player (should only be one)
				var listOfKingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
				//returns the first king position, which is the only king for that player
				BoardPosition kingPos = listOfKingPos.First();
				//if out of all the attacked positions in the enemies and our king is in there
				if (GetAttackedPositions(enemy).Contains(kingPos))
				{
					//the games state is in check
					isInCheck = true;
				}
				//returns true if the game is in check and there are at least one possible move to get out of check
				return isInCheck && GetPossibleMoves().Count() >= 1;
			}
		}

		/// <summary>
		/// Checks the board and determines if the state is in check mate.
		/// Game is over.
		/// </summary>
		public bool IsCheckmate {
			get
			{
				bool isInCheck = false;
				int enemy = currentPlayer % 2 + 1;
				var listOfKingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
				BoardPosition kingPos = listOfKingPos.First();
				if (GetAttackedPositions(enemy).Contains(kingPos))
				{
					isInCheck = true;
				}
				//returns true if the game is in check and there are no possible way to save the king
				return isInCheck && GetPossibleMoves().Count() == 0;
			}
		}

		/// <summary>
		/// Checks the board and determines if the state is in a stalemate.
		/// Game is over.
		/// </summary>
		public bool IsStalemate
		{
			get
			{
				bool inCheck = false;
				int enemy = currentPlayer%2+1;
				var listOfKingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
				BoardPosition kingPos = listOfKingPos.First();
				if (GetAttackedPositions(enemy).Contains(kingPos))
				{
					inCheck = true;
				}
				//returns true if the game is not in check and that there are no more possible moves
				return !inCheck && GetPossibleMoves().Count() == 0;
			}
		}

		/// <summary>
		/// Checks to see if the counter is greater than or equal to 50 moves.
		/// </summary>
		public bool IsDraw {
			get { return DrawCount.Last() >= 50; }
		}
		
		/// <summary>
		/// Tracks the current draw counter, which goes up by 1 for each non-capturing, non-pawn move, and resets to 0
		/// for other moves. If the counter reaches 100 (50 full turns), the game is a draw.
		/// </summary>
		public int DrawCounter {
			get { return DrawCount.Last(); }
		}
		#endregion


		#region Public methods.
		/// <summary>
		/// Gets all of the possible moves that a player has.
		/// </summary>
		public IEnumerable<ChessMove> GetPossibleMoves() {
			//creates a list of all possible moves
			var validMoves = new List<ChessMove>();
			//gets the current player
			int player = CurrentPlayer;
			//determines the enemy player
			int enemy = player % 2 + 1;
			//gets the state of the board before this players turn
			bool inCheckBefore = GameInCheck;
			//gets all the possible moves for the Rooks for that player
			var allRookMoves = GetPossibleRookAttacks(player);
			//Knights
			var allKnightMoves = GetPossibleKnightAttacks(player);
			//Bishops
			var allBishopMoves = GetPossibleBishopAttacks(player);
			//Queens
			var allQueenMoves = GetPossibleQueenAttacks(player);
			//King
			var allKingMoves = GetPossibleKingAttacks(player);
			//Pawn moves
			var allPawnMoves = GetAllPawnMoves(player);
			//Pawn Attack moves
			var allPawnAttackMoves = GetPossiblePawnAttacks(player);
			//gets the current posistion of the players king
			var kingPos = GetPositionsOfPiece(ChessPieceType.King, player);
			//possible moves for king castling (king or queen side
			var kingCastles = KingCastlesMoves(kingPos.First(), player);

			//Knight Moves
			foreach (var move in allKnightMoves)
			{
				ApplyMove(move);
				if (inCheckBefore)
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
				else
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
			}

			//For each move inside of the vaild pawn attack moves
			foreach (var move in allPawnAttackMoves)
			{
				//Apply the move to check the following conditions
				ApplyMove(move);
				// if the game was in check before this player
				if (inCheckBefore)
				{
					//If the king of the player is under attack by the enemy
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						//undo the move so, because the player will need to sacrifice that piece in order 
						//to keep the king alive
						UndoLastMove();
						continue;
					}
					else
					{
						//if the king is no longer under attack, then add that move to the lise
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
				else
				{
					//if the game was not in check but moving the piece causes the king to go into check
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						//undo the move because if the piece moves, the game would be over
						UndoLastMove();
						continue;
					}
					else
					{
						//if the piece moves and the king is safe, then the move is valid
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
			}
			//Pawn Moves
			foreach (var move in allPawnMoves)
			{
				ApplyMove(move);
				if (inCheckBefore)
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
				else
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
			}
			//Rook Moves
			foreach (var move in allRookMoves)
			{
				ApplyMove(move);
				if (inCheckBefore)
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
				else
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
			}

			//Bishop Moves
			foreach (var move in allBishopMoves)
			{
				ApplyMove(move);
				if (inCheckBefore)
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
				else
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
			}
			//Queen Moves
			foreach (var move in allQueenMoves)
			{
				ApplyMove(move);
				if (inCheckBefore)
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
				else
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
			}
			//King Moves
			foreach (var move in allKingMoves)
			{
				ApplyMove(move);
				//gets the new postion of the king after its possible move is applied to the board
				kingPos = GetPositionsOfPiece(ChessPieceType.King, player);
				if (inCheckBefore)
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
				else
				{
					if (PositionIsAttacked(kingPos.First(), enemy))
					{
						UndoLastMove();
						continue;
					}
					else
					{
						UndoLastMove();
						validMoves.Add(move);
						continue;
					}
				}
			}
			//King Castle Moves
			foreach (var move in kingCastles)
			{
				//Cannot castle if the king was in check before this players move
				if (!inCheckBefore)
				{
					validMoves.Add(move);
				}
			}
			//Checks to see if the preivous state was in check and there are no more valid moves
			if (inCheckBefore && validMoves.Count() == 0)
			{
				//sets the game to finished
				GameInCheck = false;
				GameIsFinished = true;
			}
			else if (!inCheckBefore && validMoves.Count() == 0)
			{
				//sets the game to finished if there are no more valid moces
				GameIsFinished = true;
			}
			return validMoves;
		}

		/// <summary>
		/// Applies a ChessMove to the board
		/// </summary>
		public void ApplyMove(ChessMove m) {
			//gets the piece at the start of the move
			ChessPiece startPiece = GetPieceAtPosition(m.StartPosition);
			//gets the piece at the end of the move
			ChessPiece endPiece = GetPieceAtPosition(m.EndPosition);
			//sets the move type to that of the move
			ChessMoveType moveType = m.MoveType;
			//makes the sure that the move is equal to the player and piece
			m.PieceType = startPiece.PieceType;
			m.Player = startPiece.Player;
			//gets the player of the move
			int player = m.Player;
			//gets the enemy
			int enemy = (player % 2) + 1;
			//gets the position of the enemy king
			var enemyKing = GetPositionsOfPiece(ChessPieceType.King, enemy);

			//if there is an enemy piece at the end position, then it will capture the piece as long as its not caslting
			if(endPiece.PieceType != ChessPieceType.Empty && moveType != ChessMoveType.CastleQueenSide && moveType != ChessMoveType.CastleKingSide)
			{
				//captures the piece and sets the pieces state to captured
				m.Captured = endPiece;
				m.IsCaptured = true;
			}
			else
			{
				//if the move is an enpassant, it captures the enemy pawn
				if(moveType == ChessMoveType.EnPassant)
				{
					m.Captured = new ChessPiece(ChessPieceType.Pawn, enemy);
					m.IsCaptured = true;
				}
				else
				{
					//else nothing is captured
					m.IsCaptured = false;
				}
			}
			//if the move is just a normal move, taking an empty space
			if (moveType == ChessMoveType.Normal)
			{
				//sets the start poistion of the move to an empty square
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				//sets the end position to the piece that was being moved
				SetPieceAtPosition(m.EndPosition, startPiece);
				//if the piece is a pawm that is being moved or if there was a capture
				//that means there has been progress and the draw counter needs to be reset
				if (startPiece.PieceType == ChessPieceType.Pawn || m.IsCaptured)
				{
					DrawCount.Add(0);
				}
				else
				{
					//if no progress
					DrawCount.Add(DrawCount.Last() + 1);
				}
			//if the move is a pawn promote
			} else if (moveType == ChessMoveType.PawnPromote)
			{
				//create an empty square at the starting positioin
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				//creates a new piece depending on what promote is being created. changes from pawn to that promoted piece
				SetPieceAtPosition(m.EndPosition, new ChessPiece(m.PawnPromoted, player));
				//resets the draw counter since its progress
				DrawCount.Add(0);
			} else if (moveType == ChessMoveType.EnPassant) 
			{
				//keeps track of what is above the pawn that is being captured (white side)
				var pawnCapUp = m.EndPosition.Translate(-1, 0);
				//keeps track of what is below the pawn that is being captured (black side)
				var pawnCapDown = m.EndPosition.Translate(1, 0);
				//create an empty square at the starting positioin
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				//sets the start piece, which is a  pawn to the end position of the move
				SetPieceAtPosition(m.EndPosition, startPiece);
				//if the player is white
				if(player == 1)
				{
					//sets the pawn that was captured to an empty square
					SetPieceAtPosition(pawnCapDown, new ChessPiece(ChessPieceType.Empty, 0));
				}
				else
				{
					SetPieceAtPosition(pawnCapUp, new ChessPiece(ChessPieceType.Empty, 0));
				}
				DrawCount.Add(0);
			//Castling king side of the board
			} else if (moveType == ChessMoveType.CastleKingSide)
			{
				//gets the location of where the rook is going to land
				var rookMove = m.StartPosition.Translate(0, 1);
				//gets the starting location of the rook
				var rookStart = m.EndPosition.Translate(0, 1);
				//puts the rook on the left side of the king (white perspective)
				SetPieceAtPosition(rookMove, new ChessPiece(ChessPieceType.Rook, player));
				//sets the king on the right side of the rook
				SetPieceAtPosition(m.EndPosition, startPiece);
				//sets the kings starting point to an empty square
				SetPieceAtPosition(m.StartPosition, new ChessPiece((ChessPieceType.Empty), 0));
				//sets the rooks starting poin to an empty square
				SetPieceAtPosition(rookStart, new ChessPiece((ChessPieceType.Empty), 0));
				//adds to the draw count since no progress was made
				DrawCount.Add(DrawCount.Last() + 1);
			} else if (moveType == ChessMoveType.CastleQueenSide)
			{
				var rookMove = m.StartPosition.Translate(0, -1);
				//since the queen side has more empty squares between the rook and king
				var rookStart = m.EndPosition.Translate(0, -2);
				SetPieceAtPosition(rookMove, new ChessPiece(ChessPieceType.Rook, player));
				SetPieceAtPosition(m.EndPosition, startPiece);
				SetPieceAtPosition(m.StartPosition, new ChessPiece((ChessPieceType.Empty), 0));
				SetPieceAtPosition(rookStart, new ChessPiece((ChessPieceType.Empty), 0));
				DrawCount.Add(DrawCount.Last() + 1);
			}
			//if the game has reached a draw
			if (IsDraw)
			{
				GameIsFinished = true;
			}
			//if the enemy king has been placed in check
			if(PositionIsAttacked(enemyKing.First(), player))
			{
				GameInCheck = true;
			}
			else
			{
				GameInCheck = false;
			}
			//calculates the current game advantage of the board
			Advantage = CurrentGameAdvantage();
			//adds the move to the history of moves
			mMoveHistory.Add(m);
			//swaps the current player to the enemy
			currentPlayer = enemy;
		}

		/// <summary>
		/// Undos the last move in the move history
		/// </summary>
		public void UndoLastMove() {
			//call the last move in the history log
			ChessMove m = mMoveHistory.Last();
			//saves the end position of the last move
			var end = m.EndPosition;
			//saves the start positon of the last move
			var start = m.StartPosition;
			//saves the piece type of the last move
			var pieceTpye = m.PieceType;
			//sets the move type of the last move
			var moveType = m.MoveType;
			//sets the current player
			int player = m.Player;
			//sets the enemey
			int enemy = (player % 2) + 1;
			//since undoing, game is not finished
			GameIsFinished = false;
			//if the move was a normal move
			if (moveType == ChessMoveType.Normal)
			{
				//if a piece was captured
				if (m.IsCaptured)
				{
					//set the end postion of the last move to the captured piece
					SetPieceAtPosition(m.EndPosition, m.Captured);
					//sets the start postion to the piece that captured that piece
					SetPieceAtPosition(m.StartPosition, new ChessPiece(m.PieceType, player));
				}
				else
				{
					//set the end postion of the last move back to empty
					SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
					//sets the start postion of the last move back to the piece that was there
					SetPieceAtPosition(m.StartPosition, new ChessPiece(m.PieceType, player));
				}
			}
			else if (m.MoveType == ChessMoveType.CastleKingSide)
			{
				var rookMove = m.EndPosition.Translate(0, 1);
				var rookStart = m.EndPosition.Translate(0, -1);
				//move rook back to original position
				SetPieceAtPosition(rookMove, new ChessPiece(ChessPieceType.Rook, player));
				//clear the spot it previously occupied
				SetPieceAtPosition(m.EndPosition, new ChessPiece((ChessPieceType.Empty), 0));

				//clear the spot it previously occupied
				SetPieceAtPosition(rookStart, new ChessPiece(ChessPieceType.Empty, 0));
				//move the king back to its starting pos
				SetPieceAtPosition(m.StartPosition, new ChessPiece(m.PieceType, player));
			}
			else if (m.MoveType == ChessMoveType.CastleQueenSide)
			{
				var rookMove = m.StartPosition.Translate(0, 1);
				var rookStart = m.EndPosition.Translate(0, -2);
				//clear the spot it previously occupied
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
				//moves the rook back
				SetPieceAtPosition(rookStart, new ChessPiece((ChessPieceType.Rook), player));

				//clear the spot it previously occupied
				SetPieceAtPosition(rookMove, new ChessPiece(ChessPieceType.Empty, 0));
				//moves the king back
				SetPieceAtPosition(m.StartPosition, new ChessPiece(m.PieceType, player));
			}
			else if (m.MoveType == ChessMoveType.EnPassant)
			{
				//oppsite logic for applymove
				var pawnCapUp = m.EndPosition.Translate(-1, 0);
				var pawnCapDown = m.EndPosition.Translate(1, 0);
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
				if (player == 1)
				{
					//restores the pawn that was removed
					SetPieceAtPosition(pawnCapDown, new ChessPiece(ChessPieceType.Pawn, enemy));
				}
				else
				{
					SetPieceAtPosition(pawnCapUp, new ChessPiece(ChessPieceType.Pawn, enemy));
				}
				//puts the pawn back in place
				SetPieceAtPosition(m.StartPosition, new ChessPiece(m.PieceType, player));
			}
			else if (m.MoveType == ChessMoveType.PawnPromote)
			{
				//if there was a capture on the pawn promote
				if (m.IsCaptured)
				{
					//put the capture piece back to its spot
					SetPieceAtPosition(m.EndPosition, m.Captured);
					//put the pawn back at its starting point
					SetPieceAtPosition(m.StartPosition, new ChessPiece(m.PieceType, player));
				}
				else
				{
					//if no capture, then put the empty square back and the pawn
					SetPieceAtPosition(end, new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(start, new ChessPiece(pieceTpye, player));
				}
			}
			//calculate the current advantage of the game
			Advantage = CurrentGameAdvantage();
			//gets the position of the current players king
			BoardPosition kingPos = GetPositionsOfPiece(ChessPieceType.King, player).First();
			//if the king is now being attacked
			if (PositionIsAttacked(kingPos, enemy))
			{
				// the game is back in check
				GameInCheck = true;
			}
			else
			{
				GameInCheck = false;
			}
			//reset the remaining game state		
			DrawCount.RemoveAt(DrawCount.Count() - 1);
			mMoveHistory.RemoveAt(mMoveHistory.Count() - 1);
			currentPlayer = player;
		}
	
		/// <summary>
		/// Returns whatever chess piece is occupying the given position.
		/// </summary>
		public ChessPiece GetPieceAtPosition(BoardPosition position) {
			//take the row and times by 4 to get starting index (7,1) row 7*4 = 28
			int row = position.Row * 4;
			//take the col and divide by 2 to get how many cols over
			int col = position.Col / 2;
			//add them together to get exact index
			int index = row + col;
			//gets left side or right side of index
			var leftOrRight = position.Col % 2;

			//finds the value at the index inside the board
			var boardValue = cboard[index];
			//1111
			var mask = 0x0F;
			//finds the first half and second half of the board value
			//10101011
			//00001111 - >00001011
			var firstHalf = boardValue >> 4;
			var secondHalf = boardValue & mask;

			//finds the player of each have by shifting right three times
			var firstPlayer = firstHalf >> 3;
			var secondPlayer = secondHalf >> 3;

			//0111
			var maskPiece = 0x07;
			//finds the piece by anding the first and second halves with the above mask
			//1010
			//0111  -> return 010
			var firstPiece = firstHalf & maskPiece;
			var secondPiece = secondHalf & maskPiece;

			ChessPiece foundPiece;

			//equal to right
			if(leftOrRight == 0)
			{
				//returns an empty space
				if(firstPiece == 0)
				{
					foundPiece = new ChessPiece(ChessPieceType.Empty, 0);
					return foundPiece;
				}
				//creates the piece that is going to be returned
				foundPiece = new ChessPiece((ChessPieceType)firstPiece, firstPlayer+1);
				return foundPiece;
			}
			else
			{
				if (secondPiece == 0)
				{
					foundPiece = new ChessPiece(ChessPieceType.Empty, 0);
					return foundPiece;
				}
				foundPiece = new ChessPiece((ChessPieceType)secondPiece, secondPlayer+1);
				return foundPiece;
			}
		}

		/// <summary>
		/// Returns whatever player is occupying the given position.
		/// </summary>
		public int GetPlayerAtPosition(BoardPosition pos) {
			ChessPiece piece = GetPieceAtPosition(pos);
			return piece.Player;
		}

		/// <summary>
		/// Returns true if the given position on the board is empty.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEmpty(BoardPosition pos) {
			//if out of bounds
			if (PositionInBounds(pos))
			{
				//gets piece at pos
				ChessPiece piece = GetPieceAtPosition(pos);
				//if the spot is empty
				if(piece.PieceType == ChessPieceType.Empty)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Returns true if the given position contains a piece that is the enemy of the given player.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEnemy(BoardPosition pos, int player) {
			//if in bounds
			if (PositionInBounds(pos))
			{
				//if spot is empty
				if (PositionIsEmpty(pos))
				{
					return false;
				}
				else
				{
					//checks to see if passed in player is equal to player at pos
					return player != GetPlayerAtPosition(pos) ? true : false;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Returns true if the given position is in the bounds of the board.
		/// </summary>
		public static bool PositionInBounds(BoardPosition pos) {
			int row = pos.Row;
			int col = pos.Col;
			
			//checks to see of the column goes out of bounds (left/right)
			if(col < 0 || col > 7)
			{
				return false;
			}
			//checks to see if the rows go out of bounds (top/bottom)
			if(row < 0 || row > 7)
			{
				return false;
			}

			return true;
			
		}

		/// <summary>
		/// Returns all board positions where the given piece can be found.
		/// </summary>
		public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType piece, int player) {
			//create a list
			List<BoardPosition> piecePositions = new List<BoardPosition>();
			//searches rows
			for(int row = 0; row < 8; row++)
			{
				//searches cols
				for(int col = 0; col < 8; col++)
				{
					//creates a new position with the current row and col
					BoardPosition pos = new BoardPosition(row, col);
					//gets the chess piece at the current position
					ChessPiece chessPiece = GetPieceAtPosition(pos);
					//if the player matches the passed in player
					if (player == chessPiece.Player)
					{
						//if the piece matches the passed in piece
						if (piece == chessPiece.PieceType)
						{
							//add to the list of locations that matches the piece and player
							piecePositions.Add(pos);
						}
					}
				}
			}
			return piecePositions;
		}

		/// <summary>
		/// Returns true if the given player's pieces are attacking the given position.
		/// </summary>
		public bool PositionIsAttacked(BoardPosition position, int byPlayer) {
			var allAttackedPos = GetAttackedPositions(byPlayer);
			return allAttackedPos.Contains(position);
		}

		/// <summary>
		/// Returns a set of all BoardPositions that are attacked by the given player. A posistion can 
		/// only be attacked by one piece. multiple pieces can attack it but it will not duplicate
		/// </summary>
		public ISet<BoardPosition> GetAttackedPositions(int byPlayer) {
			//stores all the possible attack moves in the hash set
			HashSet<BoardPosition> attackedPositions = new HashSet<BoardPosition>();

			//gets all the bishop moves for the current player
			var bishopAttackMoves = GetAllBishopAttackMoves(byPlayer);
			foreach(var attack in bishopAttackMoves)
			{
				//for each attack in attack moves, it addes the endposition to the total list
				//end pos is where the piece can attack
				attackedPositions.Add(attack.EndPosition);
			}

			//All Knights
			var knightAttackMoves = GetAllKnightAttackMoves(byPlayer);
			foreach (var attack in knightAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}

			//All Rooks
			var rookAttackMoves = GetAllRookAttackMoves(byPlayer);
			foreach (var attack in rookAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}

			//All Queens
			var queenAttackMoves = GetAllQueenAttackMoves(byPlayer);
			foreach (var attack in queenAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}

			//King
			var kingAttackMoves = GetAllKingAttackMoves(byPlayer);
			foreach (var attack in kingAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}

			//all pawns
			var pawnAttackMoves = GetAllPawnAttackMoves(byPlayer);
			foreach (var attack in pawnAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}

			return attackedPositions;
		}

		/// <summary>
		/// Returns a set of all BoardPositions that are attacked by the given player. 
		/// will be used for calculating the weight of the board.
		/// THIS WILL ADD EVERY ATTACKED POSITION, EVEN IF THE POS IS ATTACKED BY A DIFFERENT PIECE.
		/// </summary>
		public List<BoardPosition> GetAllAttackedPositions(int byPlayer)
		{
			//stores all the possible attack moves in the hash set
			List<BoardPosition> attackedPositions = new List<BoardPosition>();

			//All Rooks
			var rookAttackMoves = GetAllRookAttackMoves(byPlayer);
			foreach (var attack in rookAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}
			//All Knights
			var knightAttackMoves = GetAllKnightAttackMoves(byPlayer);
			foreach (var attack in knightAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}
			//gets all the bishop moves for the current player
			var bishopAttackMoves = GetAllBishopAttackMoves(byPlayer);
			foreach (var attack in bishopAttackMoves)
			{
				//for each attack in attack moves, it addes the endposition to the total list
				//end pos is where the piece can attack
				attackedPositions.Add(attack.EndPosition);
			}

			//All Queens
			var queenAttackMoves = GetAllQueenAttackMoves(byPlayer);
			foreach (var attack in queenAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}

			//King
			var kingAttackMoves = GetAllKingAttackMoves(byPlayer);
			foreach (var attack in kingAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}

			//all pawns
			var pawnAttackMoves = GetAllPawnAttackMoves(byPlayer);
			foreach (var attack in pawnAttackMoves)
			{
				attackedPositions.Add(attack.EndPosition);
			}

			return attackedPositions;
		}
		#endregion

		#region Private methods.
		/// <summary>
		/// Mutates the board state so that the given piece is at the given position.
		/// </summary>
		private void SetPieceAtPosition(BoardPosition position, ChessPiece piece) {
			//take the row and times by 4 to get starting index (7,1) row 7*4 = 28
			int row = position.Row * 4;
			//take the col and divide by 2 to get how many cols over
			int col = position.Col / 2;
			//add them together to get exact index
			int index = row + col;
			//gets left side or right side of index
			var leftOrRight = position.Col % 2;

			//finds the value at the index inside the board
			var boardValue = cboard[index];

			//converts enum to in
			byte pieceType = (byte)piece.PieceType;
			//gets player
			byte piecePlayer = (byte)(piece.Player - 1);

			//moves the player to the left ex. player 1 -> 1000
			var piecePlayerConvert = piecePlayer << 3;
			//combines player and piece ex. player 1 -> 1000, pawn 1 -> 0001 = 1001
			var pieceConvert = piecePlayerConvert | pieceType;

			//as long as the piece is not an empty piece
			if (piece.PieceType != ChessPieceType.Empty)
			{
				//if left side of byte
				if (leftOrRight == 0)
				{
					//move piece to left side ex. 1001 -> 10010000
					pieceConvert <<= 4;
					//takes current board and modifys
					cboard[index] = (byte)(pieceConvert | (boardValue & 0x0F));
				}
				else
				{ 

					cboard[index] = (byte)(pieceConvert | (boardValue & 0xF0));
				}
			}
			else
			{
				//if left side of byte
				if (leftOrRight == 0)
				{
					//needs mask so it can correctly save
					var mask = 0x0F;
					//takes current board and modifys
					cboard[index] = (byte)(mask & boardValue);
				}
				else
				{
					var mask = 0xF0;
					cboard[index] = (byte)(mask & boardValue);
				}
			}

		}

		/// <summary>
		/// Calculates the current game advantage
		/// </summary>
		private GameAdvantage CurrentGameAdvantage()
		{
			//pawns are worth 1 point
			int pawnValue = 1;
			//bishops are  worth 3 point
			int bishopValue = 3;
			//Knights are  worth 3 point
			int knightValue = 3;
			//Rooks are  worth 5 point
			int rookValue = 5;
			//Queen are  worth 9 point
			int queenValue = 9;

			//gets the total amount of pieces that are on the board for that player and mulitplied them by their point value
			var whitePawnsPoints = GetPositionsOfPiece(ChessPieceType.Pawn, 1).Count() * pawnValue;
			var whiteBishopsPoints = GetPositionsOfPiece(ChessPieceType.Bishop, 1).Count() * bishopValue;
			var whiteKnightssPoints = GetPositionsOfPiece(ChessPieceType.Knight, 1).Count() * knightValue;
			var whiteRooksPoints = GetPositionsOfPiece(ChessPieceType.Rook, 1).Count() * rookValue;
			var whiteQueensPoints = GetPositionsOfPiece(ChessPieceType.Queen, 1).Count() * queenValue;
			//sums up all the points
			int whiteSum = whitePawnsPoints + whiteBishopsPoints + whiteKnightssPoints + whiteRooksPoints + whiteQueensPoints;

			var blackPawnsPoints = GetPositionsOfPiece(ChessPieceType.Pawn, 2).Count() * pawnValue;
			var blackBishopsPoints = GetPositionsOfPiece(ChessPieceType.Bishop, 2).Count() * bishopValue;
			var blackKnightssPoints = GetPositionsOfPiece(ChessPieceType.Knight, 2).Count() * knightValue;
			var blackRooksPoints = GetPositionsOfPiece(ChessPieceType.Rook, 2).Count() * rookValue;
			var blackQueensPoints = GetPositionsOfPiece(ChessPieceType.Queen, 2).Count() * queenValue;
			int blackSum = blackPawnsPoints + blackBishopsPoints + blackKnightssPoints + blackRooksPoints + blackQueensPoints;

			//if the advantage is the same
			if(blackSum == whiteSum)
			{
				//returns a 0,0 to show that no player has the advantage
				return new GameAdvantage(0, 0);
			}else if(blackSum < whiteSum)
			{
				//white has the advantage
				return new GameAdvantage(1, whiteSum - blackSum);
			}
			else
			{
				//black has the advantage
				return new GameAdvantage(2, blackSum - whiteSum);
			}
		}

		/// <summary>
		/// Gets all of the diagonal attack moves that a certain players piece has at a location.
		/// can attack all empty squares, also enemy and friendly squares
		/// </summary>
		private List<ChessMove> DiagonalAttackMoves(BoardPosition currentPos, ChessPieceType pieceType, int player)
		{
			//creaete a new list that will hold the chess moves
			var moves = new List<ChessMove>();
			//keeps track of how many times it needs to be translated
			int translateCount = 1;

			//perform an up to the right translate
			BoardPosition upRight = currentPos.Translate(-1, 1);
			//while that translate is inbounds
			while (PositionInBounds(upRight))
			{
				var nextPieceCheck = GetPieceAtPosition(upRight);
				//if the position is empty, it will add the move
				if (nextPieceCheck.PieceType == ChessPieceType.Empty)
				{
					//creates a new chess move with the current pos to the translation
					ChessMove moveUpRight = new ChessMove(currentPos, upRight)
					{
						//sets attributes
						Player = player,
						PieceType = pieceType
					};
					//adds to the moves list
					moves.Add(moveUpRight);
					//only lets the king move to the up to the right once
					if (pieceType == ChessPieceType.King)
					{
						break;
					}
					//increments the counter
					translateCount++;
					//performs the next translate, ex (2,2), (3,3)
					upRight = currentPos.Translate(-translateCount, translateCount);
					//skips rest of loop and goes back to top
					continue;
				}
				//if the spot is not empty, regardless of player it will add the move
				else 
				{
					ChessMove moveUpRight = new ChessMove(currentPos, upRight)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveUpRight);
					//breaks out of the loop
					break;
				}
			}

			//for up to the left
			translateCount = 1;
			BoardPosition upLeft = currentPos.Translate(-1, -1);
			while (PositionInBounds(upLeft))
			{
				var nextPieceCheck = GetPieceAtPosition(upLeft);
				//if the position is empty, it will add the move
				if (nextPieceCheck.PieceType == ChessPieceType.Empty)
				{
					ChessMove moveUpLeft = new ChessMove(currentPos, upLeft)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveUpLeft);
					//only lets the king move to the up to the once
					if (pieceType == ChessPieceType.King)
					{
						break;
					}
					translateCount++;
					upLeft = currentPos.Translate(-translateCount, -translateCount);
					continue;
				}
				else
				{
					ChessMove moveUpLeft = new ChessMove(currentPos, upLeft)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveUpLeft);
					break;
				}
			}

			//for down to the left
			translateCount = 1;
			BoardPosition downLeft = currentPos.Translate(1, -1);
			while (PositionInBounds(downLeft))
			{
				var nextPieceCheck = GetPieceAtPosition(downLeft);
				//if the position is empty, it will add the move
				if (nextPieceCheck.PieceType == ChessPieceType.Empty)
				{
					ChessMove moveDownLeft = new ChessMove(currentPos, downLeft)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveDownLeft);
					//only lets the king move to the down to the left once
					if (pieceType == ChessPieceType.King)
					{
						break;
					}
					translateCount++;
					downLeft = currentPos.Translate(translateCount, -translateCount);
					continue;
				}
				else
				{
					ChessMove moveDownLeft = new ChessMove(currentPos, downLeft)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveDownLeft);
					break;
				}
			}

			//for down to the right
			translateCount = 1;
			BoardPosition downRight = currentPos.Translate(1, 1);
			while (PositionInBounds(downRight))
			{
				var nextPieceCheck = GetPieceAtPosition(downRight);
				//if the position is empty, it will add the move
				if (nextPieceCheck.PieceType == ChessPieceType.Empty)
				{
					ChessMove moveDownRight = new ChessMove(currentPos, downRight)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveDownRight);
					//only lets the king move to the down to the right once
					if (pieceType == ChessPieceType.King)
					{
						break;
					}
					translateCount++;
					downRight = currentPos.Translate(translateCount, translateCount);
					continue;
				}
				else
				{
					ChessMove moveDownRight = new ChessMove(currentPos, downRight)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveDownRight);
					break;
				}
			}
			return moves;
		}

		/// <summary>
		/// Gets all the moves for one knight
		/// </summary>
		private List<ChessMove> KnightAttackMoves(BoardPosition currentPos, int player)
		{
			//adds all the moves together
			List<ChessMove> moves = new List<ChessMove>();

			//performs all the possible translates
			//two spaces up, right one
			BoardPosition upRight = currentPos.Translate(-2, 1);
			BoardPosition upLeft = currentPos.Translate(-2, -1);
			BoardPosition downRight = currentPos.Translate(2, 1);
			BoardPosition downLeft = currentPos.Translate(2, -1);
			//two spaces to the left, up one
			BoardPosition leftUp = currentPos.Translate(-1, -2);
			BoardPosition leftDown = currentPos.Translate(1, -2);
			BoardPosition rightUp = currentPos.Translate(-1, 2);
			BoardPosition rightDown = currentPos.Translate(1, 2);

			//makes sure the move is inbounds
			if (PositionInBounds(upRight))
			{
				//creates the new move
				ChessMove moveUpRight = new ChessMove(currentPos, upRight)
				{
					Player = player,
					PieceType = ChessPieceType.Knight
				};
				//adds to the moves list
				moves.Add(moveUpRight);
			}
			if (PositionInBounds(upLeft))
			{
				ChessMove moveUpLeft = new ChessMove(currentPos, upLeft)
				{
					Player = player,
					PieceType = ChessPieceType.Knight
				};
				moves.Add(moveUpLeft);
			}
			if (PositionInBounds(downRight))
			{
				ChessMove moveDownRight = new ChessMove(currentPos, downRight)
				{
					Player = player,
					PieceType = ChessPieceType.Knight
				};
				moves.Add(moveDownRight);
			}
			if (PositionInBounds(downLeft))
			{
				ChessMove moveDownLeft = new ChessMove(currentPos, downLeft)
				{
					Player = player,
					PieceType = ChessPieceType.Knight
				};
				moves.Add(moveDownLeft);
			}
			if (PositionInBounds(rightUp))
			{
				ChessMove moveRightUp = new ChessMove(currentPos, rightUp)
				{
					Player = player,
					PieceType = ChessPieceType.Knight
				};
				moves.Add(moveRightUp);
			}
			if (PositionInBounds(rightDown))
			{
				ChessMove moveRightDown = new ChessMove(currentPos, rightDown)
				{
					Player = player,
					PieceType = ChessPieceType.Knight
				};
				moves.Add(moveRightDown);
			}
			if (PositionInBounds(leftUp))
			{
				ChessMove moveLeftUp = new ChessMove(currentPos, leftUp)
				{
					Player = player,
					PieceType = ChessPieceType.Knight
				};
				moves.Add(moveLeftUp);
			}
			if (PositionInBounds(leftDown))
			{
				ChessMove moveLeftDown = new ChessMove(currentPos, leftDown)
				{
					Player = player,
					PieceType = ChessPieceType.Knight
				};
				moves.Add(moveLeftDown);
			}
			return moves;
		}

		/// <summary>
		/// Gets all the vertical moves a piece can make
		/// </summary>
		private List<ChessMove> VerticalAttackMoves(BoardPosition currentPos, ChessPieceType pieceType, int player)
		{
			//creaete a new list that will hold the chess moves
			var moves = new List<ChessMove>();
			//keeps track of how many times it needs to be translated
			int translateCount = 1;

			//perform an up translate
			BoardPosition up = currentPos.Translate(-1, 0);
			//while that translate is inbounds
			while (PositionInBounds(up))
			{
				var nextPieceCheck = GetPieceAtPosition(up);
				//if the position is empty, it will add the move
				if (nextPieceCheck.PieceType == ChessPieceType.Empty)
				{
					//creates a new chess move with the current pos to the translation
					ChessMove moveUp = new ChessMove(currentPos, up)
					{
						//sets attributes
						Player = player,
						PieceType = pieceType
					};
					//adds to the moves list
					moves.Add(moveUp);
					//only lets the king move to the up once
					if (pieceType == ChessPieceType.King)
					{
						break;
					}
					//increments the counter
					translateCount++;
					//performs the next translate, ex (2,0), (3,0)
					up = currentPos.Translate(-translateCount, 0);
					//skips rest of loop and goes back to top
					continue;
				}
				//if the spot is not empty, regardless of player it will add the move
				else
				{
					ChessMove moveUp = new ChessMove(currentPos, up)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveUp);
					//breaks out of the loop
					break;
				}
			}

			//for down
			translateCount = 1;
			BoardPosition down = currentPos.Translate(1, 0);
			while (PositionInBounds(down))
			{
				var nextPieceCheck = GetPieceAtPosition(down);
				//if the position is empty, it will add the move
				if (nextPieceCheck.PieceType == ChessPieceType.Empty)
				{
					ChessMove moveDown = new ChessMove(currentPos, down)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveDown);
					//only lets the king move to the down once
					if (pieceType == ChessPieceType.King)
					{
						break;
					}
					translateCount++;
					down = currentPos.Translate(translateCount, 0);
					continue;
				}
				else
				{
					ChessMove moveDown = new ChessMove(currentPos, down)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveDown);
					break;
				}
			}

			return moves;
		}

		/// <summary>
		/// Gets all the horizontal moves that a piece can take
		/// </summary>
		private List<ChessMove> HorizontalAttackMoves(BoardPosition currentPos, ChessPieceType pieceType, int player)
		{
			//create a new list that will hold the chess moves
			var moves = new List<ChessMove>();
			//keeps track of how many times it needs to be translated
			int translateCount = 1;

			//perform a right translate
			BoardPosition right = currentPos.Translate(0, 1);
			//while that translate is inbounds
			while (PositionInBounds(right))
			{
				var nextPieceCheck = GetPieceAtPosition(right);
				//if the position is empty, it will add the move
				if (nextPieceCheck.PieceType == ChessPieceType.Empty)
				{
					//creates a new chess move with the current pos to the translation
					ChessMove moveRight = new ChessMove(currentPos, right)
					{
						//sets attributes
						Player = player,
						PieceType = pieceType
					};
					//adds to the moves list
					moves.Add(moveRight);
					//only lets the king move to the right once
					if(pieceType == ChessPieceType.King)
					{
						break;
					}
					//increments the counter
					translateCount++;
					//performs the next translate, ex (2,0), (3,0)
					right = currentPos.Translate(0, translateCount);
					//skips rest of loop and goes back to top
					continue;
				}
				//if the spot is not empty, regardless of player it will add the move
				else
				{
					ChessMove moveRight = new ChessMove(currentPos, right)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveRight);
					//breaks out of the loop
					break;
				}
			}

			//for left
			translateCount = 1;
			BoardPosition left = currentPos.Translate(0, -1);
			while (PositionInBounds(left))
			{
				var nextPieceCheck = GetPieceAtPosition(left);
				//if the position is empty, it will add the move
				if (nextPieceCheck.PieceType == ChessPieceType.Empty)
				{
					ChessMove moveLeft = new ChessMove(currentPos, left)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveLeft);
					//only lets the king move to the left once
					if (pieceType == ChessPieceType.King)
					{
						break;
					}
					translateCount++;
					left = currentPos.Translate(0, -translateCount);
					continue;
				}
				else
				{
					ChessMove moveLeft = new ChessMove(currentPos, left)
					{
						Player = player,
						PieceType = pieceType
					};
					moves.Add(moveLeft);
					break;
				}
			}

			return moves;
		}

		/// <summary>
		/// Gets the pawn attack positions. only attack moves not the actual moves
		/// </summary>
		private List<ChessMove> PawnAttackPositions(BoardPosition currentPos, int player)
		{
			//create a list of chess moves
			var moves = new List<ChessMove>();

			// if its the white player
			if (player == 1)
			{
				//the possible attack moves for white
				BoardPosition upLeft = currentPos.Translate(-1, -1);
				BoardPosition upRight = currentPos.Translate(-1, 1);
				//if the attack is inbounds
				if (PositionInBounds(upLeft))
				{
					//create the chess move
					ChessMove moveUpLeft = new ChessMove(currentPos, upLeft)
					{
						Player = player,
						PieceType = ChessPieceType.Pawn
					};
					//adds to the list
					moves.Add(moveUpLeft);
				}
				if (PositionInBounds(upRight))
				{
					ChessMove moveUpRight = new ChessMove(currentPos, upRight)
					{
						Player = player,
						PieceType = ChessPieceType.Pawn
					};
					moves.Add(moveUpRight);
				}
			}
			else //black side
			{
				BoardPosition downLeft = currentPos.Translate(1, -1);
				BoardPosition downRight = currentPos.Translate(1, 1);
				if (PositionInBounds(downLeft))
				{
					ChessMove moveDownLeft = new ChessMove(currentPos, downLeft)
					{
						Player = player,
						PieceType = ChessPieceType.Pawn
					};
					moves.Add(moveDownLeft);
				}
				if (PositionInBounds(downRight))
				{
					ChessMove moveDownRight = new ChessMove(currentPos, downRight)
					{
						Player = player,
						PieceType = ChessPieceType.Pawn
					};
					moves.Add(moveDownRight);
				}
			}
			return moves;
		}

		/// <summary>
		/// Gets the pawn valid moves. not attack moves
		/// </summary>
		private List<ChessMove> PawnPossibleMoves(BoardPosition currentPos, int player)
		{
			var moves = new List<ChessMove>();

			//check to see if it's the first player
			if (player == 1)
			{
				//move either 1 or 2 spaces depending on if it is the first move
				BoardPosition up = currentPos.Translate(-1, 0);
				BoardPosition up2 = currentPos.Translate(-2, 0);

				//move once if not the first move
				if (PositionIsEmpty(up))
				{
					//if the next move lands on the last row, pawn promote
					if(up.Row == 0)
					{
						//creates a new chess moves that will promote a pawn to the queen
						ChessMove promoteQueen = new ChessMove(currentPos, up, ChessPieceType.Queen)
						{
							Player = player
						};
						ChessMove promoteRook = new ChessMove(currentPos, up, ChessPieceType.Rook)
						{
							Player = player
						};
						ChessMove promoteKnight = new ChessMove(currentPos, up, ChessPieceType.Knight)
						{
							Player = player
						};
						ChessMove promoteBishop = new ChessMove(currentPos, up, ChessPieceType.Bishop)
						{
							Player = player
						};
						//adds all the possible promotes to the list of moves
						moves.Add(promoteQueen);
						moves.Add(promoteRook);
						moves.Add(promoteKnight);
						moves.Add(promoteBishop);
					}
					else
					{
						//if its just a regular row, then move up
						ChessMove moveUp = new ChessMove(currentPos, up)
						{
							Player = player,
							PieceType = ChessPieceType.Pawn
						};
						moves.Add(moveUp);
					}
				}
				//check if it's the first move
				if (currentPos.Row == 6)
				{
					//make sure the spoit above and two spots above are empty
					if (PositionIsEmpty(up2) && PositionIsEmpty(up))
					{
						ChessMove moveUp2 = new ChessMove(currentPos, up2)
						{
							Player = player,
							PieceType = ChessPieceType.Pawn
						};
						moves.Add(moveUp2);
					}
				}
			}
			//player 2
			else
			{
				BoardPosition down = currentPos.Translate(1, 0);
				BoardPosition down2 = currentPos.Translate(2, 0);
				if (PositionIsEmpty(down))
				{
					//if the next move lands on the last row, pawn promote
					if (down.Row == 7)
					{
						//creates a new chess moves that will promote a pawn to the queen
						ChessMove promoteQueen = new ChessMove(currentPos, down, ChessPieceType.Queen)
						{
							Player = player
						};
						ChessMove promoteRook = new ChessMove(currentPos, down, ChessPieceType.Rook)
						{
							Player = player
						};
						ChessMove promoteKnight = new ChessMove(currentPos, down, ChessPieceType.Knight)
						{
							Player = player
						};
						ChessMove promoteBishop = new ChessMove(currentPos, down, ChessPieceType.Bishop)
						{
							Player = player
						};
						//adds all the possible promotes to the list of moves
						moves.Add(promoteQueen);
						moves.Add(promoteRook);
						moves.Add(promoteKnight);
						moves.Add(promoteBishop);
					}
					else
					{
						//if its just a regular row, then move up
						ChessMove moveDown = new ChessMove(currentPos, down)
						{
							Player = player,
							PieceType = ChessPieceType.Pawn
						};
						moves.Add(moveDown);
					}
				}
				if (currentPos.Row == 1)
				{
					if (PositionIsEmpty(down2) && PositionIsEmpty(down))
					{
						ChessMove moveDown2 = new ChessMove(currentPos, down2)
						{
							Player = player,
							PieceType = ChessPieceType.Pawn
						};
						moves.Add(moveDown2);
					}
				}
			}
			return moves;
		}

		private List<ChessMove> KingCastlesMoves(BoardPosition currentPos, int player)
		{
			//creats the list of moves
			var moves = new List<ChessMove>();
			//determines the enemy
			int enemy = player % 2 + 1;

			//for the king side castling, needs the two spaces next to king
			BoardPosition rightOne = currentPos.Translate(0, 1);
			BoardPosition rightTwo = currentPos.Translate(0, 2);
			//queen side, needs three spaces next to king (left)
			BoardPosition leftOne = currentPos.Translate(0, -1);
			BoardPosition leftTwo = currentPos.Translate(0, -2);
			BoardPosition leftThree = currentPos.Translate(0, -3);
			//if white player
			if (player == 1)
			{
				//if the current postion is at the bottom of the board
				if(currentPos.Row == 7)
				{
					//determines if the king has moved by looking at the move history
					IEnumerable<ChessMove> hasKingMoved = mMoveHistory.Where(m => m.StartPosition.Equals(currentPos));
					//if the king has not moved
					if (!hasKingMoved.Any())
					{
						//checks to see if the two spots to the right are empty
						if (PositionIsEmpty(rightOne) && PositionIsEmpty(rightTwo))
						{
							//gets the starting pos of the right rook
							BoardPosition rightRookStart = new BoardPosition(7, 7);
							//makes sure that the rook has not been moved
							IEnumerable<ChessMove> hasRightRookMoved = mMoveHistory.Where(m => m.StartPosition.Equals(rightRookStart));
							//as long as the rook has not moved
							if (!hasRightRookMoved.Any())
							{
								//make sure none of the moves exposes the king to an enemy attack
								if (!PositionIsAttacked(rightOne, enemy) && !PositionIsAttacked(rightTwo, enemy))
								{
									//creates the castling move
									ChessMove moveKing = new ChessMove(currentPos, rightTwo, ChessMoveType.CastleKingSide)
									{
										Player = player,
										PieceType = ChessPieceType.King
									};
									//adds the the list
									moves.Add(moveKing);
								}
							}
						}
						//queen side
						if (PositionIsEmpty(leftOne) && PositionIsEmpty(leftTwo) && PositionIsEmpty(leftThree))
						{
							BoardPosition leftRookStart = new BoardPosition(7, 0);
							IEnumerable<ChessMove> hasLeftRookMoved = mMoveHistory.Where(m => m.StartPosition.Equals(leftRookStart));
							if (!hasLeftRookMoved.Any())
							{
								if (!PositionIsAttacked(leftOne, enemy) && !PositionIsAttacked(leftTwo, enemy))
								{
									ChessMove moveKing = new ChessMove(currentPos, leftTwo, ChessMoveType.CastleQueenSide)
									{
										Player = player,
										PieceType = ChessPieceType.King
									};
									moves.Add(moveKing);
								}
							}
						}
					}
				}
			}
			else //black side
			{
				if (currentPos.Row == 0)
				{
					IEnumerable<ChessMove> hasKingMoved = mMoveHistory.Where(m => m.StartPosition.Equals(currentPos));
					if (!hasKingMoved.Any())
					{
						if (PositionIsEmpty(rightOne) && PositionIsEmpty(rightTwo))
						{
							BoardPosition rightRookStart = new BoardPosition(0, 7);
							IEnumerable<ChessMove> hasRightRookMoved = mMoveHistory.Where(m => m.StartPosition.Equals(rightRookStart));
							if (!hasRightRookMoved.Any())
							{
								if (!PositionIsAttacked(rightOne, enemy) && !PositionIsAttacked(rightTwo, enemy))
								{
									ChessMove moveKing = new ChessMove(currentPos, rightTwo, ChessMoveType.CastleKingSide)
									{
										Player = player,
										PieceType = ChessPieceType.King
									};
									moves.Add(moveKing);
								}
							}
						}
						if (PositionIsEmpty(leftOne) && PositionIsEmpty(leftTwo) && PositionIsEmpty(leftThree))
						{
							BoardPosition leftRookStart = new BoardPosition(0, 0);
							IEnumerable<ChessMove> hasLeftRookMoved = mMoveHistory.Where(m => m.StartPosition.Equals(leftRookStart));
							if (!hasLeftRookMoved.Any())
							{
								if (!PositionIsAttacked(leftOne, enemy) && !PositionIsAttacked(leftTwo, enemy))
								{
									ChessMove moveKing = new ChessMove(currentPos, leftTwo, ChessMoveType.CastleQueenSide)
									{
										Player = player,
										PieceType = ChessPieceType.King
									};
									moves.Add(moveKing);
								}
							}
						}
					}
				}			
			}
			return moves;
		}

		/// <summary>
		/// Gets all the current pawn moves
		/// </summary>
		private List<ChessMove> GetAllPawnMoves(int player)
		{
			//create a list that will hold all the moves
			List<ChessMove> allMoves = new List<ChessMove>();
			//gets all the positions of the pawns
			var currentPawnPositions = GetPositionsOfPiece(ChessPieceType.Pawn, player);

			//goes through each pawn and gets its valid moves
			foreach(BoardPosition currentPawn in currentPawnPositions)
			{
				var moves = PawnPossibleMoves(currentPawn, player);
				allMoves.AddRange(moves);
			}

			return allMoves;
		}

		/// <summary>
		/// Gets all the current bishops attack moves
		/// </summary>
		private List<ChessMove> GetAllBishopAttackMoves(int player)
		{
			//create a list that will hold all the moves
			List<ChessMove> allMoves = new List<ChessMove>();

			//gets the current location of one of the bishops
			var currentBishopPosistions = GetPositionsOfPiece(ChessPieceType.Bishop, player);

			//for both bishops
			foreach( BoardPosition currentBishop in currentBishopPosistions)
			{
				//get all the attack moves
				var moves = DiagonalAttackMoves(currentBishop, ChessPieceType.Bishop, player);
				//add the list of moves to the total list of moves
				allMoves.AddRange(moves);
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all the current knight attack moves
		/// </summary>
		private List<ChessMove> GetAllKnightAttackMoves(int player)
		{
			List<ChessMove> allMoves = new List<ChessMove>();

			var currentKnightPosistions = GetPositionsOfPiece(ChessPieceType.Knight, player);

			foreach(BoardPosition currentKnight in currentKnightPosistions)
			{
				var moves = KnightAttackMoves(currentKnight, player);
				allMoves.AddRange(moves);
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all the current rook attack moves
		/// </summary>
		private List<ChessMove> GetAllRookAttackMoves(int player)
		{
			List<ChessMove> allMoves = new List<ChessMove>();

			var currentRookPosistions = GetPositionsOfPiece(ChessPieceType.Rook, player);

			foreach (BoardPosition currentRook in currentRookPosistions)
			{
				//gets horizontal and verical attack moves
				var horizMoves = HorizontalAttackMoves(currentRook, ChessPieceType.Rook, player);
				allMoves.AddRange(horizMoves);
				var vertMoves = VerticalAttackMoves(currentRook, ChessPieceType.Rook, player);
				allMoves.AddRange(vertMoves);

			}
			return allMoves;
		}

		/// <summary>
		/// Gets all the current Queen attack moves
		/// </summary>
		private List<ChessMove> GetAllQueenAttackMoves(int player)
		{
			List<ChessMove> allMoves = new List<ChessMove>();

			var currentQueenPosistions = GetPositionsOfPiece(ChessPieceType.Queen, player);

			foreach (BoardPosition currentQueen in currentQueenPosistions)
			{
				//all directions
				var horizMoves = HorizontalAttackMoves(currentQueen, ChessPieceType.Queen, player);
				allMoves.AddRange(horizMoves);
				var vertMoves = VerticalAttackMoves(currentQueen, ChessPieceType.Queen, player);
				allMoves.AddRange(vertMoves);
				var diagMove = DiagonalAttackMoves(currentQueen, ChessPieceType.Queen, player);
				allMoves.AddRange(diagMove);
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all the current King attack moves
		/// </summary>
		private List<ChessMove> GetAllKingAttackMoves(int player)
		{
			List<ChessMove> allMoves = new List<ChessMove>();

			var currentKingPosistions = GetPositionsOfPiece(ChessPieceType.King, player);

			foreach (BoardPosition currentKing in currentKingPosistions)
			{
				//all directions but only once
				var horizMoves = HorizontalAttackMoves(currentKing, ChessPieceType.King, player);
				allMoves.AddRange(horizMoves);
				var vertMoves = VerticalAttackMoves(currentKing, ChessPieceType.King, player);
				allMoves.AddRange(vertMoves);
				var diagMove = DiagonalAttackMoves(currentKing, ChessPieceType.King, player);
				allMoves.AddRange(diagMove);
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all the current pawn attack moves
		/// </summary>
		private List<ChessMove> GetAllPawnAttackMoves(int player)
		{
			//create a list that will hold all the moves
			List<ChessMove> allMoves = new List<ChessMove>();

			//gets the current location of all the pawns
			var currentPawnPosistions = GetPositionsOfPiece(ChessPieceType.Pawn, player);

			//for all pawns
			foreach (BoardPosition currentPawn in currentPawnPosistions)
			{
				//get all the attack moves
				var moves = PawnAttackPositions(currentPawn, player);
				//add the list of moves to the total list of moves
				allMoves.AddRange(moves);
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all of the possible pawn attack moves
		/// </summary>
		private List<ChessMove> GetPossiblePawnAttacks(int player)
		{
			//create a list that will hold all the moves
			List<ChessMove> allMoves = new List<ChessMove>();

			//gets the current location of one of the bishops
			var attackMoves = GetAllPawnAttackMoves(player);

			//check pawn promotion and enpassant
			foreach (var attack in attackMoves)
			{
				//gets the positions of the enpassant enemy pieces
				BoardPosition enPaLeft = attack.StartPosition.Translate(0, -1);
				BoardPosition enPaRight = attack.StartPosition.Translate(0, 1);
				//usde to test validity
				BoardPosition testDown = attack.EndPosition.Translate(1, 0);
				//player white
				if (player == 1)
				{
					//if the end position of the move is an enemy
					if (PositionIsEnemy(attack.EndPosition, player))
					{
						//if the end position is the end of the board on the enemy side
						if (attack.EndPosition.Row == 0)
						{
							//creates a new chess moves that will promote a pawn to the queen
							ChessMove promoteQueen = new ChessMove(attack.StartPosition, attack.EndPosition, ChessPieceType.Queen)
							{
								Player = player
							};
							ChessMove promoteRook = new ChessMove(attack.StartPosition, attack.EndPosition, ChessPieceType.Rook)
							{
								Player = player
							};
							ChessMove promoteKnight = new ChessMove(attack.StartPosition, attack.EndPosition, ChessPieceType.Knight)
							{
								Player = player
							};
							ChessMove promoteBishop = new ChessMove(attack.StartPosition, attack.EndPosition, ChessPieceType.Bishop)
							{
								Player = player
							};
							//adds all the moves to the possible moves
							allMoves.Add(promoteQueen);
							allMoves.Add(promoteRook);
							allMoves.Add(promoteKnight);
							allMoves.Add(promoteBishop);
						}
						else
						{
							//add moves of regular pawn attacks
							allMoves.Add(attack);
						}
					}
					//if an enpassant is possible, can only happen on a certain row for each player
					else if(attack.StartPosition.Row == 3)
					{
						//if there is move history
						if (mMoveHistory.Any())
						{
							//if the end postion of the attack is empty and that there is a piece at the spot next to the starting pawn
							if (PositionIsEmpty(attack.EndPosition) && !PositionIsEmpty(testDown))
							{
								//gets the pieces to the left and right of the starting pawn
								var enemyRPiece = GetPieceAtPosition(enPaRight);
								var enemyLPiece = GetPieceAtPosition(enPaLeft);
								//gets the last move in the move history
								ChessMove prevMove = mMoveHistory.Last();
								//if the position next to the starting pawn is an enemy and it is a pawn also
								if (PositionIsEnemy(enPaRight, player) && enemyRPiece.PieceType == ChessPieceType.Pawn)
								{
									//if the last moves starting point was at its initial state on the board and it moves to its current spot next to 
									//the current players pawn and that the columns are the same for the ending postion of the attacker and the captured pawn
									if (prevMove.StartPosition.Equals(enPaRight.Translate(-2, 0)) && prevMove.EndPosition.Equals(enPaRight)
										&& attack.EndPosition.Col.Equals(prevMove.EndPosition.Col))
									{
										//create the enpassant move
										ChessMove moveUpRight = new ChessMove(attack.StartPosition, attack.EndPosition, ChessMoveType.EnPassant)
										{
											Player = player,
											PieceType = ChessPieceType.Pawn,
										};
										//add to the list
										allMoves.Add(moveUpRight);
									}
								}
								//if the enpassant happens on the left side of the current players pawn
								if (PositionIsEnemy(enPaLeft, player) && enemyLPiece.PieceType == ChessPieceType.Pawn)
								{
									if (prevMove.StartPosition.Equals(enPaLeft.Translate(-2, 0)) && prevMove.EndPosition.Equals(enPaLeft)
										&& attack.EndPosition.Col.Equals(prevMove.EndPosition.Col))
									{
										ChessMove moveUpLeft = new ChessMove(attack.StartPosition, attack.EndPosition, ChessMoveType.EnPassant)
										{
											Player = player,
											PieceType = ChessPieceType.Pawn,
										};
										allMoves.Add(moveUpLeft);
									}
								}
							}
						}
					}
				}
				else //black side
				{
					if (PositionIsEnemy(attack.EndPosition, player))
					{
						if (attack.EndPosition.Row == 7)
						{
							//creates a new chess moves that will promote a pawn to the queen
							ChessMove promoteQueen = new ChessMove(attack.StartPosition, attack.EndPosition, ChessPieceType.Queen)
							{
								Player = player
							};
							ChessMove promoteRook = new ChessMove(attack.StartPosition, attack.EndPosition, ChessPieceType.Rook)
							{
								Player = player
							};
							ChessMove promoteKnight = new ChessMove(attack.StartPosition, attack.EndPosition, ChessPieceType.Knight)
							{
								Player = player
							};
							ChessMove promoteBishop = new ChessMove(attack.StartPosition, attack.EndPosition, ChessPieceType.Bishop)
							{
								Player = player
							};
							allMoves.Add(promoteQueen);
							allMoves.Add(promoteRook);
							allMoves.Add(promoteKnight);
							allMoves.Add(promoteBishop);
						}
						else
						{
							allMoves.Add(attack);
						}
					}else if(attack.StartPosition.Row == 4)
					{
						if (PositionIsEmpty(attack.EndPosition) && !PositionIsEmpty(testDown))
						{
							var enemyRPiece = GetPieceAtPosition(enPaRight);
							if (PositionIsEnemy(enPaRight, player) && enemyRPiece.PieceType == ChessPieceType.Pawn)
							{
								ChessMove prevMove = mMoveHistory.Last();
								if (prevMove.StartPosition.Equals(enPaRight.Translate(2, 0)) && prevMove.EndPosition.Equals(enPaRight)
									&& attack.EndPosition.Col.Equals(prevMove.EndPosition.Col))
								{
									ChessMove moveDownRight = new ChessMove(attack.StartPosition, attack.EndPosition, ChessMoveType.EnPassant)
									{
										Player = player,
										PieceType = ChessPieceType.Pawn,
									};
									allMoves.Add(moveDownRight);
								}
							}
						}
						else if (PositionIsEmpty(attack.EndPosition))
						{
							var enemyLPiece = GetPieceAtPosition(enPaLeft);

							if (PositionIsEnemy(enPaLeft, player) && enemyLPiece.PieceType == ChessPieceType.Pawn)
							{
								ChessMove prevMove = mMoveHistory.Last();
								if (prevMove.StartPosition.Equals(enPaLeft.Translate(2, 0)) && prevMove.EndPosition.Equals(enPaLeft)
									&& attack.EndPosition.Col.Equals(prevMove.EndPosition.Col))
								{
									ChessMove moveDownLeft = new ChessMove(attack.StartPosition, attack.EndPosition, ChessMoveType.EnPassant)
									{
										Player = player,
										PieceType = ChessPieceType.Pawn,
									};
									allMoves.Add(moveDownLeft);
								}
							}
						}
					}		
				}
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all of the possible Rook attack moves
		/// </summary>
		private List<ChessMove> GetPossibleRookAttacks(int player)
		{
			//creates a list for all the possible moves
			List<ChessMove> allMoves = new List<ChessMove>();
			//gets all the valid attacks
			var validAttacks = GetAllRookAttackMoves(player);
			//for each attack in all of the valid attacks
			foreach (var attack in validAttacks)
			{
				//if the attack end postion is empty or an enemy
				if(PositionIsEmpty(attack.EndPosition) || PositionIsEnemy(attack.EndPosition, player))
				{
					//add the move
					allMoves.Add(attack);
				}
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all of the possible Knight attack moves
		/// </summary>
		private List<ChessMove> GetPossibleKnightAttacks(int player)
		{
			List<ChessMove> allMoves = new List<ChessMove>();
			var validAttacks = GetAllKnightAttackMoves(player);

			foreach (var attack in validAttacks)
			{
				if (PositionIsEmpty(attack.EndPosition) || PositionIsEnemy(attack.EndPosition, player))
				{
					allMoves.Add(attack);
				}
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all of the possible Bishop attack moves
		/// </summary>
		private List<ChessMove> GetPossibleBishopAttacks(int player)
		{
			List<ChessMove> allMoves = new List<ChessMove>();
			var validAttacks = GetAllBishopAttackMoves(player);

			foreach (var attack in validAttacks)
			{
				if (PositionIsEmpty(attack.EndPosition) || PositionIsEnemy(attack.EndPosition, player))
				{
					allMoves.Add(attack);
				}
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all of the possible Queen attack moves
		/// </summary>
		private List<ChessMove> GetPossibleQueenAttacks(int player)
		{
			List<ChessMove> allMoves = new List<ChessMove>();
			var validAttacks = GetAllQueenAttackMoves(player);

			foreach (var attack in validAttacks)
			{
				if (PositionIsEmpty(attack.EndPosition) || PositionIsEnemy(attack.EndPosition, player))
				{
					allMoves.Add(attack);
				}
			}
			return allMoves;
		}

		/// <summary>
		/// Gets all of the possible King attack moves
		/// </summary>
		private List<ChessMove> GetPossibleKingAttacks(int player)
		{
			List<ChessMove> allMoves = new List<ChessMove>();
			var validAttacks = GetAllKingAttackMoves(player);

			foreach (var attack in validAttacks)
			{
				if (PositionIsEmpty(attack.EndPosition) || PositionIsEnemy(attack.EndPosition, player))
				{
					allMoves.Add(attack);
				}
			}
			return allMoves;
		}

		#endregion

		#region Explicit IGameBoard implementations.
		IEnumerable<IGameMove> IGameBoard.GetPossibleMoves() {
			return GetPossibleMoves();
		}
		void IGameBoard.ApplyMove(IGameMove m) {
			ApplyMove(m as ChessMove);
		}
		IReadOnlyList<IGameMove> IGameBoard.MoveHistory => mMoveHistory;

		/// <summary>
		/// Determines the weight of the current state of the board. Used for AI.
		/// </summary>
		public long BoardWeight
		{
			get {
				long wPawnsPts = 0;
				long bPawnsPts = 0;

				//gets each pawn position on the board
				var wPawnPos = GetPositionsOfPiece(ChessPieceType.Pawn, 1);
				var bPawnPos = GetPositionsOfPiece(ChessPieceType.Pawn, 2);

				//goes through each pos and determines how many spaces that pawn has moved forward
				foreach(var pos in wPawnPos)
				{
					//gets the number of spaces the white pawn has moved from the start row of 6
					wPawnsPts += (6 - pos.Row);
				}
				foreach(var pos in bPawnPos)
				{
					bPawnsPts += (pos.Row - 1);
				}

				//uses a new method that gets all of the attack positions. It duplicates an attacked
				//pos if more than one piece can attack it
				var wDefPos = GetAllAttackedPositions(1);
				var bDefPos = GetAllAttackedPositions(2);

				//accumulation of protecting (defense) points
				long wDefPts = 0;
				long bDefPts = 0;

				//go through each attack position of the white player and addes a point for each knight
				//or bishop that is under attack or protected by a friendly piece
				foreach (var pos in wDefPos)
				{
					//gets the current piece at the current pos
					var currentPiece = GetPieceAtPosition(pos);
					//if it is a friendly piece and if its a knight or bishop
					if (currentPiece.Player == 1 && (currentPiece.PieceType == ChessPieceType.Knight ||
							currentPiece.PieceType == ChessPieceType.Bishop))
					{
						//add 1 point if the piece is being protected
						wDefPts++;
					}
				}

				foreach (var pos in bDefPos)
				{
					//gets the current piece at the current pos
					var currentPiece = GetPieceAtPosition(pos);
					//if it is a friendly piece and if its a knight or bishop
					if (currentPiece.Player == 2 && (currentPiece.PieceType == ChessPieceType.Knight ||
							currentPiece.PieceType == ChessPieceType.Bishop))
					{
						//add 1 point if the piece is being protected
						bDefPts++;
					}
				}

				//gets all the positions that are attacked by the given player
				var wAtkPos = GetAttackedPositions(1);
				var bAtkPos = GetAttackedPositions(2);

				//accumulation of attack points
				long wAtkPts = 0;
				long bAtkPts = 0;

				//go through each attack position of the white player and addes the points for each
				//enemy piece it can attack.
				foreach (var pos in wAtkPos)
				{
					//gets the current piece at the current pos
					var currentPiece = GetPieceAtPosition(pos);
					//if it is an enemy
					if(currentPiece.Player == 2)
					{
						if (currentPiece.PieceType == ChessPieceType.Knight || 
							currentPiece.PieceType == ChessPieceType.Bishop)
						{
							//add 1 point if the attack piece is a knight or bishop
							wAtkPts++;
						}
						else if (currentPiece.PieceType == ChessPieceType.Rook)
						{
							//add 2 points if the attack piece is a rook
							wAtkPts += 2;
						}
						else if (currentPiece.PieceType == ChessPieceType.Queen)
						{
							//add 5 points if the attack piece is a rook
							wAtkPts += 5;
						}
						else if (currentPiece.PieceType == ChessPieceType.King)
						{
							//add 4 points if the attack piece is a rook
							wAtkPts += 4;
						}
					}
				}

				//black attack positions
				foreach (var pos in bAtkPos)
				{
					var currentPiece = GetPieceAtPosition(pos);
					if (currentPiece.Player == 1)
					{
						if (currentPiece.PieceType == ChessPieceType.Knight ||
							currentPiece.PieceType == ChessPieceType.Bishop)
						{
							bAtkPts++;
						}
						else if (currentPiece.PieceType == ChessPieceType.Rook)
						{
							bAtkPts += 2;
						}
						else if (currentPiece.PieceType == ChessPieceType.Queen)
						{
							bAtkPts += 5;
						}
						else if (currentPiece.PieceType == ChessPieceType.King)
						{
							bAtkPts += 4;
						}
					}
				}

				//adds up all points
				long wWeight = wPawnsPts + wAtkPts + wDefPts;
				long bWeight = bPawnsPts + bAtkPts + bDefPts;
				long totalWeight = wWeight - bWeight;

				return (CurrentAdvantage.Player == 1) ? totalWeight += CurrentAdvantage.Advantage : totalWeight -= CurrentAdvantage.Advantage;
			}
				
		}
		#endregion

		// Chess board constructor
		public ChessBoard() {
			//starts the game with white, player 1
			currentPlayer = 1;
			//the game cant be in check at the beginning
			GameInCheck = false;
			//creates a new draw counter and puts zero in it
			DrawCount = new List<int>();
			DrawCount.Add(0);
		}

		//Chess board constructor with a tuple parameter
		public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiece>> startingPositions)
			: this() {
			var king1 = startingPositions.Where(t => t.Item2.Player == 1 && t.Item2.PieceType == ChessPieceType.King);
			var king2 = startingPositions.Where(t => t.Item2.Player == 2 && t.Item2.PieceType == ChessPieceType.King);
			if (king1.Count() != 1 || king2.Count() != 1) {
				throw new ArgumentException("A chess board must have a single king for each player");
			}

			foreach (var position in BoardPosition.GetRectangularPositions(8, 8)) {
				SetPieceAtPosition(position, ChessPiece.Empty);
			}

			int[] values = { 0, 0 };

			//sets a new advantage
			Advantage = new GameAdvantage(0, 0);
			//game just started
			GameInCheck = false;
			//sets the draw counter to zero
			DrawCount = new List<int>();
			DrawCount.Add(0);
			//used for the king location
			var king = new BoardPosition(0, 0);
			foreach (var pos in startingPositions) {
				//if the starting postion is equal to a type king and is the current player
				if(pos.Item2.PieceType == ChessPieceType.King && pos.Item2.Player == currentPlayer)
				{
					//the position of the knig
					king = pos.Item1;
				}
				//sets the piece at the positon
				SetPieceAtPosition(pos.Item1, pos.Item2);
				//calculates the current advantage
				Advantage = CurrentGameAdvantage();;			
			}
			//the game is in check when the king is under attack by the enemy
			GameInCheck = PositionIsAttacked(king, 2);
		}
	}
}
