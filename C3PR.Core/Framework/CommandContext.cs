using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace C3PR.Core.Framework
{
    public class CommandContext
    {
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string ChannelName { get; set; }
        public string UserName { get; set; }
    }
}
