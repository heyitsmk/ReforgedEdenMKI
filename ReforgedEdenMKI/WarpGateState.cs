using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReforgedEdenMKI
{
    internal enum WarpGateState
    {
        Disabled,
        Enabled,
        PendingDialogResponse,
        WarpCountdownInitiated,
        WarpCountdown90,
        WarpCountdown60,
        WarpCountdown30,
        Cooldown
    }
}
