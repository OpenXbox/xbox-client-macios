using System;
using AVFoundation;
using SmartGlass.Nano.Consumer;

namespace SmartGlass.Nano.AVFoundation
{
    public interface IAVFoundationAudioDataConsumer : IAudioDataConsumer, IDisposable
    {
        AVAudioPcmBuffer Buffer { get; }
    }
}
