using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;

using Game2048.ViewModels.Commands;
using Game2048.Objects;
using Game2048.Other;

namespace Game2048.ViewModels
{
    class GamePageViewModel
    {
        private ICommand moveTilesCommand;
        public ICommand MoveTilesCommand
        {
            get
            {
                return moveTilesCommand
                    ?? (moveTilesCommand = new ActionCommand(GamePage.Move)); // perform move
            }
        }

        public static void InitMoveCommandsInWindow()
        {
            GamePageViewModel vm = new GamePageViewModel();
            KeyBinding b = new KeyBinding() // Left key
            {
                Command = vm.MoveTilesCommand,
                Key = Key.Left,
                CommandParameter = MovesHandling.MoveDirection.Left
            };
            Application.Current.MainWindow.InputBindings.Add(b);
            b = new KeyBinding() // Up key
            {
                Command = vm.moveTilesCommand,
                Key = Key.Up,
                CommandParameter = MovesHandling.MoveDirection.Top
            };
            Application.Current.MainWindow.InputBindings.Add(b);
            b = new KeyBinding() // Right key
            {
                Command = vm.moveTilesCommand,
                Key = Key.Right,
                CommandParameter = MovesHandling.MoveDirection.Right
            };
            Application.Current.MainWindow.InputBindings.Add(b);
            b = new KeyBinding() // Down key
            {
                Command = vm.moveTilesCommand,
                Key = Key.Down,
                CommandParameter = MovesHandling.MoveDirection.Bottom
            };
            Application.Current.MainWindow.InputBindings.Add(b);
        }

        public static void RemoveAllMoveCommandsFromWindow()
        {
            Application.Current.MainWindow.InputBindings
                .OfType<KeyBinding>()
                .Where(kb => kb.Key == Key.Left || kb.Key == Key.Up || kb.Key == Key.Right || kb.Key == Key.Down)
                .ToList().ForEach(kb => { 
                    Application.Current.MainWindow.InputBindings.Remove(kb); // remove all arrow key commands
                });
        }
    }
}
