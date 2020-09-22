using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
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
using System.Windows.Shapes;

namespace Cecs475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Interaction logic for PawnPromote.xaml
    /// </summary>
    public partial class PawnPromote : Window
    {
        private ChessViewModel vm;
        private BoardPosition moveStart;
        private BoardPosition moveEnd;

        /// <summary>
        /// constructor
        /// </summary>
        public PawnPromote(ChessViewModel viewModel, BoardPosition start, BoardPosition end)
        {
            vm = viewModel;
            moveStart = start;
            moveEnd = end;

            //creates all new images
            Image queen = new Image();
            Image knight = new Image();
            Image bishop = new Image();
            Image rook = new Image();

            //if its the white player
            if (vm.CurrentPlayer == 1)
            {
                //creates images of those pieces
                queen.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_queen.png", UriKind.Relative));
                knight.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_knight.png", UriKind.Relative));
                bishop.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_bishop.png", UriKind.Relative));
                rook.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_rook.png", UriKind.Relative));
            }
            else
            {
                queen.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_queen.png", UriKind.Relative));
                knight.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_knight.png", UriKind.Relative));
                bishop.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_bishop.png", UriKind.Relative));
                rook.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_rook.png", UriKind.Relative));
            }

            //creats the window
            InitializeComponent();

            //assigns the content of the button to the pictures of the pieces
            this.Rook.Content = rook;
            this.Queen.Content = queen;
            this.Bishop.Content = bishop;
            this.Knight.Content = knight;
        }

        /// <summary>
        /// when the user clicks a button
        /// Implements Async
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            //will determine which button was clicked
            switch (b.Name)
            {
                case "Queen":
                    //applys the pawn promotion moves of the selected piece
                    //awaits since the apply move runs the AIs move in the background
                    await vm.ApplyMove(new ChessMove(moveStart, moveEnd, ChessPieceType.Queen));
                    break;
                case "Knight":
                    await vm.ApplyMove(new ChessMove(moveStart, moveEnd, ChessPieceType.Knight));
                    break;
                case "Bishop":
                    await vm.ApplyMove(new ChessMove(moveStart, moveEnd, ChessPieceType.Bishop));
                    break;
                case "Rook":
                    await vm.ApplyMove(new ChessMove(moveStart, moveEnd, ChessPieceType.Rook));
                    break;
            }
            //closes the window after the button has been clicked
            this.Close();
        }
    }
}
