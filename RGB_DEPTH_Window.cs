using System;
using System.Windows.Forms;
using Microsoft.Kinect;

using Coding4Fun.Kinect.WinForm;

namespace App2
{
    public partial class RGB_DEPTH_Window : Form
    {
        private KinectSensor kin;
        public RGB_DEPTH_Window()
        {
            InitializeComponent();
        }
        public RGB_DEPTH_Window(KinectSensor x)
        {
            this.kin = x;
            InitializeComponent();
            
        }

        private void RGB_DEPTH_Window_Load(object sender, EventArgs e)
        {
            
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try {
                this.kin.ElevationAngle += 3;
            }
            catch (Exception) {
                MessageBox.Show("Error");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try{
                this.kin.ElevationAngle-=3;
            }
            catch (Exception) {
                MessageBox.Show("Error");
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Enabled = false;
        }

        private void Sensing_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    var pixelData = new Byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this.pictureBox1.Image = pixelData.ToBitmap(frame.Width, frame.Height);
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.label1.Text = this.kin.Status.ToString();
            this.kin.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            this.kin.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(Sensing_ColorFrameReady);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.label1.Text = this.kin.Status.ToString();
            this.kin.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            this.kin.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kin_DepthFrameReady);
        }

        private void kin_DepthFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                    return;
                GeneratorDepth image = new GeneratorDepth(depthFrame);
                byte[] pixel = image.GenerateColoredBytes();
                this.pictureBox2.Image = pixel.ToBitmap(depthFrame.Width, depthFrame.Height);
            }
        }
    }
}
