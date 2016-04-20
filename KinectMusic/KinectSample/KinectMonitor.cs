using log4net;
using Microsoft.Kinect;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace KinectSample
{
    public partial class KinectMonitor : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public KinectMonitor(KinectSensor _sensor)
        {
            Log.Debug("Creating a new KinectMonitor");

            InitializeComponent();

            sensor = _sensor;
            mapper = new CoordinateMapper(sensor);
            graphics = this.CreateGraphics();

            this.Size = new Size(640 + 320, 480 + 100);
            this.SetStyle(ControlStyles.UserPaint, true);

            var closeButton = new Button()
            {
                Text = "Close",
                Location = new Point(0, 480)                
            };
            closeButton.Click += (sender, args) =>
            {
                this.Close();
            };
            //this.Controls.Add(closeButton);            

            this.FormClosing += (sender, args) =>
            {
                Log.Debug("KinectMonitor Closing");
                this.Showing = false;
            };

            Showing = true;

            Log.Debug("Created a new KinectMonitor");
        }

        public bool Showing = false;

        private KinectSensor sensor;
        Graphics graphics;
        CoordinateMapper mapper;

        public void DrawBitmapToForm(Bitmap bitmap, Point p)
        {
            graphics.DrawImage(bitmap, p);
        }

        public void DrawTrackedSkeletonJoints(JointCollection jointCollection)
        {
            // Render Head and Shoulders
            DrawBone(jointCollection[JointType.Head], jointCollection[JointType.ShoulderCenter]);
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderLeft]);
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderRight]);

            // Render Left Arm
            DrawBone(jointCollection[JointType.ShoulderLeft], jointCollection[JointType.ElbowLeft]);
            DrawBone(jointCollection[JointType.ElbowLeft], jointCollection[JointType.WristLeft]);
            DrawBone(jointCollection[JointType.WristLeft], jointCollection[JointType.HandLeft]);

            // Render Right Arm
            DrawBone(jointCollection[JointType.ShoulderRight], jointCollection[JointType.ElbowRight]);
            DrawBone(jointCollection[JointType.ElbowRight], jointCollection[JointType.WristRight]);
            DrawBone(jointCollection[JointType.WristRight], jointCollection[JointType.HandRight]);

            //Render Spine
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.Spine]);
            DrawBone(jointCollection[JointType.Spine], jointCollection[JointType.HipCenter]);

            //Render Left Leg
            DrawBone(jointCollection[JointType.HipCenter], jointCollection[JointType.HipLeft]);
            DrawBone(jointCollection[JointType.HipLeft], jointCollection[JointType.KneeLeft]);
            DrawBone(jointCollection[JointType.KneeLeft], jointCollection[JointType.AnkleLeft]);
            DrawBone(jointCollection[JointType.AnkleLeft], jointCollection[JointType.FootLeft]);

            //Render Right Leg
            DrawBone(jointCollection[JointType.HipCenter], jointCollection[JointType.HipRight]);
            DrawBone(jointCollection[JointType.HipRight], jointCollection[JointType.KneeRight]);
            DrawBone(jointCollection[JointType.KneeRight], jointCollection[JointType.AnkleRight]);
            DrawBone(jointCollection[JointType.AnkleRight], jointCollection[JointType.FootRight]);
        }

        public void DrawBone(Joint jointFrom, Joint jointTo)
        {
            if (jointFrom.TrackingState == JointTrackingState.NotTracked ||
            jointTo.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            if (jointFrom.TrackingState == JointTrackingState.Inferred ||
            jointTo.TrackingState == JointTrackingState.Inferred)
            {
                DrawBoneLine(jointFrom.Position, jointTo.Position, JointTrackingState.Inferred);
            }

            if (jointFrom.TrackingState == JointTrackingState.Tracked &&
            jointTo.TrackingState == JointTrackingState.Tracked)
            {
                DrawBoneLine(jointFrom.Position, jointTo.Position, JointTrackingState.Tracked);
            }
        }

        void DrawBoneLine(SkeletonPoint skeletonFrom, SkeletonPoint skeletonTo, JointTrackingState trackingState)
        {
            var colorImagePointFrom = mapper.MapSkeletonPointToColorPoint(skeletonFrom, ColorImageFormat.RgbResolution640x480Fps30);
            var colorImagePointTo = mapper.MapSkeletonPointToColorPoint(skeletonTo, ColorImageFormat.RgbResolution640x480Fps30);

            var color = Color.Red;

            switch (trackingState)
            {
                case JointTrackingState.Tracked:
                    color = Color.Red;
                    break;
                case JointTrackingState.Inferred:
                    color = Color.Blue;
                    break;
            }

            graphics.DrawLine(new Pen(color) { Width = 4 }, colorImagePointFrom.X, colorImagePointFrom.Y, colorImagePointTo.X, colorImagePointTo.Y);
        }
    }
}
