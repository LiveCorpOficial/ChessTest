using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    class Board
    {
        public string fen { get; private set; } 
        Figure[,] figures;                              // массив всех фигур
        public Color moveColor { get; private set; }    // чей ход
        public int moveNumber { get; private set; }     // номер хода
        public Board(string fen)                        // создает новую матрицу из всех фигур 8 на 8
        {
            this.fen = fen;
            figures = new Figure[8, 8];
            Init();                                      // будет смотреть на фен и инициализировать расположение всех фигур
        }
        void Init()
        {
            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            string[] parts = fen.Split();
            if (parts.Length != 6) return;
            InitFigures(parts[0]);
            moveColor = (parts[1] == "b") ? Color.black : Color.white;
            moveNumber = int.Parse(parts[5]);
        }

        void InitFigures (string data)
        {
            for(int j = 8; j >= 2; j--)
            {
                data = data.Replace(j.ToString(), (j - 1).ToString() + "1");
            }
            data = data.Replace("1", ".");
            string[] lines = data.Split('/');
            for (int y = 7; y >= 0; y--)
                for (int x = 0; x < 8; x++)
                    figures[x, y] = lines[7 - y][x] == '.' ? Figure.none : 
                            (Figure)lines[7 - y][x];
        }

        public IEnumerable<FigureOnSquare> YieldFigures()
        {
            foreach (Square square in Square.YieldSquares())
                if (GetFigureAt(square).GetColor() == moveColor)
                    yield return new FigureOnSquare(GetFigureAt(square), square);
        }

        void GenerateFEN()
        {
            fen = FenFigures() + " " +
                (moveColor == Color.white ? "w" : "b") + 
                " - - 0 " + moveNumber.ToString();
        }


        string FenFigures()
        {
            StringBuilder sb = new StringBuilder();
            for(int y = 7; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                    sb.Append(figures[x, y] == Figure.none ? '1' : (char)figures[x, y]);
                if (y > 0)
                    sb.Append('/');
            }
            string eight = "11111111";
            for (int j = 8; j >= 2; j--)
                sb.Replace(eight.Substring(0, j), j.ToString());
            return sb.ToString();
        }

        public Figure GetFigureAt(Square square)        // Получает фигуру, стоящую на определенной клетке
        {
            if (square.OnBoard())   // находится ли клетка на доске
                return figures[square.x, square.y];
            return Figure.none;
        }

        void SetFigureAt (Square square, Figure figure)    
        {
            if (square.OnBoard())
                figures[square.x, square.y] = figure;
        }

        public Board Move (FigureMoving fm)  // Перемещение фигуры
        {

            Board next = new Board(fen);
            next.SetFigureAt(fm.from, Figure.none); // откуда пошла
            next.SetFigureAt(fm.to, fm.promotion == Figure.none ? fm.figure : fm.promotion);    // куда пошла фигура
            if (moveColor == Color.black)   // увеличим номер хода
                next.moveNumber++;
            next.moveColor = moveColor.FlipColor();     // переключаем цвет, чтобы ходел следубщий человек
            next.GenerateFEN();
            return next;
        }

        bool CanEatKing()
        {
            Square badKing = FindBadKing();
            Moves moves = new Moves(this);                      // Перебираем все фигуры и идём на координаты плохого короля
            foreach(FigureOnSquare fs in YieldFigures())
            {
                FigureMoving fm = new FigureMoving(fs, badKing);
                if (moves.CanMove(fm))
                    return true;
            }
            return false;
        }

        private Square FindBadKing()
        {
            Figure badKing = moveColor == Color.black ? Figure.whiteKing : Figure.blackKing;
            foreach (Square square in Square.YieldSquares())
                if (GetFigureAt(square) == badKing)
                    return square;
            return Square.none;
        }

        // проверка на шах
        public bool IsCheck()
        {
            Board after = new Board(fen);
            after.moveColor = moveColor.FlipColor();
            return after.CanEatKing();
        }
        public bool IsCheckAfterMove (FigureMoving fm)
        {
            Board after = Move(fm);
            return after.CanEatKing();
        }
    }
}
