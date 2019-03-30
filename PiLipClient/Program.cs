using System;
using System.Threading.Tasks;
using MMALSharp;
using MMALSharp.Components;
using MMALSharp.Handlers;
using MMALSharp.Native;
using MMALSharp.Ports;

namespace Lipstream.pi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await TakePictureManual();
            Console.WriteLine("Done");
        }

        public static async Task TakePictureManual()
        {
            MMALCamera cam = MMALCamera.Instance;

            using (var imgCaptureHandler = new ImageStreamCaptureHandler("/home/pi/Desktop/", "jpg"))
            using (var imgEncoder = new MMALImageEncoder(imgCaptureHandler))
            using (var nullSink = new MMALNullSinkComponent())
            {
                cam.ConfigureCameraSettings();

                var portConfig = new MMALPortConfig(MMALEncoding.JPEG, MMALEncoding.I420, 90);

                // Create our component pipeline.         
                imgEncoder.ConfigureOutputPort(portConfig);

                cam.Camera.StillPort.ConnectTo(imgEncoder);
                cam.Camera.PreviewPort.ConnectTo(nullSink);

                // Camera warm up time
                await Task.Delay(2000);
                await cam.ProcessAsync(cam.Camera.StillPort);
            }

            // Only call when you no longer require the camera, i.e. on app shutdown.
            cam.Cleanup();
        }
    }
}
