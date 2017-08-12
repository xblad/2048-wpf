using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

using System.Windows.Navigation;
using System.Windows;
using System.Windows.Controls;

using Game2048.CustomControls;
using Game2048.Other;
using Game2048.Data;
using Game2048.ViewModels;

namespace Game2048.Objects
{
    public class GameBoard
    {
        public const int BASE = 2; // smallest possible tile basic number
        public const int STARTUP_MAX_POWER = 2; // maximum power of smallest possible tile on startup
        private static double[] STARTUP_PDF = new double[STARTUP_MAX_POWER] { .8, .2 }; // probability density function of smallest possible tile basic number power

        public static Grid gameBoardGrid; // instance of GameBoardGrid

        #region GameStateHandling
        public static GameState? CurrentGameState { get; private set; }

        public static void SetGameStateInProgress()
        {
            if (CurrentGameState != GameState.GameOver) // Game State "InProgress" can't be set after GameOver
            {
                CurrentGameState = GameState.InProgress;
                currentScore = LocalDataStorage.GetCurrentScore(); // load current score from local storage
                bestScore = LocalDataStorage.GetBestScore(); // load best score from local storage
            }
        }

        public static void SetGameStatePaused()
        {
            if (CurrentGameState != GameState.GameOver) // Game State "Paused" can't be set after GameOver
            {
                CurrentGameState = GameState.Paused;
            }
        }

        public static void SetGameStateNewGame()
        {
            CurrentGameState = GameState.NewGame;
            currentScore = 0; // drop current score
            bestScore = LocalDataStorage.GetBestScore(); // load best score from local storage
        }

        public static bool SetGameStateGameOver()
        {
            CurrentGameState = GameState.GameOver;
            GamePage.AddGameOverWindow(); // show up game over window
            GamePageViewModel.RemoveAllMoveCommandsFromWindow(); // don't allow user to move tiles
            return true;
        }

        public static bool CheckAndSetGameStateGameOver(TilesMovePlan mp)
        {
            if (mp.IsBlindAlleyForAnyDirection()) // check all posible directions
            {
                return SetGameStateGameOver();
            }

            return false;
        }

        public enum GameState // game states
        {
            NewGame = 0,
            InProgress = 1,
            Paused = 2,
            GameOver = 3,
        }
        #endregion

        #region Score Handling

        private static long currentScore;
        private static long bestScore;

        public static long AddScore(int score) // increases actual total score
        {
            return currentScore += score;
        }

        public static long GetCurrentScore() // returns actual total score
        {
            return currentScore;
        }

        public static long CheckAndRefreshBestScore() // refresh best score only in case it smaller current total score
        {
            if (currentScore > bestScore)
            {
                LocalDataStorage.SetBestScore(currentScore);
                return bestScore = currentScore;
            }
            return bestScore;
        }

        public static long GetBestScore() // returns best score
        {
            return bestScore;
        }

        public static string ScoreFormat(long score) // formatting too high score to be more readable
        {
            if (score >= Math.Pow(10,5) && score < Math.Pow(10,8)) // from 100,000 to 9,999,999
            {
                return (score / 1000) + "K"; // integer number of thousands with "K" (e.g. from 100K to 9999K)
            }
            else if (score >= Math.Pow(10,8)) // 10,000,000 and bigger
            {
                return (score / 1000000) + "M"; // integer number of millions with "M" (e.g. from 10M)
            }

            return score.ToString(); // without formatting
        }

        #endregion

        #region GenerateTileFunctions

        public static void AddTiles(int[,] matrix) // generate tiles using matrix
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);

            Tile.Count = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (matrix[i, j] != 0)
                        GenerateTile(new Coordinates(i, j), matrix[i, j]);
                }
            }
        }

        public static void AddTiles(int count) // generate specific number of random tiles
        {
            for (int i = 0; i < count; i++)
                GenerateTile(GetGridTileMatrix());
        }

        public static Tile GenerateTile(int[,] reservedMatrix) // generate random tile in one of vacant cells
        {            
            return GenerateTile(GetPower(), reservedMatrix);
        }

        public static Tile GenerateTile(int power, int[,] reservedMatrix) // generate tile with specific power in one of vacant cells
        {
            Coordinates c = GenerateCoordinates(reservedMatrix);
            return GenerateTile(power, c);
        }

        public static Tile GenerateTile(Coordinates coordinates, int number) // generate tile with specific number in specific cell
        {
            return GenerateTile((int)Math.Log(number, BASE), coordinates);
        }

        public static Tile GenerateTile(int power, Coordinates coordinates) // generate tile with specific power in specific cell
        {
            Tile t = new Tile();
            t.CurrentState = Tile.State.Normal; // regular tile
            t.Visibility = Visibility.Hidden;
            t.Uid = Guid.NewGuid().ToString();
            t.Content = Math.Pow(BASE, power); // calc number on tile
            GamePage.SetTileStyleByPower(t, power); // set tile style depend on power

            // setting grid row and column
            Grid.SetRow(t, coordinates.Row);
            Grid.SetColumn(t, coordinates.Column);

            gameBoardGrid.Children.Add(t); // add to game grid

            t.Visibility = Visibility.Visible;
            Tile.Count++; // increasing total count of tiles
            return t;
        }

        #endregion

        #region GetPowerFunctions

        private static int GetPower() // fully random power based on default maximum power and probability density function
        {
            return GetPower(STARTUP_MAX_POWER, STARTUP_PDF);
        }

        private static int GetPower(int maxPower) // get random power with upper bound without specified probability density function
        {
            double[] PDF = new double[maxPower];
            for (int i = 0; i < maxPower; i++)
                PDF[i] = (double)1 / maxPower; // same probability of all cases
            
            return GetPower(maxPower, PDF);
        }

        private static int GetPower(int maxPower, double[] PDF) // fully random power based on specified maximum power and probability density function
        {
            Random r = Tools.CustomRandom();
            int power = r.Next(1, maxPower + 1); // random power from 1 to max power
            if (PDF.Length != maxPower || PDF.Sum() != 1) // in case PDF specified incorrect
                return power; // returns random power

            double quantile = r.NextDouble(); // get quantile (number from 0 to 1)
            for (int i = 1; i <= maxPower; i++) // look over all possible power
            {
                if (quantile <= PDF.Take(i).Sum()) // compare quantile with CDF (cumulative distribution function)
                   return i;
            }

            return power;
        }
        #endregion

        #region GenerateCoordinatesFunctions

        public static Coordinates GenerateCoordinates(int[,] reservedMatrix) // randomly choose one of vacant coordinates
        {
            List<Coordinates> vacantCoordinates = GetListOfVacantCoordinates(reservedMatrix);

            Random r = Tools.CustomRandom();
            Coordinates c = vacantCoordinates[r.Next(vacantCoordinates.Count)];
            return GenerateCoordinates(c.Row, c.Column);
        }

        public static List<Coordinates> GetListOfVacantCoordinates(int[,] reservedMatrix) // fulfill list of vacant coordinantes 
        {
            int rowNumber = reservedMatrix.GetLength(0);
            int columnNumber = reservedMatrix.GetLength(1);

            List<Coordinates> array = new List<Coordinates>();
            for (int i = 0; i < rowNumber; i++)
            {
                for (int j = 0; j < columnNumber; j++)
                {
                    if (reservedMatrix[i, j] == 0) // only empty cells
                    {
                        array.Add(new Coordinates(i, j));
                    }
                }
            }
            return array;
        }

        public static Coordinates GenerateCoordinates(int row, int column) // generate coordinates
        {
            // replace negative rows and columns with zero
            row = row < 0 ? 0 : row;
            column = column < 0 ? 0 : column;
            Coordinates c = new Coordinates(row, column);
            return c;
        }

        #endregion

        public static void SetNewCoordinatesToTilesInGrid(TilesMovePlan mp) // translate tile to new location in the context of move plan
        {
            gameBoardGrid.Children.OfType<Tile>()
                .ToList().ForEach(x => { GamePage.TranslateTile(x, mp); });
        }

        public static void UpdateTileStates(TilesMovePlan mp) // update tile state based on move plan maps
        {
            foreach (Tile t in gameBoardGrid.Children.OfType<Tile>())
            {
                if (Convert.ToBoolean(mp.DeletionMap[Grid.GetRow(t), Grid.GetColumn(t)]))
                    t.CurrentState = Tile.State.Delete;
                else if (Convert.ToBoolean(mp.DoublingMap[Grid.GetRow(t), Grid.GetColumn(t)]))
                    t.CurrentState = Tile.State.Multiply;
                else
                    t.CurrentState = Tile.State.Normal;
            }
        }

        public static void TryAgain() // refresh game board
        {
            GameBoard.SetGameStateNewGame();
            NavigationService nav = NavigationService.GetNavigationService(GamePage.GetGamePageInstance());
            nav.Refresh();
            LocalDataStorage.RefreshSaveFileXML();
        }

        public static int GetBiggestTile() // get tile with biggest number
        {
            return GamePage.GetGameBoardGrid().Children.OfType<Tile>().Max(x => x.GetNumber());
        }

        public static int[,] GetGridTileMatrix() // transform tile grid to multidimensional array
        {
            Tile tile;
            int n = gameBoardGrid.RowDefinitions.Count;
            int m = gameBoardGrid.ColumnDefinitions.Count;
            int[,] matrix = new int[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    tile = gameBoardGrid.Children.OfType<Tile>()
                            .Where(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == j).SingleOrDefault();
                    if (tile != null)
                        matrix[i,j] = tile.GetNumber();
                    else
                        matrix[i,j] = 0;
                }
            }

            return matrix;
        }
    }
}
