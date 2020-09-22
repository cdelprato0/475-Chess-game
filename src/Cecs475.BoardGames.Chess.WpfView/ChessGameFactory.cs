﻿using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Cecs475.BoardGames.Chess.WpfView
{
    public class ChessGameFactory : IWpfGameFactory
    {
        public string GameName => "Chess";

        public IValueConverter CreateBoardAdvantageConverter()
        {
            return new ChessAdvantageConverter();
        }

        public IValueConverter CreateCurrentPlayerConverter()
        {
            return new ChessCurrentPlayerConverter();
        }

        public IWpfGameView CreateGameView(NumberOfPlayers players)
        {
            var view = new ChessView();
            view.ChessVM.Players = players;
            return view;
        }
    }
}
