using System;
using System.Collections.Generic;
using System.Diagnostics;
using AppKit;
using AVFoundation;
using CoreAnimation;
using CoreGraphics;
using CoreMedia;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
using Foundation;

namespace SmartGlass.Nano.AVFoundation
{
    public class VideoEngineManager : IDisposable
    {
        private CoreGraphics.CGSize _videoDimensions;
        private CMVideoFormatDescription _videoFormatDescription;
        private AVSampleBufferDisplayLayer _displayLayer;

        public AVSampleBufferDisplayLayer DisplayLayer
        {
            get => _displayLayer;
        }

        public VideoEngineManager(VideoFormat format)
        {
            if (format.Codec != VideoCodec.H264)
            {
                throw new InvalidOperationException("VideoEngineManager only supports H264");
            }

            InitDisplayLayer();
        }

        private void InitDisplayLayer()
        {
            Debug.WriteLine("Initializing DisplayLayer");
            _displayLayer = new AVSampleBufferDisplayLayer();
        }

        public int EnqueueH264Nalu(CMBlockBuffer naluData)
        {
            if (_displayLayer.Status == AVQueuedSampleBufferRenderingStatus.Failed)
            {
                InitDisplayLayer();
                return 1;
            }

            CMSampleBufferError sampleErr;
            CMSampleBuffer sampleBuf = CMSampleBuffer.CreateReady(
                dataBuffer: naluData,
                formatDescription: _videoFormatDescription,
                samplesCount: 1,
                sampleTimingArray: null,
                sampleSizeArray: null,
                error: out sampleErr);

            if (sampleErr != CMSampleBufferError.None)
            {
                Debug.WriteLine($"CMSampleBuffer.CreateReady failed, err: {sampleErr}");
                return 2;
            }

            _displayLayer.Enqueue(sampleBuf);

            return 0;
        }

        public int ConsumeVideoData(H264Frame frame)
        {

            // if we havent already set up our format description with our SPS PPS parameters, we
            // can't process any frames except type 7 that has our parameters
            if (_videoFormatDescription == null && !frame.ContainsSPS)
            {
                Debug.WriteLine($"Video error: Frame is not an SPS/PPS frame and format description is null");
                return 1;
            }

            if (frame.ContainsPPS)
            {
                // now we set our H264 parameters
                List<byte[]> parameterSetPointers = new List<byte[]>
                {
                    frame.GetSpsData(),
                    frame.GetPpsData()
                };

                CMFormatDescriptionError formatDescError;
                _videoFormatDescription = CMVideoFormatDescription.FromH264ParameterSets(
                    parameterSets: null,
                    nalUnitHeaderLength: 0,
                    error: out formatDescError);

                if (formatDescError != CMFormatDescriptionError.None)
                {
                    Debug.WriteLine($"CMVideoFormatDescription.FromH264ParameterSets failed, err: {formatDescError}");
                    return 2;
                }

                _videoDimensions = _videoFormatDescription.GetPresentationDimensions(false, false);
            }

            if (frame.ContainsPFrame || frame.ContainsIFrame)
            {
                CMBlockBufferError blockBufErr;
                CMBlockBuffer blockBuf;

                blockBuf = CMBlockBuffer.FromMemoryBlock(
                    frame.GetFrameDataAvcc(), 0, 0, out blockBufErr);

                if (blockBufErr != CMBlockBufferError.None)
                {
                    Debug.WriteLine($"BlockBufferCreation IFrame failed, err: {blockBufErr}");
                    return 3;
                }

                EnqueueH264Nalu(blockBuf);
            }
            return 0;
        }

        public void Dispose()
        {
            _displayLayer.Dispose();
        }
    }
}
