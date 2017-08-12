using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game2048.Other
{
    class Tools
    {
        /// <summary>
        /// Function makes Random number more "random"
        /// </summary>
        /// <returns>customize random number, which supposed to be more random</returns>
        public static Random CustomRandom()
        {
            return new Random(Guid.NewGuid().GetHashCode());
        }
    }
}
