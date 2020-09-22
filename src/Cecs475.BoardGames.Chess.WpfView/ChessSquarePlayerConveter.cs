using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.Chess.Model;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cecs475.BoardGames.Chess.WpfView
{
	public class ChessSquarePlayerConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//gets the chess piece of the square
			ChessPiece c = (ChessPiece)value;
			//determines the player of that piece
			int player = c.Player;
			//creates a new image
			Image img = new Image();

			//if the square is empty
			if (player == 0)
			{
				return null;
			}
			else if (c.PieceType != ChessPieceType.Empty)
			{
				//if the piece is a pawn
				if (c.PieceType == ChessPieceType.Pawn)
				{
					//checks the player
					if (c.Player == 1)
					{
						//adds the pawn image to the board
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_pawn.png", UriKind.Relative));
					}
					else if (c.Player == 2)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_pawn.png", UriKind.Relative));
					}
				}

				if (c.PieceType == ChessPieceType.King)
				{
					if (c.Player == 1)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_king.png", UriKind.Relative));
					}
					else if (c.Player == 2)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_king.png", UriKind.Relative));
					}
				}
				if (c.PieceType == ChessPieceType.Rook)
				{
					if (c.Player == 1)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_rook.png", UriKind.Relative));
					}
					else if (c.Player == 2)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_rook.png", UriKind.Relative));
					}
				}
				if (c.PieceType == ChessPieceType.Queen)
				{
					if (c.Player == 1)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_queen.png", UriKind.Relative));
					}
					else if (c.Player == 2)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_queen.png", UriKind.Relative));
					}
				}
				if (c.PieceType == ChessPieceType.Knight)
				{
					if (c.Player == 1)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_knight.png", UriKind.Relative));
					}
					else if (c.Player == 2)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_knight.png", UriKind.Relative));
					}
				}
				if (c.PieceType == ChessPieceType.Bishop)
				{
					if (c.Player == 1)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_bishop.png", UriKind.Relative));
					}
					else if (c.Player == 2)
					{
						img.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_bishop.png", UriKind.Relative));
					}
				}
			}

			return img;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}