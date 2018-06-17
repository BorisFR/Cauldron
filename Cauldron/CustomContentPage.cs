// /*
// Author: Boris
// Create: 17/06/2018
// */

/*using System;
using Xamarin.Forms;

namespace Xamarin.Forms
{
    public class CustomContentPage : ContentPage
    {
        public event EventHandler<KeyEventArgs> KeyPressed;

        public void SendKeyPressed(object sender, KeyEventArgs e)
        {
            KeyPressed?.Invoke(sender, e);
        }

        public CustomContentPage()
        {
            KeyPressed += CustomContentPage_KeyPressed;
        }

        private void CustomContentPage_KeyPressed(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Key pressed: " + e.Key);
        }

    }

    public class KeyEventArgs : EventArgs
    {
        public string Key { get; set; }
    }

}
*/