using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AudioToolbox;
using AVFoundation;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

namespace SmartGlass.Nano.AVFoundation
{
    public class PcmAudioBufferDataConsumer : IDisposable
    {
        static readonly int MAX_BUFFER_SIZE = 0x10000;

        private bool _bufferReady;

        private readonly IntPtr _audioBuffer;
        private readonly AudioStreamBasicDescription _basicDescription;
        private readonly OutputAudioQueue _audioQueue;

        public PcmAudioBufferDataConsumer()
        {
            _bufferReady = false;

            _basicDescription = AudioStreamBasicDescription.CreateLinearPCM();
            _audioQueue = new OutputAudioQueue(_basicDescription);
            _audioQueue.Volume = 1.0f;
            _audioQueue.BufferCompleted += _audioQueue_BufferCompleted;

            AudioQueueStatus status = _audioQueue.AllocateBuffer(
                MAX_BUFFER_SIZE,
                out _audioBuffer);

            if (status != AudioQueueStatus.Ok)
            {
                throw new InvalidOperationException(
                    "Failed to alloc AudioBuffer");
            }
        }

        void _audioQueue_BufferCompleted(object sender, BufferCompletedEventArgs e)
        {
            _bufferReady = true;
            Debug.WriteLine("Audio buffer is ready");
        }

        public void ConsumeAudioData(AudioBuffers data, AudioStreamPacketDescription[] descs)
        {
            if (!_bufferReady)
            {
                Debug.WriteLine("ConsumeAudioData called when buffer wasnt ready");
                return;
            }

            _audioQueue.EnqueueBuffer(data.Handle, descs);

            if (!_audioQueue.IsRunning)
            {
                Play();
            }
        }

        public void Play()
        {
            _audioQueue.Start();
        }

        public void Stop()
        {
            _audioQueue.Stop(immediate: false);
        }

        public void Dispose()
        {
        }
    }
}
