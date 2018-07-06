// /*
// Author: Boris
// Create: 06/07/2018
// */
using System;

namespace Cauldron
{
    public delegate void ChangeSize(int scale);

    public class ChangeScreenSize
    {
        public event ChangeSize ChangeSize;

        private int scale;

        public int Scale
        {
            get { return scale; }
            set
            {
                if (scale == value)
                    return;
                scale = value;
                ChangeSize?.Invoke(scale);
            }
        }

    }

}