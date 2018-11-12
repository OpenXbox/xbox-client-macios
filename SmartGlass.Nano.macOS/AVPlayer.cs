using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using SmartGlass.Nano.AVFoundation;
using Foundation;

namespace SmartGlass.Nano.macOS
{
    public class AVPlayer : NSObject
    {
        public bool viewInitialized { get; private set; }
        private static readonly string _hostname = "10.0.0.241";
        private SmartGlassClient _smartGlassClient;
        private NanoClient _nanoClient;
        private AVFoundationConsumer _avConsumer;

        public AVPlayer() : base()
        {
            _avConsumer = new AVFoundationConsumer();
            viewInitialized = false;
        }

        public void SetView(NSView view)
        {
            if (_avConsumer.VideoEngineManager == null ||
                _avConsumer.VideoEngineManager.DisplayLayer == null)
            {
                Debug.WriteLine("DisplayLayer not ready yet");
                return;
            }

            var displayLayer = _avConsumer.VideoEngineManager.DisplayLayer;
            Debug.WriteLine("SetView DisplayLayer");
            displayLayer.Bounds = view.Bounds;
            displayLayer.Frame = view.Frame;
            displayLayer.BackgroundColor = NSColor.Black.CGColor;
            displayLayer.Position = new CGPoint(view.Bounds.GetMidX(),
                                                 view.Bounds.GetMidY());
            displayLayer.VideoGravity = "ResizeAspect";

            // Remove from previous view if exists
            displayLayer.RemoveFromSuperLayer();

            view.Layer.AddSublayer(displayLayer);
            viewInitialized = true;
        }

        public async Task CreateClient()
        {
            Debug.WriteLine($"Connecting to console...");

            _smartGlassClient = await SmartGlassClient.ConnectAsync(_hostname);

            var broadcastChannel = _smartGlassClient.BroadcastChannel;
            var result = await broadcastChannel.StartGamestreamAsync();

            Debug.WriteLine($"Connecting to Nano, TCP: {result.TcpPort}, UDP: {result.UdpPort}");

            _nanoClient = new NanoClient(_hostname, result.TcpPort, result.UdpPort, new Guid(), _avConsumer);
            await _nanoClient.Initialize();
            await _nanoClient.StartStream();

            Debug.WriteLine($"Nano connected and running.");
        }
    }
}
