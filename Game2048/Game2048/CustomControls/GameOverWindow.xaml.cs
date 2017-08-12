using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Threading;
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
using Game2048.Other;

namespace Game2048.CustomControls
{
    /// <summary>
    /// Interaction logic for GameOverWindow.xaml
    /// </summary>
    public partial class GameOverWindow : UserControl
    {
        public GameOverWindow()
        {
            InitializeComponent();
        }

        private async void GameOverUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (await ResultDO.TestConnectionAsync()) // test of connection
                await PrepareGamerNameTextBox();
            else
            {
                GamerNameTextBox.IsEnabled = true;
                this.Resources["GamerNamePlaceholderText"] = "Your name...";
                ErrorMessageLabel.Visibility = Visibility.Visible;
            }
        }

        private async Task PrepareGamerNameTextBox() // handling of gamer name textbox
        {
            Guid gamerGuid = LocalDataStorage.GetGamerGuid();
            if (await ResultDO.IsGuidInDatabaseAsync(gamerGuid))
            {
                GamerNameTextBox.Text = await ResultDO.GetGamerNameByGuidAsync(gamerGuid); // get gamer name from database using Guid
                GamerNameTextBox.IsEnabled = false;
            }
            else // allow gamer to add new name in text box
            {
                GamerNameTextBox.IsEnabled = true;
                this.Resources["GamerNamePlaceholderText"] = "Your name...";
            }
        }

        private void SendResult() // send all relevant statistics to database
        {
            LocalDataStorage.RefreshSaveFileXML(); // insure we have last progress in local storage
            Guid gamerGuid = LocalDataStorage.GetGamerGuid();
            if (gamerGuid == Guid.Empty) // completely new gamer
            {
                gamerGuid = Guid.NewGuid();
                ResultDO.SendGamerToDatabase(gamerGuid, GamerNameTextBox.Text); // send new gamer to database
            }
            else if (!ResultDO.IsGuidInDatabase(gamerGuid)) // wrong Guid of gamer, which isn't in database
            {
                gamerGuid = Guid.NewGuid();
                ResultDO.SendGamerToDatabase(gamerGuid, GamerNameTextBox.Text); // send new gamer to database
            }

            ResultDO.SendResultToDatabaseAsync(
                    gamerGuid,
                    LocalDataStorage.GetLastCurrentScore(),
                    GameBoard.GetBiggestTile()
                );
            this.Visibility = Visibility.Collapsed;
            LocalDataStorage.SetGamerGuid(gamerGuid); // insure, that program will recognize gamer next time

            LocalDataStorage.ClearSaveFileBeforeNewGame();
        }

        private void TryAgainButton_Click(object sender, RoutedEventArgs e)
        {
            GameBoard.TryAgain();
        }

        private async void SendResultButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageLabel.Visibility = Visibility.Collapsed;
            LoadingLabel.Visibility = Visibility.Visible;
            SendResultButton.IsEnabled = false;
            if (await ResultDO.TestConnectionAsync())
                SendResult();
            else
            {
                LoadingLabel.Visibility = Visibility.Collapsed;
                ErrorMessageLabel.Visibility = Visibility.Visible;
                SendResultButton.IsEnabled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
