using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MMALSharp;
using MMALSharp.Common.Utility;
using MMALSharp.Components;
using MMALSharp.Handlers;
using MMALSharp.Native;
using MMALSharp.Ports;
using NLog.Config;
using NLog.Targets;

namespace Lipstream.pi
{
    public class CustomVideoCapturer : VideoStreamCaptureHandler
    {
        public CustomVideoCapturer(string directory, string extension) : base(directory, extension)
        {
        }

        public override void NewFile()
        {
            CurrentStream = File.Create(Path.Combine(Directory, $"output.{Extension}"));
        }
    }

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
            Console.WriteLine("PostProcess");
            Stopwatch.Stop();
        }

        ProcessResult ICaptureHandler.Process(uint allocSize)
        {
            Console.WriteLine($"Process {allocSize}");
            return NoResult;
        }

        void ICaptureHandler.Process(byte[] data)
        {
            Console.WriteLine($"Process data {data.Length}");

            if (FrameCount == 0)
            {
                Stopwatch.Start();
            }
            else
            {
                // Debug.Assert(Stopwatch.IsRunning);
            }

            ++FrameCount;
        }

        string ICaptureHandler.TotalProcessed()
        {
            return $"TotalProcessed {FrameCount} frames in {Stopwatch.ElapsedMilliseconds}ms, {FrameCount / Stopwatch.Elapsed.TotalSeconds:000.0}FPS";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            MMALCameraConfig.Debug = true;

            var config = new LoggingConfiguration();
            var target = new ColoredConsoleTarget();
            config.AddTarget("console", target);

            await TakeVideoManual();
        }

        public static async Task TakeVideoManual()
        {
            MMALCamera cam = MMALCamera.Instance;

            const int fps = 60;

            cam.EnableCamera();

            // https://github.com/techyian/MMALSharp/wiki/Sony-IMX219-Camera-Module
            MMALCameraConfig.SensorMode = MMALSensorMode.Mode7;
            MMALCameraConfig.VideoResolution = Resolution.As03MPixel;
            MMALCameraConfig.VideoFramerate = new MMAL_RATIONAL_T(fps, 1);

            using (var streamer = new CameraUdpStreamer())
            using (var resizer = new MMALResizerComponent(streamer))
            using (var nullSink = new MMALNullSinkComponent())
            {
                cam.ConfigureCameraSettings();

                var resizerPortConfig = new MMALPortConfig(MMALEncoding.I420, MMALEncoding.I420, 640 / 4, 480 / 4, fps, 0, 0, false, null);
                resizer.ConfigureInputPort(MMALEncoding.OPAQUE, MMALEncoding.I420, cam.Camera.VideoPort);
                resizer.ConfigureOutputPort(resizerPortConfig);

                resizer.EnableConnections();

                cam.Camera.VideoPort.ConnectTo(resizer);
                cam.Camera.PreviewPort.ConnectTo(nullSink);

                cam.PrintPipeline();

                // Camera warm up time
                Console.WriteLine("Warming up...");
                await Task.Delay(2000);

                Console.WriteLine("Capturing...");
                
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                //await cam.ProcessAsync(cam.Camera.VideoPort, cts.Token);
                await cam.ProcessAsync(cam.Camera.VideoPort, cts.Token);

                Console.WriteLine("Completed!");
            }

            // Only call when you no longer require the camera, i.e. on app shutdown.
            cam.Cleanup();
        }
    }
}
