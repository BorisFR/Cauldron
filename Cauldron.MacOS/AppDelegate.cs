using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Cauldron.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow _window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(100, NSScreen.MainScreen.VisibleFrame.Size.Height - 100, 320, 200); // 1600 - 40 * 8 - 34 * 8, 998 - 23 * 8 - 23 * 8 - 3);
            _window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            _window.Title = "Cauldron";
            //_window.TitleVisibility = NSWindowTitleVisibility.Hidden;
        }

        public override NSWindow MainWindow
        {
            get { return _window; }
        }

        private static NSEvent KeyboardDownEventHandler(NSEvent keyEvent)
        {
            All.Keydown(keyEvent.KeyCode);
            return null;
            //return (keyEvent);
        }

        private static NSEvent KeyboardUpEventHandler(NSEvent keyEvent)
        {
            All.Keyup(keyEvent.KeyCode);
            return (keyEvent);
        }

        private static NSEvent KeyboardFlagsEventHandler(NSEvent keyEvent)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("Flags: {0}", keyEvent.ModifierFlags));
            if (keyEvent.ModifierFlags.HasFlag(NSEventModifierMask.ShiftKeyMask))
                All.Keydown(keyEvent.KeyCode);
            else
                All.Keyup(keyEvent.KeyCode);
            return (keyEvent);
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Insert code here to initialize your application
            Forms.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);
            NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyDown, KeyboardDownEventHandler);
            NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyUp, KeyboardUpEventHandler);
            NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.FlagsChanged, KeyboardFlagsEventHandler);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
