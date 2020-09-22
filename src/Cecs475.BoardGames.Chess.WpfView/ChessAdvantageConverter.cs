using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Cecs475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Determines the player and determines the advantage for that player
    /// </summary>
    public class ChessAdvantageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //game advangtage
            var v = (GameAdvantage)value;
            //player color
            string player = v.Player == 1 ? "White" : "Black";
            //if theres a tie game
            if (v.Advantage == 0)
                return "Tie game";
            else
            {
                //returns the player with their advantage
                return $"{player} has a +{v.Advantage} advantage";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
