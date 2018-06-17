/*using System;
using AppKit;
using Cauldron.MacOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(MyContentPage))]
namespace Cauldron.MacOS
{
    public class MyContentPage : PageRenderer
    {
        public MyContentPage()
        {
            System.Diagnostics.Debug.WriteLine("Je suis là");
        }

        public override void KeyDown(NSEvent theEvent)
        {
            Tools.Keydown(theEvent.KeyCode);
            base.KeyDown(theEvent);
        }
    }
}
*/