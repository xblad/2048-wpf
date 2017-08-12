using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using Game2048.ViewModels;
using Game2048.Objects;
using Game2048.CustomControls;
using Game2048.Other;
using Game2048.Data;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        public const int GAME_BOARD_SIZE = 4; // number of rows/columns for tile
        public const int STARTUP_TILE_COUNT = 2; // number of tiles appears on startup

        public const int WIN_NUMBER = 2048; // number you need to collect to win
        private static bool WasYouWinWindowShown = false; // did you see the "You Win!" window already?
        
        public GamePage()
        {
            InitializeComponent();
        }

        public static Grid GetGamePageMainGrid() // instance of game page main grid
        {
            return (Grid)GetGamePageInstance().GamePageMainGrid;
        }

        public static YouWinWindow AddYouWinWindow() // show up "You Win!" window
        {
            Grid mainGrid = GetGamePageMainGrid();
            YouWinWindow YWW;
            if (!mainGrid.Children.OfType<YouWinWindow>().Any()) // if "You Win!" window isn't shown up already
            {
                YWW = new YouWinWindow();
                mainGrid.Children.Add(YWW);
                Grid.SetRow(YWW, 1);
                Grid.SetColumn(YWW, 1);
            }
            else // else return already shown up window
            {
                YWW = mainGrid.Children.OfType<YouWinWindow>().FirstOrDefault();
            }
            WasYouWinWindowShown = true; // in any case "You Win!" window was shown up
            return YWW;
        }

        public static GameOverWindow AddGameOverWindow() // show up "Game over" window
        {
            Grid mainGrid = GetGamePageMainGrid();
            mainGrid.Children.OfType<YouWinWindow>().ToList().ForEach(x => { mainGrid.Children.Remove(x); x = null; }); // remove YouWinWinow (if any) before adding GameOverWindow

            if (!mainGrid.Children.OfType<GameOverWindow>().Any()) // if "You Win!" window isn't shown up already
            {
                GameOverWindow GOW = new GameOverWindow();
                mainGrid.Children.Add(GOW);
                Grid.SetRow(GOW, 1);
                Grid.SetColumn(GOW, 1);
                return GOW;
            }
            else // else return already shown up window
            {
                return mainGrid.Children.OfType<GameOverWindow>().FirstOrDefault();
            }
            
        }

        public static Grid GetGameBoardGrid() // return instance of gameboard grid
        {
            return (Grid)GetGamePageInstance().GameBoardGrid;
        }

        public static ResourceDictionary GetGamePageResourceDictionary() // returns instance of game page's resource dictionary
        {
            return (ResourceDictionary)GetGamePageInstance().Resources;
        }

        public static ResourceDictionary GetApplicationResourceDictionary() // returns instance of application's resource dictionary
        {
            return (ResourceDictionary)Application.Current.Resources;
        }

        public static GamePage GetGamePageInstance() // returns instance of game page
        {
            return (GamePage)((Grid)Application.Current.MainWindow.Content).Children.OfType<Frame>().FirstOrDefault().Content;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GameBoardInit(); // initialize gameboard
        }

        private void GameBoardInit()
        {
            PrepareGameBoard(GAME_BOARD_SIZE); // prepare empty gameboard
            GameBoard.gameBoardGrid = GetGameBoardGrid(); // make reference on gameboard grid in GameBoard class
            switch (LocalDataStorage.GetGameState())
            {
                // continue game
                case GameBoard.GameState.InProgress:
                case GameBoard.GameState.Paused:
                    GameBoard.AddTiles(LocalDataStorage.GetTileMatrix()); // get tile's matrix from local storage
                    break;
                // start new game
                default:
                    LocalDataStorage.ClearSaveFileBeforeNewGame();
                    GameBoard.AddTiles(STARTUP_TILE_COUNT); // randomly add specified number of tiles on gameboard
                    GameBoard.SetGameStateNewGame();
                    break;

            }
            GameBoard.SetGameStateInProgress(); // change game state on "InProgress"

            GamePageViewModel.InitMoveCommandsInWindow(); // add move commands

            GetGamePageResourceDictionary()["BestScore"] = GameBoard.ScoreFormat(GameBoard.GetBestScore()); // fulfill best score
            GetGamePageResourceDictionary()["CurrentScore"] = GameBoard.ScoreFormat(GameBoard.GetCurrentScore()); // fulfill current score
            LocalDataStorage.RefreshSaveFileXML();
        }

        private void PrepareGameBoard(int size) // prepare square empty gameboard
        {
            PrepareGameBoard(size, size);
        }

        private void PrepareGameBoard(int rowCount, int columnCount) // prepare empty gameboard with specified row and column counts
        {
            // Row and column definitions
            for (int i = 0; i < rowCount; i++)
                GameBoardGrid.RowDefinitions.Add(new RowDefinition());
            for (int j = 0; j < columnCount; j++)
                GameBoardGrid.ColumnDefinitions.Add(new ColumnDefinition());

            //Tile vocant places graphics
            Style tilePlaceholderStyle = (Style)this.Resources["TilePlaceholderStyle"];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    Border tilePlaceholder = new Border();
                    tilePlaceholder.Style = tilePlaceholderStyle;
                    Grid.SetRow(tilePlaceholder, i);
                    Grid.SetColumn(tilePlaceholder, j);
                    GameBoardGrid.Children.Add(tilePlaceholder);
                }
            }
        }

        public static Tile AddNewTileOnBoard(TilesMovePlan mp) // add new tile in context of specified move plan
        {
            Tile t = null;
            if (!GameBoard.CheckAndSetGameStateGameOver(mp)) // don't move, if move is impossible
            {
                t = AddNewTileOnBoard(mp.DestinationMatrix);

                mp.SourceMatrix = mp.DestinationMatrix;
                mp.SourceMatrix[Grid.GetRow(t), Grid.GetColumn(t)] = t.GetNumber();
                GameBoard.CheckAndSetGameStateGameOver(mp); // check potencial moves after move performance
            }

            return t;
        }

        public static Tile AddNewTileOnBoard(int[,] reservedMatrix) // add new tile in one of vocant places
        {
            Tile t = GameBoard.GenerateTile(reservedMatrix);
            return t;
        }

        public static void Move(MovesHandling.MoveDirection direction) // move tiles in one of specified directions
        {
            try
            {
                if (GameBoard.CurrentGameState != GameBoard.GameState.GameOver) // don't move in case of gameover
                {
                    TilesMovePlan mp = MovesHandling.GetTilesMovePlan(direction); // get move plan in context of specified direction
                    if (!GameBoard.CheckAndSetGameStateGameOver(mp) && !mp.IsBlindAlley()) // if move is possible in some direction and also possible in specified direction
                    {
                        MovesHandling.GetTIlesDistinationCoordinates(mp); // perform all calculations for move plan
                        GameBoard.UpdateTileStates(mp); // change state of tiles if needed
                        GameBoard.SetNewCoordinatesToTilesInGrid(mp); // perform move graphically
                        AddNewTileOnBoard(mp); // add one tile after each move
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.Print("Wow, wow, wow... slow down, cowboy!"); // ignoring extremely fast keyboard actions
            }
        }

        public static Storyboard TranslateTile(Tile tile, TilesMovePlan mp)
        {
            Border destinationBorder = GetDestinationBorder(tile, mp); // reference object used for movement performance
            Point diff = destinationBorder.TranslatePoint(new Point(0, 0), tile); // difference in pixels between tile and that destination

            if (tile.CurrentState == Tile.State.Multiply)
                Panel.SetZIndex(tile, 1000); // set multiply tiles on top

            // add translation to tile
            TransformGroup tg = new TransformGroup();
            TranslateTransform t = new TranslateTransform();
            tg.Children.Add(t);
            tile.RenderTransform = tg;

            // ease function
            CubicEase easeFunction = new CubicEase();
            easeFunction.EasingMode = EasingMode.EaseIn;

            //animations
            DoubleAnimation TranslateTileXAnimation = new DoubleAnimation();
            TranslateTileXAnimation.BeginTime = TimeSpan.FromSeconds(0);
            TranslateTileXAnimation.Duration = new Duration(TimeSpan.FromSeconds(.05));
            TranslateTileXAnimation.EasingFunction = easeFunction;

            DoubleAnimation TranslateTileYAnimation = new DoubleAnimation();
            TranslateTileYAnimation.BeginTime = TimeSpan.FromSeconds(0);
            TranslateTileYAnimation.Duration = new Duration(TimeSpan.FromSeconds(.05));
            TranslateTileYAnimation.EasingFunction = easeFunction;

            // storyboard
            Storyboard TranslateTileStoryboard = new Storyboard();
            Storyboard.SetTarget(TranslateTileXAnimation, tile);
            Storyboard.SetTarget(TranslateTileYAnimation, tile);
            Storyboard.SetTargetProperty(TranslateTileXAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
            Storyboard.SetTargetProperty(TranslateTileYAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
            TranslateTileStoryboard.Children.Add(TranslateTileXAnimation);
            TranslateTileStoryboard.Children.Add(TranslateTileYAnimation);
            TranslateTileStoryboard.Completed += new EventHandler((s, e) => TranslateBoxStoryBoard_Completed(s, e, tile, destinationBorder));

            TranslateTileXAnimation.To = diff.X;
            TranslateTileYAnimation.To = diff.Y;

            TranslateTileStoryboard.Begin();

            return TranslateTileStoryboard;
        }

        private static Border GetDestinationBorder(Tile tile, TilesMovePlan mp) // use tile placeholder as reference for tile
        {
            Coordinates dc = mp.TileDistinationCoordinates[Grid.GetRow(tile), Grid.GetColumn(tile)];
            Border border = GameBoard.gameBoardGrid.Children.OfType<Border>()
                .Where(x => Grid.GetRow(x) == dc.Row && Grid.GetColumn(x) == dc.Column)
                .SingleOrDefault();
            return border;
        }

        private static void TranslateBoxStoryBoard_Completed(object sender, EventArgs e, Tile tile, Border border)
        {
            // set new grid position
            Grid.SetRow(tile, Grid.GetRow(border));
            Grid.SetColumn(tile, Grid.GetColumn(border));
            tile.RenderTransform = null; // delete transform from tile

            if (tile.CurrentState == Tile.State.Multiply) 
            {
                Tile.SetIsNumberChanged(tile, false); // refresh dependency property IsNumberChanged
                tile.Content = 2 * tile.GetNumber(); // multiply number on tile
                GamePage.SetTileStyle(tile); // change style depends on stored number
                GameBoard.AddScore(tile.GetNumber()); // increase total actual score
                Tile.SetIsNumberChanged(tile, true); // inform program that number was changed
                Panel.SetZIndex(tile, 0); // clean up z-index

                if (!WasYouWinWindowShown && tile.GetNumber() == WIN_NUMBER) // if you collect 2048 firstly
                {
                    AddYouWinWindow(); // show up "You win!" window
                }
            }

            // deletion of tile with state "delete"
            if (tile.CurrentState == Tile.State.Delete) { GameBoard.gameBoardGrid.Children.Remove(tile); tile = null; Tile.Count--; }
            
            // refresh scores
            GetGamePageResourceDictionary()["CurrentScore"] = GameBoard.ScoreFormat(GameBoard.GetCurrentScore());
            GetGamePageResourceDictionary()["BestScore"] = GameBoard.ScoreFormat(GameBoard.CheckAndRefreshBestScore());
        }

        private static void SaveScreenshotToClipboard() // save screenshot to clipboard
        {
            Clipboard.SetImage(ScreenCapturer.GetActiveWindowBitmapSource());
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            GamePageViewModel.RemoveAllMoveCommandsFromWindow();
        }

        public static void SetTileStyle(Tile tile) // set tile style using number on tile
        {
            int number = tile.GetNumber();
            SetTileStyleByNumber(tile, number);
        }
        
        public static void SetTileStyleByPower(Tile tile, int power) // set tile style using specified power
        {
            int number = (int)Math.Pow(GameBoard.BASE, power);
            SetTileStyleByNumber(tile, number);
        }

        public static void SetTileStyleByNumber(Tile tile, int number) // set tile style using specified number
        {
            switch (number)
            {
                case 2: case 4: // dark tile foreground + specific bachground
                    tile.Background = (SolidColorBrush)GetApplicationResourceDictionary()["N" + number + "BackgroundTileColorBrush"];
                    tile.Foreground = (SolidColorBrush)GetApplicationResourceDictionary()["DarkTileForegroundColorBrush"];
                    break;
                case 8: case 16: case 32: case 64: case 128: case 256: case 512: case 1024: case 2048: // light tile foreground + specific background
                    tile.Background = (SolidColorBrush)GetApplicationResourceDictionary()["N" + number + "BackgroundTileColorBrush"];
                    tile.Foreground = (SolidColorBrush)GetApplicationResourceDictionary()["LightTileForegroundColorBrush"];
                    break;
                default: // white foreground + black background
                    tile.Background = new SolidColorBrush(Colors.Black);
                    tile.Foreground = new SolidColorBrush(Colors.White);
                    break;
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e) // start new game
        {
            GameBoard.TryAgain();
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e) // navigate to main menu page
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("MainMenuPage.xaml", UriKind.RelativeOrAbsolute));
            GameBoard.SetGameStatePaused();
            LocalDataStorage.RefreshSaveFileXML();
        }

        private void ScreenshotButton_Click(object sender, RoutedEventArgs e) // take screenshot button action
        {
            SaveScreenshotToClipboard();
        }
    }
}
