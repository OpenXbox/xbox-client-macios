using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AudioToolbox;
using AVFoundation;
using CoreFoundation;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
using Foundation;

namespace SmartGlass.Nano.AVFoundation
{
    public class CompressedAudioBufferDataConsumer
    {
        private readonly static int BUFFER_SIZE = 0x1000;

        private readonly IntPtr _sampleBuffer;
        private readonly Queue<AudioData> _sampleQueue;
        private readonly AudioConverter _converter;
        private readonly PcmAudioBufferDataConsumer _pcmConsumer;

        public CompressedAudioBufferDataConsumer(AudioStreamBasicDescription format)
        {
            _sampleQueue = new Queue<AudioData>();
            _pcmConsumer = new PcmAudioBufferDataConsumer();
            _converter = AudioConverter.Create(
                format,
                AudioStreamBasicDescription.CreateLinearPCM());

            if (_converter == null)
            {
                throw new Exception("Failed to init AudioConverter");
            }

            _converter.InputData += NeedData;
            _sampleBuffer = Marshal.AllocHGlobal(BUFFER_SIZE);
        }

        AudioConverterError NeedData(ref int numberDataPackets, AudioBuffers data, ref AudioStreamPacketDescription[] dataPacketDescription)
        {
            numberDataPackets = 0;
            AudioData sample;
            if(!_sampleQueue.TryDequeue(out sample))
            {
                Marshal.Copy(sample.Data, 0, _sampleBuffer, sample.Data.Length);
                data.SetData(0, _sampleBuffer);
                numberDataPackets = 1;
            }
            else
            {
                numberDataPackets = 0;
            }

            return AudioConverterError.None;
        }

        public void ConsumeAudioData(AudioData data)
        {
            // Enqueue fresh data
            _sampleQueue.Enqueue(data);

            // Try to get decoded data back
            AudioConverterError err;
            AudioBuffers buffers = new AudioBuffers(10);
            AudioStreamPacketDescription[] descs = new AudioStreamPacketDescription[10];
            int packetSize = 0;

            err = _converter.FillComplexBuffer(ref packetSize,
                                               buffers,
                                               descs);

            if (err != AudioConverterError.None)
            {
                Debug.WriteLine("FillComplexBuffer failed: {0}", err);
                return;
            }

            Debug.WriteLine("Got back {0} bytes of converted audio", packetSize);
            _pcmConsumer.ConsumeAudioData(buffers, descs);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_sampleBuffer);
        }
    }
}
