using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectHandTracker
{
    public partial class MainWindow : Window
    {
        KinectSensor sensor;
        MultiSourceFrameReader reader;
        IList<Body> bodies;
        private Nullable<ulong> bodyIndex = null;

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists("Output"))
                Directory.CreateDirectory("Output");
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            sensor = KinectSensor.GetDefault();
            if (sensor != null)
            {
                sensor.Open();
                reader =
                    sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth |
                                                      FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                reader.MultiSourceFrameArrived += OnMultiSourceFrameArrived;
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            if (reader != null)
            {
                reader.Dispose();
            }

            if (sensor != null)
            {
                sensor.Close();
            }
        }

        private void OnMultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();
            ushort[] rawDepthData = new ushort[512*424];

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.CopyFrameDataToArray(rawDepthData);
                    Parameters.minDepth = frame.DepthMinReliableDistance;
                    Parameters.maxDepth = frame.DepthMaxReliableDistance;
                    SourceView.Source = frame.ToBitmap();
                }
            }

            // Body
            using (var bodyFrame = reference.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    SkeletonView.Children.Clear();
                    bodies = new Body[bodyFrame.BodyFrameSource.BodyCount];

                    bodyFrame.GetAndRefreshBodyData(bodies);
                    bool proc = false;                    
                    foreach (var body in bodies)
                    {
                        if (body.IsTracked == false)
                        {
                            continue;
                        }
                        else
                        {
                            if (bodyIndex == null)
                            {
                                bodyIndex = body.TrackingId;
                            }
                            else if (bodyIndex != body.TrackingId)
                            {
                                continue;
                            }
                        }

                        proc = true;
                        // COORDINATE MAPPING
                        //ushort hBase = 0;
                        //foreach (Joint joint in body.Joints.Values)
                        //{
                        //    if (joint.JointType == JointType.Head && joint.TrackingState == TrackingState.Tracked)
                        //    {
                        //        CameraSpacePoint jointPosition = joint.Position;
                        //        Point point = new Point();
                        //        DepthSpacePoint depthPoint = sensor.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);
                        //        point.X = float.IsInfinity(depthPoint.X) ? 0 : depthPoint.X;
                        //        point.Y = float.IsInfinity(depthPoint.Y) ? 0 : depthPoint.Y;
                        //        if (point.X != 0 && point.Y != 0)
                        //            hBase = rawDepthData[((int)(512 * point.Y)) + (int)point.X];
                        //        break;
                        //    }
                        //}

                        //if (hBase == 0)
                        //    break;

                        foreach (Joint joint in body.Joints.Values)
                        {
                            if (joint.TrackingState != TrackingState.Tracked)
                                continue;

                            // 3D space point
                            CameraSpacePoint jointPosition = joint.Position;

                            // 2D space point
                            Point point = new Point();

                            DepthSpacePoint depthPoint = sensor.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);

                            point.X = float.IsInfinity(depthPoint.X) ? 0 : depthPoint.X;
                            point.Y = float.IsInfinity(depthPoint.Y) ? 0 : depthPoint.Y;

                            if (point.X != 0 && point.Y != 0)
                            {
                                if (joint.JointType == JointType.WristRight || joint.JointType == JointType.WristLeft)
                                {
                                    Rect region = new Rect((int)(point.X - Constants.croppedRegionWidth / 2),
                                                           (int)(point.Y - Constants.croppedRegionHeight / 2),
                                                           Constants.croppedRegionWidth,
                                                           Constants.croppedRegionWidth);
                                    ushort[] cropped_region = new ushort[Constants.croppedRegionWidth * Constants.croppedRegionHeight];
                                    try
                                    {
                                        int sum = 0;
                                        for (int i = 0; i < Constants.croppedRegionHeight/2; ++i)
                                        {
                                            for (int j = 0; j < Constants.croppedRegionWidth/2; ++j)
                                            {
                                                 sum += rawDepthData[(int)(((region.Y + i) * 512) + (region.X + j))];
                                            }
                                        }
                                        ushort basePixel = (ushort) (sum/(Constants.croppedReginSize / 4));
                                        //ushort handBase = rawDepthData[(int)((512 * region.Y) + region.X)];
                                        int cntZero = 0;
                                        for (int i = 0; i < Constants.croppedRegionHeight; ++i)
                                        {
                                            for (int j = 0; j < Constants.croppedRegionWidth; ++j)
                                            {
                                                ushort curr = rawDepthData[(int)(((region.Y + i) * 512) + (region.X + j))];
                                                if (curr <= (basePixel + 50))
                                                    cropped_region[i*Constants.croppedRegionWidth + j] =
                                                        (ushort)((rawDepthData[(int) (((region.Y + i)*512) + (region.X + j))]));
                                                else
                                                {
                                                    cropped_region[i*Constants.croppedRegionWidth + j] = 0;
                                                    ++cntZero;
                                                }
                                            }
                                        }

                                        if (cntZero > ((Constants.croppedReginSize) * 3/4))
                                            continue;

                                        //var img = Utilites.DepthToBitmap(cropped_region, Constants.croppedRegionWidth, Constants.croppedRegionHeight);
                                        if (joint.JointType == JointType.WristLeft)
                                        {
                                            statusPanel.LeftHand = cropped_region;
                                        }
                                        else
                                        {
                                            statusPanel.RightHand = cropped_region;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // nothing to do!
                                    }
                                }
                            }

                            // Draw
                            Ellipse ellipse = new Ellipse
                            {
                                Fill = Brushes.Red,
                                Width = 10,
                                Height = 10
                            };

                            Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                            Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                            SkeletonView.Children.Add(ellipse);
                        }
                    }
                    if (!proc)
                        bodyIndex = null;
                }
            }
        }
    }
}
