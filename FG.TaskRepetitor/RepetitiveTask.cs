namespace FG.TaskRepetitor
{
    /// <summary>
    /// Represents a repetitive task with a defined schedule.
    /// </summary>
    public abstract class RepetitiveTask
    {
        public abstract required Schedule Schedule { get; init; }

        public virtual TimeSpan RetryTimeIfFailed { get; protected set; } = TimeSpan.FromMinutes(1);
        public DateTime NextRun { get; private set; }

        public void CalculateNextRun()
        {
            if (Schedule == null)
                throw new InvalidOperationException("Schedule is not set.");
            NextRun = Schedule.GetNextOccurrence();
        }
        public void CalculateNextRetry()
        {
            NextRun = DateTime.Now + RetryTimeIfFailed;
        }
        /// <summary>
        /// Executes the repetitive task.
        /// </summary>
        public abstract void Execute();

        public virtual void OnError(Exception ex)
        {
            return;
        }
    }
}
