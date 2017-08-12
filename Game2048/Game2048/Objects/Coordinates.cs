using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game2048.Objects
{
    public class Coordinates
    {
        public int Row { get; set; }
        public int Column { get; set; }

        /// <summary>
        /// Basic constructor for Coordinates class without parameters
        /// </summary>
        public Coordinates() : this(0, 0) { }

        /// <summary>
        /// Constructor for Coordinates class
        /// </summary>
        /// <param name="row">Row number</param>
        /// <param name="column">Column number</param>
        public Coordinates(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }

        /// <summary>
        /// Get vector sum of two coordinates
        /// </summary>
        /// <param name="a">First coordinates</param>
        /// <param name="b">Second coordinates</param>
        /// <returns></returns>
        public static Coordinates Sum(Coordinates a, Coordinates b)
        {
            return new Coordinates(a.Row + b.Row, a.Column + b.Column);
        }

        /// <summary>
        /// Shows coordinates in human readable form
        /// </summary>
        /// <returns>returns coordinates in format "[row, column]"</returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}]",this.Row, this.Column);
        }
    }
}
