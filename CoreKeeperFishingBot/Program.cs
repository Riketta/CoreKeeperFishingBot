using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreKeeperFishingBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Process game = GetGameProcess();
            BotConfig config = new BotConfig();

            Console.WriteLine("Press \"Enter\" to capture game window handle in 3 seconds. Switch to game window during this time.");
            Console.ReadLine();
            for (int i = 3; i > 0; i--)
            {
                Console.WriteLine($"{i}...");
                Thread.Sleep(1000);
            }

            UnityGameWindowFinder gameWindowFinder = new UnityGameWindowFinder();
            IntPtr windowHandle = gameWindowFinder.FindMainWindow(game.Id);
            //IntPtr windowHandle = NativeMethods.GetForegroundWindow();
            Console.WriteLine($"Game window handle: {windowHandle}.");

            TemplateMatching templateMatching = new TemplateMatching(config.PathToTemplate, config.TemplateMatchingThreshold);
            Bot bot = new Bot(config, templateMatching, windowHandle);

            //Console.WriteLine("Press \"Enter\" to start.");
            //Console.ReadLine();
            bot.FishingLoop();
        }

        private static Process GetGameProcess()
        {
            return Process.GetProcessesByName("CoreKeeper").FirstOrDefault();
        }
    }
}
