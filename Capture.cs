namespace LiveCapture
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog;
    using MonoMac.Foundation;
    using MonoMac.AppKit;
    using MonoMac.QTKit;
    using MonoMac.CoreVideo;
    using MonoMac.CoreImage;

    /// <summary>
    /// キャプチャクラス
    /// </summary>
    public class Capture : IDisposable
    {
        static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        bool _isDisposed = false;
        object _sync = new object();
        QTCaptureSession _captureSession;
        QTCaptureDeviceInput _captureInput;
        QTCaptureDecompressedVideoOutput _captureOutput;
        volatile CVImageBuffer _currentImage;

        public Capture()
        {
            // セッション開始
            _captureSession = new QTCaptureSession();
        }

        ~Capture()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed == false)
            {
                if (disposing)
                {
                    Stop();

                    if (_captureSession != null)
                    {
                        _captureSession.Dispose();
                        _captureSession = null;
                    }
                }
                _isDisposed = true;
            }
        }

        public QTCaptureSession CaptureSession
        {
            get { return _captureSession; }
        }
         
        public bool Start()
        {
            NSError err = null;

            try
            {
                // キャプチャデバイスの一覧を取得する
                var devices = QTCaptureDevice.GetInputDevices(QTMediaType.Video);

                // いちばん最初に登場いたキャプチャデバイスを取り出す
                var device = devices.FirstOrDefault();
                if (device == null)
                {
                    _logger.Info("capture device not found");
                    return false;
                }

                // キャプチャデバイス開く
                if (!device.Open(out err))
                {
                    _logger.Info("capture device not opening");
                    return false;
                }

                // インプットデバイスを設定する
                _captureInput = new QTCaptureDeviceInput(device);
                if (!_captureSession.AddInput(_captureInput, out err))
                {
                    _logger.Info("do not add input device");
                    return false;
                }

                // アウトプットデバイスを設定する
                // 保存用に最新フレームを取得し続ける
                _captureOutput = new QTCaptureDecompressedVideoOutput();
                _captureOutput.DidOutputVideoFrame += (object sender, QTCaptureVideoFrameEventArgs e) => {
                    _captureOutput.Retain();
                    lock (this)
                    {
                        if (_currentImage != null)
                        {
                            _currentImage.Dispose();
                            _currentImage = null;
                        }
                        _currentImage = e.VideoFrame;
                    }
                    _captureOutput.Release();
                };
                if (!_captureSession.AddOutput(_captureOutput, out err))
                {
                    _logger.Info("do not add output device");
                    return false;
                }

                // キャプチャ開始
                _captureSession.StartRunning();
                _logger.Info("start capture");

                return true;
            }
            finally
            {
                if (err != null)
                {
                    err.Dispose();
                    err = null;
                }
            }
        }

        public void Stop()
        {
            if (_captureInput != null)
            {
                _captureInput.Dispose();
                _captureInput = null;
            }
            if (_captureOutput != null)
            {
                _captureOutput.Dispose();
                _captureOutput = null;
            }
            if (_currentImage != null)
            {
                _currentImage.Dispose();
                _currentImage = null;
            }
        }

        public NSImage Still()
        {
            CIImage imageCore = null;
            NSCIImageRep imageRep = null;
            NSImage imageCocoa = null;

            lock (_sync)
            {
                if (_currentImage == null)
                {
                    return null;
                }

                try
                {
                    imageCore = CIImage.FromImageBuffer(_currentImage);
                    imageRep = NSCIImageRep.FromCIImage(CIImage.FromImageBuffer(_currentImage));

                    imageCocoa = new NSImage(imageRep.Size);
                    imageCocoa.AddRepresentation(imageRep);
                }
                finally
                {
                    if (imageRep != null)
                    {
                        imageRep.Dispose();
                        imageRep = null;
                    }
                    if (imageCore != null)
                    {
                        imageCore.Dispose();
                        imageCore = null;
                    }
                }
            }

            return imageCocoa;
        }

        public void Save(string file)
        {
            NSError error = null;
            NSImage image = null;
            NSData data = null;

            try
            {
                image = Still();
                data = image.AsTiff();
                data.Save(file: file, auxiliaryFile: false, error:out error);
            }
            finally
            {
                if (data != null)
                {
                    data.Dispose();
                    data = null;
                }
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                if (error != null)
                {
                    error.Dispose();
                    error = null;
                }
            }
        }

    }
}

