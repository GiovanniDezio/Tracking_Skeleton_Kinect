using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.IO;

namespace App2
{
    public partial class MainWindow : Window
    {
        #region Variable_Declaration

        KinectSensor Kinect;
        private DrawingGroup GroupImage;
        private DrawingImage SkeletonImage;

        Skeleton[] Skel = new Skeleton[0];
        Skeleton[] totalSkel = new Skeleton[6];

        BitmapImage image = new BitmapImage();
        bool imageChoise = false;
        JointType positionImage;

        #endregion Variable_Declaration

        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.GroupImage = new DrawingGroup();
                this.SkeletonImage = new DrawingImage(this.GroupImage);

                this.comboBox.Items.Add("Head");
                this.comboBox.Items.Add("Hand Right");
                this.comboBox.Items.Add("Hand Left");
                this.comboBox.Items.Add("Clear");

                SkelView.Source = this.SkeletonImage;
                this.Kinect = KinectSensor.KinectSensors.Where(item => item.Status == KinectStatus.Connected).FirstOrDefault();
                this.Kinect.ColorStream.Enable();
                this.Kinect.SkeletonStream.Enable();
                this.Kinect.DepthStream.Enable();
                this.Kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(Sensing_SkeletonFrameReady);
                this.Kinect.Start(); 
                RGB_DEPTH_Window prova = new RGB_DEPTH_Window(this.Kinect);
                prova.Show();
            }
            else
            {
                System.Windows.MessageBox.Show("Kinect Not Connected!");
                System.Windows.Application.Current.Shutdown();
                this.Kinect.Stop();
                return;
            }
        }

        #region Skeleton_Tracking
        private static void drawRedBorder(Skeleton skeleton, DrawingContext context)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
                context.DrawRectangle(Brushes.Red, null, new Rect(0, 640 - 10, 480, 10));
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
                context.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 10, 640));
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
                context.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 10, 640));
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
                context.DrawRectangle(Brushes.Red, null, new Rect(480 - 10, 0, 10, 640));
        }

        //Method to convert a SkeletonPoint in a Point
        private Point SkeletonPointToScreen(SkeletonPoint SkelPoin)
        {
            DepthImagePoint PoinDep = this.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(SkelPoin, DepthImageFormat.Resolution640x480Fps30);
            return new Point(PoinDep.X, PoinDep.Y);
        }

        //Method to print line between two joints
        void printLine(Joint j1, Joint j2)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Red);
            line.StrokeThickness = 5;

            ColorImagePoint j1c = Kinect.MapSkeletonPointToColor(j1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            line.X1 = j1c.X;
            line.Y1 = j1c.Y;

            ColorImagePoint j2c = Kinect.MapSkeletonPointToColor(j2.Position, ColorImageFormat.RgbResolution640x480Fps30);
            line.X2 = j2c.X;
            line.Y2 = j2c.Y;
        }

        //Method to print line of bones
        private void printBone(Skeleton skeleton, DrawingContext context, JointType tipe0, JointType tipe1)
        {
            this.printLine(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
            this.printLine(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
            this.printLine(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

            this.printLine(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
            this.printLine(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
            this.printLine(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);

            Joint J0 = skeleton.Joints[tipe0];
            Joint J1 = skeleton.Joints[tipe1];

            if (J0.TrackingState == JointTrackingState.NotTracked || J1.TrackingState == JointTrackingState.NotTracked)
                return;
            if (J0.TrackingState == JointTrackingState.Inferred && J1.TrackingState == JointTrackingState.Inferred)
                return;
            Pen GbrPen = new Pen(Brushes.Green, 6);
            if (J0.TrackingState == JointTrackingState.Tracked && J1.TrackingState == JointTrackingState.Tracked)
                GbrPen = new Pen(Brushes.Green, 6);
            context.DrawLine(GbrPen, this.SkeletonPointToScreen(J0.Position), this.SkeletonPointToScreen(J1.Position));
        }

        //Method to print full Skeleton that 
        private void printFullSkeleton(Skeleton skeleton, DrawingContext context)
        {
            this.printBone(skeleton, context, JointType.Head, JointType.ShoulderCenter);
            this.printBone(skeleton, context, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.printBone(skeleton, context, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.printBone(skeleton, context, JointType.ShoulderCenter, JointType.Spine);

            this.printBone(skeleton, context, JointType.Spine, JointType.HipCenter);
            this.printBone(skeleton, context, JointType.HipCenter, JointType.HipLeft);
            this.printBone(skeleton, context, JointType.HipCenter, JointType.HipRight);

            this.printBone(skeleton, context, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.printBone(skeleton, context, JointType.ElbowLeft, JointType.WristLeft);
            this.printBone(skeleton, context, JointType.WristLeft, JointType.HandLeft);

            this.printBone(skeleton, context, JointType.ShoulderRight, JointType.ElbowRight);
            this.printBone(skeleton, context, JointType.ElbowRight, JointType.WristRight);
            this.printBone(skeleton, context, JointType.WristRight, JointType.HandRight);

            this.printBone(skeleton, context, JointType.HipLeft, JointType.KneeLeft);
            this.printBone(skeleton, context, JointType.KneeLeft, JointType.AnkleLeft);
            this.printBone(skeleton, context, JointType.AnkleLeft, JointType.FootLeft);

            this.printBone(skeleton, context, JointType.HipRight, JointType.KneeRight);
            this.printBone(skeleton, context, JointType.KneeRight, JointType.AnkleRight);
            this.printBone(skeleton, context, JointType.AnkleRight, JointType.FootRight);

            if (this.imageChoise == true)
            {
                Point p2 = this.SkeletonPointToScreen(skeleton.Joints[this.positionImage].Position);
                p2.X -= 50;
                p2.Y -= 50;
                context.DrawImage(this.image, new Rect(p2, new Size(100, 100)));
            }

            foreach (Joint joint in skeleton.Joints)
            {
                Brush gbrBrush = null;
                if (joint.TrackingState == JointTrackingState.Tracked)
                    gbrBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
                else if (joint.TrackingState == JointTrackingState.Inferred)
                    gbrBrush = Brushes.Yellow;
                if (gbrBrush != null)
                    context.DrawEllipse(gbrBrush, null, this.SkeletonPointToScreen(joint.Position), 7, 7);
            }
        }
        //Method to manage the event about input of Skeleton Frame 
        void Sensing_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame SkelFrame = e.OpenSkeletonFrame())
            {
                if (SkelFrame == null)
                    return;
                Skel = new Skeleton[SkelFrame.SkeletonArrayLength];
                SkelFrame.CopySkeletonDataTo(Skel);
                SkelFrame.CopySkeletonDataTo(totalSkel);
                Skeleton firstSkel = (from trackskeleton in totalSkel
                                      where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                      select trackskeleton).FirstOrDefault();
                Skeleton secondSkel = (from trackskeleton in totalSkel
                                       where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                       select trackskeleton).FirstOrDefault();
                if (firstSkel == null || secondSkel == null)
                    return;
                if (firstSkel.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                    this.MapJointwithUIElement(firstSkel);
                if (secondSkel.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Tracked)
                    this.MapJointwithUIElement(secondSkel);
            }

            using (DrawingContext dc = this.GroupImage.Open())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0.0, 0.0, 480, 640));
                if (Skel.Length != 0)
                {
                    foreach (Skeleton skele in Skel)
                    {
                        drawRedBorder(skele, dc);
                        if (skele.TrackingState == SkeletonTrackingState.Tracked)
                            this.printFullSkeleton(skele, dc);
                        else if (skele.TrackingState == SkeletonTrackingState.PositionOnly)
                            dc.DrawEllipse(Brushes.Blue, null, this.SkeletonPointToScreen(skele.Position), 7, 7);
                    }
                }
                this.GroupImage.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, 480, 640));
            }
        }
        #endregion Skeleton_Tracking

        #region Print_Coordinates
        //Method to print coodinates X, Y and Z of hands of Skeleton
        private void MapJointwithUIElement(Skeleton skeleton)
        {
            if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                foreach (Skeleton skelt in Skel)
                {
                    float dephtRightHand = skeleton.Joints[JointType.HandRight].Position.Z * 100;
                    float dephtLeftHand = skeleton.Joints[JointType.HandLeft].Position.Z * 100;
                    
                    Point mappedPoint = this.SkeletonPointToScreen(skeleton.Joints[JointType.HandRight].Position);
                    this.ManoDestra.Content = string.Format("X = {0}, Y = {1}, Z = {2}", mappedPoint.X, mappedPoint.Y, dephtRightHand);
                    Point mappedPoint1 = this.SkeletonPointToScreen(skeleton.Joints[JointType.HandLeft].Position);
                    this.ManoSinistra.Content = string.Format("X = {0}, Y = {1}, Z = {2}", mappedPoint1.X, mappedPoint1.Y, dephtLeftHand);
                }
            }
        }

        #endregion Print_Coordinates

        private void Closed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Kinect.Stop();
            App.Current.Shutdown();
        }

        #region Choise_Image

        private void ScegliFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();

            if(open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (this.imageChoise == true)
                    this.image = new BitmapImage();
                this.image.BeginInit();
                this.image.UriSource = new Uri(open.FileName, UriKind.RelativeOrAbsolute);
                this.image.EndInit();
                this.imageChoise = true;
            }

        }

        private void Usa_Immagine_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox.Text == "Head")
                this.positionImage = JointType.Head;
            else if (comboBox.Text == "Hand Right")
                this.positionImage = JointType.HandRight;
            else if (comboBox.Text == "Hand Left")
                this.positionImage = JointType.HandLeft;
            else if (comboBox.Text == "Clear")
                this.imageChoise = false;
            else if (this.imageChoise == true)
            {
                System.Windows.MessageBox.Show("Choise position!");
                this.imageChoise = false;
            }
        }

        #endregion Choise_Image
    }
}
