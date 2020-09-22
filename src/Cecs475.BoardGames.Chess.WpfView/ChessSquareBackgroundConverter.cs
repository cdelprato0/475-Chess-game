using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Cecs475.BoardGames.Chess.WpfView
{
    class ChessSquareBackgroundConverter : IMultiValueConverter
    {
        private static SolidColorBrush LIGHT_BRUSH = Brushes.Bisque;
        private static SolidColorBrush DARK_BRUSH = Brushes.DarkGoldenrod;
        private static SolidColorBrush HOVER_BRUSH = Brushes.LightGreen;
        private static SolidColorBrush SELECTED_BRUSH = Brushes.Red;
        private static SolidColorBrush CHECK_BRUSH = Brushes.Yellow;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            BoardPosition position = (BoardPosition)values[0];
            bool isHovered = (bool)values[1];
            bool isSelected = (bool)values[2];
            bool isInCheck = (bool)values[3];
     
            //if a piece is hovered over, it will change the background
            if (isHovered)
            {
                return HOVER_BRUSH;
            }
            //if a piece is selected, it will change the background
            if (isSelected)
            {
                return SELECTED_BRUSH;
            }
            //if a king is in check, it will change the background
            if (isInCheck)
            {
                return CHECK_BRUSH;
            }

            //creates an alternating uniform board that has light and dark squares
            if (position.Row % 2 == 0)
            {
                if(position.Col % 2 == 0)
                {
                    return LIGHT_BRUSH;
                }
                else
                {
                    return DARK_BRUSH;
                }
            }
            else
            {
                if (position.Col % 2 != 0)
                {
                    return LIGHT_BRUSH;
                }
                else
                {
                    return DARK_BRUSH;
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
