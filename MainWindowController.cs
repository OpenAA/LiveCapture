namespace LiveCapture
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MonoMac.Foundation;
    using MonoMac.AppKit;
    using NLog;

    public partial class MainWindowController : MonoMac.AppKit.NSWindowController
    {

        #region Constructors

        // Called when created from unmanaged code
        public MainWindowController(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        // Call to load from the XIB/NIB file
        public MainWindowController() : base("MainWindow")
        {
            Initialize();
        }
        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        //strongly typed window accessor
        public new MainWindow Window
        {
            get
            {
                return (MainWindow)base.Window;
            }
        }

        // ----------------------------------------------------------
        // ここからメイン処理

        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        Capture _capture;

        /// <summary>
        /// AwakeFromNibはWindowsFormsやJavaScriptでいうOnLoadに相当するもの。
        /// </summary>
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            // フルスクリーンモードの入切りを検出する。
            this.Window.WillEnterFullScreen += (object sender, EventArgs e) => {
                _logger.Debug("event: WillEnterFullScreen");
            };
            this.Window.WillExitFullScreen += (object sender, EventArgs e) => {
                _logger.Debug("event: WillExitFullScreen");
            };

            // キャプチャ開始
            _capture = new Capture();
            _capture.Start();
            captureView.CaptureSession = _capture.CaptureSession;
        }

        /// <summary>
        /// Cocoaはメモリ消費量が激しすぎて全自動GCでは解放が追いつかないので手動でDisposeしまくった方がいい。
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_capture != null)
                {
                    _capture.Dispose();
                    _capture = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}

