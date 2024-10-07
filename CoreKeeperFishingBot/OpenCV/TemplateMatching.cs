using CoreKeeperFishingBot.OpenCV;
using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;
using static OpenCvSharp.Stitcher;

namespace CoreKeeperFishingBot
{
    internal class TemplateMatching
    {
        private readonly Mat _template;
        private readonly double _threshold;

        public TemplateMatching(string pathToTemplate, double threshold)
        {
            Mat image = new Mat(pathToTemplate, ImreadModes.Color);
            _template = image.CvtColor(ColorConversionCodes.BGR2BGRA);
            //Cv2.ImShow("Template", _template);
            //Cv2.WaitKey();

            _threshold = threshold;
        }

        public bool IsBiteAlertMatched()
        {
            double matchWeight = MatchFrameWithTemplate();

            return matchWeight > _threshold;
        }

        public double MatchFrameWithTemplate()
        {
            (Mat frame, _, _) = LatestFrame.GetLatestFrameAsMat();
            if (frame is null)
                throw new Exception("Null frame!");

            // TODO: shrink frame.
            Mat matchResult = new Mat(_template.Rows, _template.Cols, MatType.CV_32FC1);
            Cv2.MatchTemplate(frame, _template, matchResult, TemplateMatchModes.CCoeffNormed);

            //Cv2.ImShow("Heat Map", matchResult);
            //Cv2.WaitKey();

            Cv2.MinMaxLoc(matchResult, out _, out double maxValue, out _, out Point maxLocation);

            //Rect rectangle = new Rect(new Point(maxLocation.X, maxLocation.Y), new Size(_template.Width, _template.Height));
            //Cv2.Rectangle(frame, rectangle, Scalar.LimeGreen, 3);
            //Cv2.ImShow("Best Match", matchResult);
            //Cv2.WaitKey();

            return maxValue;
        }

        private static (Point?, double) MatchFrame(Mat frame, Mat template, double threshold)
        {
            var result = new Mat(frame.Rows - template.Rows + 1, frame.Cols - template.Cols + 1, MatType.CV_32FC1);
            Cv2.MatchTemplate(frame, template, result, TemplateMatchModes.CCoeffNormed);

            Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);

            if (maxValue >= threshold)
                return (maxLocation, maxValue);

            return (null, 0);
        }

        private static Mat GetSubMat(Mat source, Rect region)
        {
            Mat mat = source.SubMat(region);
            return mat;
        }
    }
}
