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

using Game2048.Objects;
using Game2048.Data;
using Game2048.DataObjects;
using Game2048.CustomControls;

namespace Game2048
{
    /// <summary>
    /// Interaction logic for StatisticsPage.xaml
    /// </summary>
    public partial class StatisticsPage : Page
    {
        public StatisticsPage()
        {
            InitializeComponent();
        }

        public static StatisticsPage GetStatisticsPageInstance() // get instance of statistics page
        {
            return (StatisticsPage)((Grid)Application.Current.MainWindow.Content).Children.OfType<Frame>().FirstOrDefault().Content;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e) // start preparing of statistics table
        {
            await PrepareStatisticsTable();
        }

        private async Task PrepareStatisticsTable()
        {
            Grid statisticsTable = GetStatisticsPageInstance().StatisticsTable;

            // label used for user informing about actual state
            Label messageText = new Label();
            messageText.Content = "Loading, please wait...";
            messageText.HorizontalAlignment = HorizontalAlignment.Center;
            statisticsTable.Children.Add(messageText);
            Grid.SetRow(messageText, 1);
            Grid.SetColumnSpan(messageText, 4);
            try
            {
                List<ResultDO> scoresList = await ResultDO.GetBestResultsAsync();
                if (scoresList.Count > 0)
                {
                    statisticsTable.Children.Remove(messageText);
                    Label gamerLabel;
                    Label scoreLabel;
                    Tile bgstTile;
                    statisticsTable.Children.OfType<Ellipse>().ToList().ForEach(x => { x.Visibility = Visibility.Visible; }); // show up all medals
                    scoresList.ForEach(scoreRow =>
                    {

                        int currentIndex = scoresList.FindIndex(x => x == scoreRow) + 1; //current row + header
                        if (statisticsTable.RowDefinitions.Count <= currentIndex)
                            statisticsTable.RowDefinitions.Add(new RowDefinition());

                        // prepare game name column content
                        gamerLabel = new Label();
                        gamerLabel.Content = scoreRow.GamerName + (scoreRow.GamerGuid == LocalDataStorage.GetGamerGuid() ? " (you)" : "");
                        gamerLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        gamerLabel.VerticalAlignment = VerticalAlignment.Center;
                        statisticsTable.Children.Add(gamerLabel);
                        Grid.SetRow(gamerLabel, currentIndex);
                        Grid.SetColumn(gamerLabel, 0);

                        // prepare score column content
                        scoreLabel = new Label();
                        scoreLabel.Content = scoreRow.GamerScores;
                        scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        scoreLabel.VerticalAlignment = VerticalAlignment.Center;
                        statisticsTable.Children.Add(scoreLabel);
                        Grid.SetRow(scoreLabel, currentIndex);
                        Grid.SetColumn(scoreLabel, 2);

                        // prepare biggest tile column content
                        bgstTile = new Tile();
                        bgstTile.Content = scoreRow.GamerBiggestTile;
                        GamePage.SetTileStyleByNumber(bgstTile, bgstTile.GetNumber());
                        bgstTile.HorizontalAlignment = HorizontalAlignment.Right;
                        bgstTile.VerticalAlignment = VerticalAlignment.Center;
                        statisticsTable.Children.Add(bgstTile);
                        Grid.SetRow(bgstTile, currentIndex);
                        Grid.SetColumn(bgstTile, 3);
                        bgstTile.Height = 50;
                        bgstTile.Width = 50;
                    });
                }
                else
                {
                    messageText.Content = "No relevant data to display.";
                }
                
            }
            catch
            {
                messageText.Content = "No connection to server, please try again later...";
            }
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e) // navigate to main menu page
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("MainMenuPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}
