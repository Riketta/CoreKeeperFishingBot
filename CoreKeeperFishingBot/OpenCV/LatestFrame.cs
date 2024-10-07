using OpenCvSharp;
using System.Diagnostics;

namespace CoreKeeperFishingBot.OpenCV
{
    internal class LatestFrame : VersaScreenCapture.LatestFrame
    {
        public static (Mat, int width, int height) GetLatestFrameAsMat()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            (byte[] frameBytes, int width, int height, int stride) = GetLatestFrameAsByteBgra();
            if (frameBytes == null) return (null, 0, 0);

            // 8UC4: 8 unsigned bits * 4 colors (BGRA), Padding: width * (4 bytes / pixel).
            Mat frameMat = Mat.FromPixelData(height, width, MatType.CV_8UC4, frameBytes, stride);

            watch.Stop();
            MatTime = watch.ElapsedMilliseconds;

            return (frameMat, width, height);
        }
    }
}
