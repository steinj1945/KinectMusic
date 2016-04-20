//#define REMOTE_DEBUG
using log4net;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KinectSample
{
    public class Program
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                
        static void Main()
        {

#if REMOTE_DEBUG
            Log.Warn("This assembly was built with REMOTE_DEBUG defined. Execution will pause until a debugger is attached.");

            while(!Debugger.IsAttached)
            {
                System.Threading.Thread.Sleep(1000);
            }
            Log.Debug("Debugger attached, Calling Debugger.Break()");            
            Debugger.Break();
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Some of my computers are blue command prompt, lets fix that for this program
            Console.BackgroundColor = ConsoleColor.Black;

            Log.InfoFormat("Running KinectSample v{0}", "<Add the version here>");

            if (KinectSensor.KinectSensors.Count() > 0)
            {
                sensor = KinectSensor.KinectSensors[0];

                Log.DebugFormat("Using Kinect Sensor with UniqueKinectId of {0}", sensor.UniqueKinectId);                                
            }
            else
            {
                Log.Fatal("No Kinect Sensors found! Press any key to exit...");
                Console.ReadKey();
                return;
            }

            sensor.AllFramesReady += Sensor_AllFramesReady;

            Log.Debug("Enabling SkeletonStream");
            sensor.SkeletonStream.Enable();
            Log.Debug("Enabling ColorStream");
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            Log.Debug("Enabling DepthStream");
            sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

            kinectMonitor = new KinectMonitor(sensor);

            kinectMonitor.Show();

            Log.Debug("Starting the KinectSensor");
            sensor.Start();
            Log.Debug("Started the KinectSensor");
            
            bool runProgram = true;

            while (runProgram)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key == ConsoleKey.Escape)
                {
                    runProgram = false;
                    Log.Info("Exiting...");
                }

            }

            Log.Debug("Stopping the KinectSensor");
            sensor.Stop();
            Log.Debug("Stopped the KinectSensor");

            Log.Debug("Closing the KinectMonitor");
            kinectMonitor.Close();
            Log.Debug("Closed the KinectMonitor");

#if DEBUG
            //I would like to hang on to the console so I can see what the end is, just in case.
            Log.Info("End of Program, Press any key to exit...");
            Console.ReadKey();
#endif                                     
        }

        private static void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //throw new NotImplementedException();            
            if(kinectMonitor.WindowState != FormWindowState.Minimized)
            {
                using (var colorFrame = e.OpenColorImageFrame())
                using (var depthFrame = e.OpenDepthImageFrame())
                using (var skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (colorFrame == null)
                        return;

                    var colorBitmap = Helpers.ImageToBitmap(colorFrame);

                    //draw to form
                    if (colorBitmap != null)
                        kinectMonitor.DrawBitmapToForm(colorBitmap, new Point(0, 0));

                    if (skeletonFrame != null)
                    {
                        var skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];

                        skeletonFrame.CopySkeletonDataTo(skeletons);

                        foreach (var skeleton in skeletons)
                        {
                            if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                kinectMonitor.DrawTrackedSkeletonJoints(skeleton.Joints);
                            }
                        }
                    }                   
                }
            }            
        }

        public static KinectSensor sensor;
        public static KinectMonitor kinectMonitor;
    }

    class Helpers
    {
        public static Bitmap ImageToBitmap(ColorImageFrame Image)
        {
            byte[] pixelData = new byte[Image.PixelDataLength];
            Image.CopyPixelDataTo(pixelData);
            Bitmap bmap = new Bitmap(Image.Width, Image.Height, PixelFormat.Format32bppRgb);
            BitmapData bitmapData = bmap.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly, bmap.PixelFormat);
            IntPtr ptr = bitmapData.Scan0;
            Marshal.Copy(pixelData, 0, ptr, Image.PixelDataLength);
            bmap.UnlockBits(bitmapData);
            return bmap;
        }

        public static Bitmap CreateBitMapFromDepthFrame(DepthImageFrame frame)
        {
            if (frame != null)
            {
                var bitmapImage = new Bitmap(frame.Width, frame.Height, PixelFormat.Format16bppRgb565);
                var g = Graphics.FromImage(bitmapImage);
                g.Clear(Color.FromArgb(0, 34, 68));

                var pixelData = new short[frame.PixelDataLength];
                frame.CopyPixelDataTo(pixelData);
                BitmapData bitmapData = bitmapImage.LockBits(new Rectangle(0, 0, frame.Width,
                 frame.Height), ImageLockMode.WriteOnly, bitmapImage.PixelFormat);
                IntPtr ptr = bitmapData.Scan0;
                Marshal.Copy(pixelData, 0, ptr, frame.Width * frame.Height);
                bitmapImage.UnlockBits(bitmapData);

                return bitmapImage;
            }
            return null;
        }
    }
}
