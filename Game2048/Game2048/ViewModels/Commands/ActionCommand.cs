using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Game2048.Objects;

namespace Game2048.ViewModels.Commands
{
    class ActionCommand : ICommand
    {
        private readonly Action<MovesHandling.MoveDirection> action;

        public ActionCommand(Action<MovesHandling.MoveDirection> action)
        {
            this.action = action;
        }

        public void Execute(object parameter)
        {
            MovesHandling.MoveDirection direction = (MovesHandling.MoveDirection)parameter;
            this.action(direction);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
