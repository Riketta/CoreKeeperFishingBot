using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreKeeperFishingBot
{
    internal class BotConfig
    {
        public bool Debug { get; set; } = true;
        public bool CaptureScreen { get; set; } = true;
        public string PathToTemplate { get; set; } = @"Templates\TemplateSmall.png";
        public double TemplateMatchingThreshold { get; set; } = 0.85;
        public int RightClickDuration { get; set; } = 50;
        public int AttemptInterval { get; set; } = 1500;
        public int FrameCheckInterval { get; set; } = 15;
        public int WaitForBiteTimeout { get; set; } = 10 * 1000;
    }
}
