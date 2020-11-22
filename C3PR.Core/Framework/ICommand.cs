using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace C3PR.Core.Framework
{
    public interface ICommand
    {
        bool CanHandleMessage(CommandContext commandContext);

        Task HandleMessage(CommandContext commandContext);
    }
}
