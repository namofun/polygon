namespace Polygon.Entities
{
    /// <summary>
    /// The enum class for judgement verdict.
    /// </summary>
    public enum Verdict
    {
        /// <summary>
        /// Unknwon
        /// </summary>
        /// <remarks>Unknwon status in judgement</remarks>
        Unknown = 0,

        /// <summary>
        /// Time Limit Exceeded
        /// </summary>
        /// <remarks>a.k.a. TIME-LIMIT</remarks>
        TimeLimitExceeded = 1,

        /// <summary>
        /// Memory Limit Exceeded
        /// </summary>
        /// <remarks>a.k.a. MEMORY-LIMIT</remarks>
        MemoryLimitExceeded = 2,

        /// <summary>
        /// Runtime Error
        /// </summary>
        /// <remarks>a.k.a. RUN-ERROR</remarks>
        RuntimeError = 3,

        /// <summary>
        /// Output Limit Exceeded
        /// </summary>
        /// <remarks>a.k.a. OUTPUT-LIMIT</remarks>
        OutputLimitExceeded = 4,

        /// <summary>
        /// Wrong Answer
        /// </summary>
        /// <remarks>a.k.a. WRONG-ANSWER</remarks>
        WrongAnswer = 5,

        /// <summary>
        /// Compile Error
        /// </summary>
        /// <remarks>a.k.a. COMPILE-ERROR</remarks>
        CompileError = 6,

        /// <summary>
        /// Presentation Error
        /// </summary>
        [System.Obsolete]
        PresentationError = 7,

        /// <summary>
        /// Pending
        /// </summary>
        /// <remarks>a.k.a. QUEUED</remarks>
        Pending = 8,

        /// <summary>
        /// Running
        /// </summary>
        /// <remarks>a.k.a. RUNNING</remarks>
        Running = 9,

        /// <summary>
        /// Undefined Error
        /// </summary>
        /// <remarks>Internal Error in judgement</remarks>
        UndefinedError = 10,

        /// <summary>
        /// Accepted
        /// </summary>
        /// <remarks>a.k.a. CORRECT</remarks>
        Accepted = 11,
    }
}
