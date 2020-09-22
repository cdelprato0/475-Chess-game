using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cecs475.BoardGames.ComputerOpponent;
using System.Windows;

namespace Cecs475.BoardGames.Chess.WpfView
{

    public class ChessSquare : INotifyPropertyChanged
    {
        //the chess quare has a chess piece
        private ChessPiece piece;
        //the chess square can be highlighted, selected, or be in check
        private bool squareHighlighted;
        private bool squareIsSelected;
        private bool squareIsInCheck;

        //holds the position of the square on the board
        public BoardPosition Position{get; set;}
        
        //when the property of the square has been changed
        public event PropertyChangedEventHandler PropertyChanged;

        //change the property that is related to the string passed in
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //Chess piece property
        public ChessPiece ChessPiece
        {
            get { return piece; }
            set
            {
                piece = value;
                OnPropertyChanged(nameof(ChessPiece));
            }
        }

        //Square is highlighted property
        public bool IsHighlighted
        {
            get { return squareHighlighted; }
            set
            {
                if(value != squareHighlighted)
                {
                    squareHighlighted = value;
                    OnPropertyChanged(nameof(IsHighlighted)); 
                }
            }
        }

        //square is selected property
        public bool IsSelected
        {
            get { return squareIsSelected; }
            set
            {
                if (value != squareIsSelected)
                {
                    squareIsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        //square is in check property
        public bool IsInCheck
        {
            get { return squareIsInCheck; }
            set
            {
                if (value != squareIsInCheck)
                {
                    squareIsInCheck = value;
                    OnPropertyChanged(nameof(IsInCheck));
                }
            }
        }
    }
    public class ChessViewModel : IGameViewModel, INotifyPropertyChanged
    {
        //reference to the chess board
        public ChessBoard board;

        //holds the contents of each square on the board
        private ObservableCollection<ChessSquare> squares;

        //the current looked at square
        private ChessSquare currentSquare;

        //added for AI
        private const int MAX_AI_DEPTH = 4;
        private IGameAi mGameAi = new MinimaxAi(MAX_AI_DEPTH);

        //constructor
        public ChessViewModel()
        {
            //creates a new chess board
            board = new ChessBoard();
            //determines all the pieces that are at each square
            squares = new ObservableCollection<ChessSquare>(
                BoardPosition.GetRectangularPositions(8, 8)
                .Select(position => new ChessSquare()
                {
                    Position = position,
                    ChessPiece = board.GetPieceAtPosition(position)
                }));

            //gets all the starting possible moves
            PossibleMoves = new HashSet<ChessMove>(
                from ChessMove m in board.GetPossibleMoves()
                select m);

            //no square has been selected
            currentSquare = null;
        }

        /// <summary>
        /// A collection of 64 ChessSquare objects representing the state of the 
        /// game board.
        /// </summary>
        public ObservableCollection<ChessSquare> Squares
        {
            get { return squares; }
        }

        /// <summary>
        /// A set of Chess moves where the current player can move.
        /// </summary>
        public HashSet<ChessMove> PossibleMoves
        {
            get; private set;
        }

        /// <summary>
        /// Determines all the possible moves with the starting position at the currentSq
        /// </summary>
        public IEnumerable<ChessMove> PossibleMovesByStartPos(ChessSquare currentSq)
        {
            return from ChessMove m in PossibleMoves
                   where m.StartPosition.Equals(currentSq.Position)
                   select m;
        }

        /// <summary>
        /// Determines all the end positions of the current possible moves
        /// </summary>
        public IEnumerable<BoardPosition> PossibleEndPositions(ChessSquare currentSq)
        {
            return from ChessMove m in PossibleMovesByStartPos(currentSq)
                   select m.EndPosition;
        }

        /// <summary>
        /// keeps track of the currently selected square
        /// </summary>
        public ChessSquare CurrentSquare
        {
            get { return currentSquare; }
            set
            {
                currentSquare = value;
                OnPropertyChanged(nameof(CurrentSquare));
            }
        }

        /// <summary>
        /// gets the current advantage of the board
        /// </summary>
        public GameAdvantage BoardAdvantage => board.CurrentAdvantage;

        /// <summary>
        /// gets the current player of the board
        /// </summary>
        public int CurrentPlayer => board.CurrentPlayer;

        /// <summary>
        /// gets the move history of the board and if there is any move history
        /// </summary>
        public bool CanUndo => board.MoveHistory.Any();

        //Added for AI
        public NumberOfPlayers Players { get; set; }

        //if the game is finished
        public event EventHandler GameFinished;
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

        /// <summary>
        /// Rebinds the state of the board. checks all moves and sets the board to the current state
        /// </summary>
        private void RebindState()
        {
            //gets all the new possible moves
            PossibleMoves = new HashSet<ChessMove>(
                from ChessMove m in board.GetPossibleMoves()
                select m);

            // Update the collection of squares by examining the new board state.
            var newSquares = BoardPosition.GetRectangularPositions(8, 8);
            int i = 0;
            foreach (var pos in newSquares)
            {
                //finds the piece on the board
                var chessPiece = board.GetPieceAtPosition(pos);
                //sets the piece to the GUI square
                squares[i].ChessPiece = chessPiece;
                //if the previous move has put a king in check, it will update the game
                if(board.IsCheck && chessPiece.PieceType == ChessPieceType.King && chessPiece.Player == board.CurrentPlayer)
                {
                    squares[i].IsInCheck = true;
                }
                else
                {
                    squares[i].IsInCheck = false;
                }
                //the previous move is done, so no square should be selected
                squares[i].IsSelected = false;
                i++;
            }

            //updates all of the parts of the GUI
            OnPropertyChanged(nameof(BoardAdvantage));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(CanUndo));

            //there is no current square
            currentSquare = null;
        }

        /// <summary>
        /// Applys the move to the board. Implements async so that the AI will run while the users can 
        /// still have access to the interface
        /// </summary>
        public async Task ApplyMove(ChessMove move)
        {
            //checks to makes sure that the move is a possible move
            var possMoves = board.GetPossibleMoves() as IEnumerable<ChessMove>;
            foreach(var mv in possMoves)
            {
                //if it is, then it applys the move
                if (mv.Equals(move))
                {
                    board.ApplyMove(mv);
                    break;
                }
            }
            //rebinds the state of the board to continue on to the next player/move
            RebindState();

            //added for AI
            if (Players == NumberOfPlayers.One && !board.IsFinished)
            {
                //added for async, will run the AI in the background
                var bestMove = await Task.Run(() => mGameAi.FindBestMove(board));
                if (bestMove != null)
                {
                    board.ApplyMove(bestMove as ChessMove);
                }
                RebindState();
            }

            //when the game is over
            if (board.IsFinished)
            {
                GameFinished?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Undos the previous move
        /// </summary>
        public void UndoMove()
        {
            //if there is move history
            if (CanUndo)
            {
                //undo the move
                board.UndoLastMove();
                // In one-player mode, Undo has to remove an additional move to return to the
                // human player's turn.
                if (Players == NumberOfPlayers.One && CanUndo)
                {
                    board.UndoLastMove();
                }
                //rebind the state of the board
                RebindState();
            }
        }
    }
}
