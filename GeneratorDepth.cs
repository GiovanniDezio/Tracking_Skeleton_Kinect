using System;

using Microsoft.Kinect;

namespace App2
{
    class GeneratorDepth
    {
        private DepthImageFrame depthFrame;
        private readonly int blueIndex = 0;
        private readonly int greenIndex = 1;
        private readonly int redIndex = 2;

        public GeneratorDepth(DepthImageFrame frame)
        {
            this.depthFrame = frame;
        }

        public byte[] GenerateColoredBytes()
        {
            short[] rawDepthData = new short[this.depthFrame.PixelDataLength];
            this.depthFrame.CopyPixelDataTo(rawDepthData);

            Byte[] pixels = new byte[this.depthFrame.Width * this.depthFrame.Height * 4];

            for (int depthIndex = 0, colorIndex = 0; depthIndex < rawDepthData.Length && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                byte intensity = this.calculateIntensityFromDepth(depth);
                pixels[colorIndex + blueIndex] = intensity;
                pixels[colorIndex + greenIndex] = intensity;
                pixels[colorIndex + redIndex] = intensity;
            }
            return pixels;
        }

        private byte calculateIntensityFromDepth(int distance)
        {
            int lowerLimit = 900;
            int upperLimit = 4000;

            if (distance <= lowerLimit) return 255;
            if (distance > upperLimit) return 0;

            int range = (upperLimit - lowerLimit);
            int levels = (range / 255);

            return (byte)(255 - (distance / levels));
        }
    }
}
