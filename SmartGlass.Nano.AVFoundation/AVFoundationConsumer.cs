using System;
using System.Diagnostics;
using AppKit;
using AVFoundation;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

namespace SmartGlass.Nano.AVFoundation
{
    public class AVFoundationConsumer : IConsumer, IDisposable
    {
        VideoAssembler _videoAssembler;
        AudioEngineManager _audioEngineManager;
        VideoEngineManager _videoEngineManager;

        public VideoEngineManager VideoEngineManager
        {
            get => _videoEngineManager;
        }

        public AVFoundationConsumer()
        {
            _videoAssembler = new VideoAssembler();
        }

        public void ConsumeAudioData(AudioData data)
        {
            _audioEngineManager.ConsumeAudioData(data);
        }

        public void ConsumeAudioFormat(AudioFormat format)
        {
            try
            {
                _audioEngineManager = new AudioEngineManager(format);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void ConsumeVideoData(VideoData data)
        {
            H264Frame frame = _videoAssembler.AssembleVideoFrame(data);
            if (frame != null)
            {
                _videoEngineManager.ConsumeVideoData(frame);
            }
        }

        public void ConsumeVideoFormat(VideoFormat format)
        {
            try
            {
                _videoEngineManager = new VideoEngineManager(format);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void Dispose()
        {
            if (_audioEngineManager != null)
            {
                _audioEngineManager.Dispose();
            }
            if (_videoEngineManager != null)
            {
                _videoEngineManager.Dispose();
            }
        }
    }
}
