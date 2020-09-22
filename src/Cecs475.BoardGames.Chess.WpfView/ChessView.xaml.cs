using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Interaction logic for ChessView.xaml
    /// </summary>
    public partial class ChessView : UserControl, IWpfGameView
    {
        public ChessView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the mouse enters the boarder of a square
        /// </summary>
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("ViewModel") as ChessViewModel;
            var currentSq = vm.CurrentSquare;

            //if a square has been selected
            if (currentSq != null)
            {
                //highlight all of the possible moves
                if (vm.PossibleEndPositions(currentSq).Contains(square.Position))
                {
                    square.IsHighlighted = true;
                }
            }
            else
            {   
                //highlight all the possible piece that have possible moves
                if (vm.PossibleMovesByStartPos(square).Any())
                {
                    square.IsHighlighted = true;
                }
            }
        }

        /// <summary>
        /// when the mouse leaves the border of a square. set it back to default color
        /// </summary>
        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            square.IsHighlighted = false;
        }

        /// <summary>
        /// when the mouse clicks a square and clicks another square. 
        /// Implements Async.
        /// </summary>
        private async void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("ViewModel") as ChessViewModel;
            var currentSelectedSq = vm.CurrentSquare;

            //if no square has been selected
            if (currentSelectedSq == null)
            {
                //if the square is not empty
                if(square.ChessPiece.PieceType != ChessPieceType.Empty)
                {
                    //if the square is the current player
                    if(square.ChessPiece.Player == vm.CurrentPlayer)
                    {
                        //it will select the sqaure
                        square.IsHighlighted = false;
                        square.IsSelected = true;
                        vm.CurrentSquare = square;
                    }
                }
            }
            else
            {
                //determines all the possible moves from the selected square
                var currentMoves = from ChessMove m in vm.PossibleMovesByStartPos(currentSelectedSq)
                                   where m.EndPosition.Equals(square.Position)
                                   select m;
                //if the user clicks on any square besides a possible move
                if (!vm.PossibleEndPositions(currentSelectedSq).Contains(square.Position))
                {
                    //it will cancel the move
                    currentSelectedSq.IsHighlighted = false;
                    currentSelectedSq.IsSelected = false;
                    vm.CurrentSquare = null; 
                }
                else
                {
                    //if a square has already been selected, it will check  to see if there are any moves 
                    if (currentMoves.Any())
                    {
                        //it will get the first move from the current moves
                        ChessMove currentMove = currentMoves.First();
                        //if that move is a pawn promotion, it will display a new window
                        if(currentMove.MoveType == ChessMoveType.PawnPromote)
                        {
                            //creates a new pawn promote window 
                            PawnPromote pawnWindow = new PawnPromote(vm, currentSelectedSq.Position, square.Position)
                            {
                                //cannot be resized and does not have any style to it(minimize, exit)
                                ResizeMode = ResizeMode.NoResize,
                                WindowStyle = WindowStyle.None

                            };
                            //shows the window
                            pawnWindow.ShowDialog();
                        }
                        else
                        {
                            //disables the window
                            IsEnabled = false;
                            
                            //if its jest a regulare move, apply the move
                            //uses async to match the VM
                            await vm.ApplyMove(currentMove);

                            //enables the window
                            IsEnabled = true;
                        }
                        //the square is no longer highlighted
                        square.IsHighlighted = false;
                    }
                    else
                    {
                        //square is no longer selected if there are no moves
                        vm.CurrentSquare.IsSelected = false;
                    }
                }
            }
        }
        public ChessViewModel ChessVM => FindResource("ViewModel") as ChessViewModel;

        public Control ViewControl => this;

        public IGameViewModel ViewModel => ChessVM;

    }
}
