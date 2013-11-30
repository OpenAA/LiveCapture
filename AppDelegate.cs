using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace LiveCapture
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        MainWindowController mainWindowController;

        public AppDelegate()
        {
        }

        public override void FinishedLaunching(NSObject notification)
        {
            mainWindowController = new MainWindowController();
            mainWindowController.Window.MakeKeyAndOrderFront(this);
        }

        /// <summary>
        /// ウインドウを閉じたらアプリケーションを終了するか？
        /// これ重要
        /// </summary>
        /// <returns><c>true</c>, if should terminate after last window closed was applicationed, <c>false</c> otherwise.</returns>
        /// <param name="sender">Sender.</param>
        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
            return true;
        }
    }
}

