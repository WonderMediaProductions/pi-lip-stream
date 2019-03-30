using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MMALSharp;
using MMALSharp.Common.Utility;
using MMALSharp.Components;
using MMALSharp.Config;
using MMALSharp.Handlers;
using MMALSharp.Native;
using MMALSharp.Ports;

namespace Lipstream.pi
{
    public class CameraUdpStreamer : ICaptureHandler
    {
        public Stopwatch Stopwatch = new Stopwatch();

        public int FrameCount = 0;

        public static readonly ProcessResult NoResult = new ProcessResult();

        public CameraUdpStreamer()
        {
        }

        void IDisposable.Dispose()
        {
        }

        void ICaptureHandler.PostProcess()
        {
            Stopwatch.Stop();
        }

        ProcessResult ICaptureHandler.Process(uint allocSize)
        {
            return NoResult;
        }

        void ICaptureHandler.Process(byte[] data)
        {
            if (FrameCount == 0)
            {
                Stopwatch.Start();
            }
            else
            {
                Debug.Assert(Stopwatch.IsRunning);
            }

            ++FrameCount;
            Console.WriteLine(data.Length);
        }

        string ICaptureHandler.TotalProcessed()
        {
            return $"Processed {FrameCount} frames in {Stopwatch.ElapsedMilliseconds}ms, {FrameCount / Stopwatch.Elapsed.TotalSeconds:000.0}FPS";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            MMALCameraConfig.Debug = true;

            await TakeVideoManual();
            Console.WriteLine("Done");
        }

        public static async Task TakeVideoManual()
        {
            MMALCamera cam = MMALCamera.Instance;

            const int fps = 30;

            MMALCameraConfig.SensorMode = MMALSensorMode.Mode7;
            MMALCameraConfig.VideoResolution = Resolution.As03MPixel;
            MMALCameraConfig.VideoFramerate = new MMAL_RATIONAL_T(fps, 1);
            MMALCameraConfig.ExposureMode = MMAL_PARAM_EXPOSUREMODE_T.MMAL_PARAM_EXPOSUREMODE_FIXEDFPS;

            using (var capturer = new VideoStreamCaptureHandler("/home/pi/Desktop/", "h264"))
            using (var resizer = new MMALResizerComponent(null))
            using (var encoder = new MMALVideoEncoder(capturer))
            using (var nullSink = new MMALNullSinkComponent())
            {
                cam.ConfigureCameraSettings();

                var encoderPortConfig = new MMALPortConfig(MMALEncoding.H264, MMALEncoding.I420, fps, 10, MMALVideoEncoder.MaxBitrateLevel4, null);
                encoder.ConfigureOutputPort(encoderPortConfig);

                var resizerPortConfig = new MMALPortConfig(MMALEncoding.I420, MMALEncoding.I420, 640 / 4, 480 / 4, fps, 0, 0, false, null);
                resizer.ConfigureInputPort(MMALEncoding.OPAQUE, MMALEncoding.I420, cam.Camera.VideoPort)
                       .ConfigureOutputPort(resizerPortConfig);

                resizer.Outputs[0].ConnectTo(encoder);

                cam.Camera.VideoPort.ConnectTo(resizer);

                cam.Camera.PreviewPort.ConnectTo(nullSink);

                // Camera warm up time
                await Task.Delay(2000);

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                await cam.ProcessAsync(cam.Camera.VideoPort, cts.Token);
            }

            // Only call when you no longer require the camera, i.e. on app shutdown.
            cam.Cleanup();
        }

        public static async Task StreamingTest()
        {
            // NOT WORKING YET

            MMALCamera cam = MMALCamera.Instance;

            //MMALCameraConfig.VideoColorSpace = MMALEncoding.RGB16;
            //MMALCameraConfig.VideoEncoding = MMALEncoding.RGB16;
            //MMALCameraConfig.VideoSubformat = MMALEncoding.RGB16;
            //MMALCameraConfig.VideoFramerate = new MMAL_RATIONAL_T(90, 1);
            //MMALCameraConfig.VideoResolution = Resolution.As03MPixel;
            //MMALCameraConfig.ShutterSpeed = 1000;
            //MMALCameraConfig.ExposureMode = MMAL_PARAM_EXPOSUREMODE_T.MMAL_PARAM_EXPOSUREMODE_FIXEDFPS;
            //MMALCameraConfig.SensorMode = MMALSensorMode.Mode7;

            using (var streamer = new CameraUdpStreamer())
            using (var resizer = new MMALResizerComponent(null))
            using (var encoder = new MMALVideoEncoder(streamer))
            using (var nullSink = new MMALNullSinkComponent())
            {
                cam.ConfigureCameraSettings();

                var resizerPortConfig = new MMALPortConfig(MMALEncoding.RGB16, MMALEncoding.RGB16, 640 / 4, 480 / 4, 0, 0, 0, false, null);
                var encoderPortConfig = new MMALPortConfig(MMALEncoding.RGB16, MMALEncoding.RGB16, 100);

                resizer.ConfigureInputPort(MMALEncoding.OPAQUE, MMALEncoding.RGB16, cam.Camera.VideoPort)
                       .ConfigureOutputPort(resizerPortConfig);

                encoder.ConfigureOutputPort(encoderPortConfig);

                cam.Camera.PreviewPort.ConnectTo(nullSink);
                cam.Camera.VideoPort.ConnectTo(resizer);
                resizer.Outputs[0].ConnectTo(encoder);

                // Camera warm up time
                // await Task.Delay(2000);

                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await cam.ProcessAsync(cam.Camera.VideoPort, cancellationTokenSource.Token);
            }

            // Only call when you no longer require the camera, i.e. on app shutdown.
            cam.Cleanup();
        }
    }
}
