using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreKeeperFishingBot
{
    internal class Keybindings
    {
        public bool UseItemsUsingKeyboard { get; set; } = false;
        public NativeMethods.VirtualKeys UseItemKey { get; set; } = NativeMethods.VirtualKeys.Numpad9;
        public NativeMethods.VirtualKeys NextItemKey { get; set; } = NativeMethods.VirtualKeys.Home;
        public NativeMethods.VirtualKeys PrevItemKey { get; set; } = NativeMethods.VirtualKeys.End;
    }
}
