using System;
using SmartGlass.Nano.Packets;
using AV = AVFoundation;
using AT = AudioToolbox;

namespace SmartGlass.Nano.AVFoundation
{
    public static class PacketExtensions
    {                                 
        public static AV.AVAudioFormat ToAVAudioFormat(this AudioFormat format)
        {
            var settings = format.ToATAudioStreamDescription();
            return new AV.AVAudioFormat(ref settings);
        }

        public static AT.AudioStreamBasicDescription ToATAudioStreamDescription(this AudioFormat format)
        {
            return format.Codec == AudioCodec.PCM
                ? new AT.AudioStreamBasicDescription()
                {
                    Format = format.Codec.ToATFormatType(),
                    ChannelsPerFrame = (int)format.Channels,
                    SampleRate = (double)format.SampleRate,
                    FormatFlags = AT.AudioFormatFlags.LinearPCMIsFloat,
                    FramesPerPacket = 1,
                    BitsPerChannel = (int)format.SampleSize / 2 / 8,
                    BytesPerFrame = (int)format.SampleSize,
                    BytesPerPacket = (int)format.SampleSize,
                    Reserved = 0 // always 0
                }
                : new AT.AudioStreamBasicDescription()
                {
                    Format = format.Codec.ToATFormatType(),
                    ChannelsPerFrame = (int)format.Channels,
                    SampleRate = (double)format.SampleRate,
                    FormatFlags = AT.AudioFormatFlags.FlagsAreAllClear,
                    FramesPerPacket = 1024,
                    BitsPerChannel = 0, // compressed: 0
                    BytesPerFrame = 0, // compressed: 0
                    BytesPerPacket = 0, // compressed: 0
                    Reserved = 0 // always 0
                };
        }

        public static AT.AudioFormatType ToATFormatType(this AudioCodec codec)
        {
            switch (codec)
            {
                case AudioCodec.AAC:
                    return AT.AudioFormatType.MPEG4AAC;
                case AudioCodec.PCM:
                    return AT.AudioFormatType.LinearPCM;
                case AudioCodec.Opus:
                    return AT.AudioFormatType.Opus;
                default:
                    throw new SmartGlassException("Unknown audio codec type.");
            }
        }
    }
}
