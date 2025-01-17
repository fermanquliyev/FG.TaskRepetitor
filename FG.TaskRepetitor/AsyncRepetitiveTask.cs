namespace FG.TaskRepetitor
{
    /// <summary>
    /// Represents a repetitive task with a defined schedule that can be executed asynchronously.
    /// </summary>
    public abstract class AsyncRepetitiveTask
    {
        public abstract required Schedule Schedule { get; init; }
        public DateTime NextRun { get; private set; }
        public virtual TimeSpan RetryTimeIfFailed { get; protected set; } = TimeSpan.FromMinutes(1);
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
        /// Executes the repetitive task asynchronously.
        /// </summary>
        public abstract Task ExecuteAsync();

        public virtual Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }
    }
}
