using System;

namespace Xylab.Polygon.Judgement.Daemon
{
    public enum ExitCodes
    {
        Correct = 0,
        CompilerError = 101,
        TimeLimit = 102,
        RunError = 103,
        NoOutput = 104,
        WrongAnswer = 105,

        [Obsolete("dropped since 5.0")]
        PresentationError = 106,

        MemoryLimit = 107,
        OutputLimit = 108,
        CompareError = 120,
        InternalError = 127,
    }
}
