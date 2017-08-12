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

namespace Game2048
{
    /// <summary>
    /// Interaction logic for MainMenuPage.xaml
    /// </summary>
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // show or hide continue button
            if (LocalDataStorage.GetGameState() == GameBoard.GameState.InProgress || LocalDataStorage.GetGameState() == GameBoard.GameState.Paused)
                ContinueButton.Visibility = Visibility.Visible;
            else
                ContinueButton.Visibility = Visibility.Collapsed;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e) // start new game
        {
            LocalDataStorage.ClearSaveFileBeforeNewGame();
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("GamePage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e) // continue started game
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("GamePage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e) // show statistics page
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("StatisticsPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e) // show page with info about application
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("AboutPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}
