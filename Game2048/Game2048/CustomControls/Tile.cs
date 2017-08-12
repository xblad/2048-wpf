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

namespace Game2048.CustomControls
{
    public class Tile : Label
    {
        public static int Count = 0;
        
        static Tile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(typeof(Tile)));
        }

        public int GetNumber() // returns number on the tile
        {
            int i;
            if (Int32.TryParse(this.Content.ToString(),out i)) {
                return i;
            }

            return 0;
        }

        public static readonly DependencyProperty IsNumberChangedProperty =
DependencyProperty.RegisterAttached("IsNumberChanged", typeof(bool), typeof(Tile),
new FrameworkPropertyMetadata(null)); // custom dependency property, indicates that number on tile was multiplied

        public static void SetIsNumberChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(IsNumberChangedProperty, value);
        }

        public static bool GetIsNumberChanged(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsNumberChangedProperty);
        }

        private static void OnIsNumberChangedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newPropertyValue = (bool)e.NewValue;
            Tile instance = (Tile)d;
        }

        public State CurrentState { get; set; } // current state of tile

        public enum State // states of tile
        {
            Delete = 0,
            Multiply = 1,
            Normal = 2
        }
    }
}
