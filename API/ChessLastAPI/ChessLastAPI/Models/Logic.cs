using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Chess;

namespace ChessLastAPI.Models
{
    public class Logic
    {
        private ModelChessDB db;

        public Logic()
        {
            db = new ModelChessDB();
        }

        public Game GetCurrentGame()
        {
            Game game = db
                .Games
                .Where(g => g.Status == "play")
                .OrderBy(g => g.ID)
                .FirstOrDefault();
            if (game == null)
                game = CreateNewGame();
            return game;
        }

        private Game CreateNewGame()
        {
            Game game = new Game();

            Chess.Chess chess = new Chess.Chess();

            game.FEN = chess.fen;   
            game.Status = "play";

            db.Games.Add(game);
            db.SaveChanges();

            return game;
        }
    }
}