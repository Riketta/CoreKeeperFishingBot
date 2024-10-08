﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VersaScreenCapture;

namespace CoreKeeperFishingBot
{
    internal class Bot
    {
        private readonly BotConfig _config;
        private readonly IntPtr _windowHandle;
        private readonly TemplateMatching _templateMatching;

        private DateTime lastTimeAlertSeen;

        public Bot(BotConfig config, TemplateMatching templateMatching, IntPtr windowHandle)
        {
            _config = config;
            _templateMatching = templateMatching;
            _windowHandle = windowHandle;
        }

        ~Bot()
        {
            CaptureHandler.Stop();
        }

        public void FishingLoop()
        {
            if (_config.CaptureScreen)
                CaptureHandler.StartPrimaryMonitorCapture();
            else
                CaptureHandler.StartWindowCapture(_windowHandle);

            while (true)
            {
                Console.WriteLine("Fishing attempt.");
                try
                {
                    UseItem();
                    Thread.Sleep(_config.AttemptInterval);

                    Console.WriteLine("Waiting for a bite.");
                    if (WaitForBite(_config.WaitForBiteTimeout))
                        Console.WriteLine("[+] Looting.");
                    else
                        Console.WriteLine("[-] Attempt failed.");

                    UseItem();
                    Thread.Sleep(_config.AttemptInterval);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occured while fishing: " + ex.ToString());
                }
            }
        }

        bool WaitForBite(int timeout)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            var task = Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _templateMatching.MatchFrameWithTemplate(); // TODO: hack to clean-up latest frame.
                }
                catch { }

                while (true)
                {
                    double matchWeight = 0;
                    try
                    {
                        matchWeight = _templateMatching.MatchFrameWithTemplate();
                    }
                    catch { }

                    if (_config.Debug)
                        Console.WriteLine($"Current frame best match weight: {matchWeight:F2}.");

                    if (matchWeight > _config.TemplateMatchingThreshold)
                        return true;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        //cancellationToken.ThrowIfCancellationRequested();
                        return false;
                    }
                    else
                        Thread.Sleep(_config.FrameCheckInterval);
                }
            }, cancellationToken);

            if (!task.Wait(timeout))
            {
                tokenSource.Cancel();
                return false;
            }

            return task.Result;
        }

        private void UseItem()
        {
            if (_config.Keybindings.UseItemsUsingKeyboard)
                WindowsManager.PressKeyUsingSendMessage(_windowHandle, _config.Keybindings.UseItemKey, _config.HoldKeyDownDuration);
            else
                WindowsManager.RightMouseClickMouseEvent(_windowHandle, _config.HoldKeyDownDuration);
        }
    }
}
