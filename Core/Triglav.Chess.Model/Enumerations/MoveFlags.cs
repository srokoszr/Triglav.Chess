using System;
using System.Collections.Generic;
using System.Text;

namespace Triglav.Chess.Model.Enumerations
{
    [Flags]
    public enum MoveFlags
    {
        None = 0,

        IsCheck = 1,

        IsCapture = 2,
    }
}
