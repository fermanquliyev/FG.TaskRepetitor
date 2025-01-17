using NCrontab;

namespace FG.TaskRepetitor
{
    /// <summary>
    /// Represents a schedule for repetitive tasks.
    /// </summary>
    public sealed class Schedule
    {
        private readonly Func<TimeSpan> interval;

        private Schedule(Func<TimeSpan> interval)
        {
            this.interval = interval;
        }

        public DateTime GetNextOccurrence()
        {
            return DateTime.UtcNow + interval.Invoke();
        }

        /// <summary>
        /// Creates a new schedule that will repeat every given day of the week.
        /// </summary>
        /// <param name="dayOfWeek">The day of the week to schedule the task.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule EveryWeekDay(DayOfWeek dayOfWeek)
        {
            return new Schedule(() => CalculateTimeToNextDayOfWeek(dayOfWeek));
        }

        /// <summary>
        /// Creates a new schedule that will repeat every given days interval.
        /// </summary>
        /// <param name="days">The number of days between each repetition.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule EveryDay(double days = 1)
        {
            return new Schedule(() => TimeSpan.FromDays(days));
        }

        /// <summary>
        /// Creates a new schedule that will repeat every given hours interval.
        /// </summary>
        /// <param name="hours">The number of hours between each repetition.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule EveryHour(double hours = 1)
        {
            return new Schedule(() => TimeSpan.FromHours(hours));
        }

        /// <summary>
        /// Creates a new schedule that will repeat every given minutes interval.
        /// </summary>
        /// <param name="minutes">The number of minutes between each repetition.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule EveryMinute(double minutes = 1)
        {
            return new Schedule(() => TimeSpan.FromMinutes(minutes));
        }

        /// <summary>
        /// Creates a new schedule that will repeat every given seconds interval.
        /// </summary>
        /// <param name="seconds">The number of seconds between each repetition.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule EverySecond(double seconds = 1)
        {
            return new Schedule(() => TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Creates a new schedule based on a cron expression.
        /// </summary>
        /// <param name="cronExpression">The cron expression to determine the schedule.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule Cron(string cronExpression)
        {
            return new Schedule(() => CalculateNextOccurrenceFromCron(cronExpression));
        }

        /// <summary>
        /// Creates a new custom schedule based on a provided interval calculator function.
        /// </summary>
        /// <param name="intervalCalculator">A function that calculates the interval until the next occurrence.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule Custom(Func<DateTime, TimeSpan> intervalCalculator)
        {
            return new Schedule(() => intervalCalculator(DateTime.Now));
        }

        /// <summary>
        /// Creates a new schedule that will repeat at a specific time of day.
        /// </summary>
        /// <param name="timeOfDay">The time of day to schedule the task.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule AtTimeOfDay(TimeSpan timeOfDay)
        {
            return new Schedule(() => CalculateTimeToNextTimeOfDay(timeOfDay));
        }

        /// <summary>
        /// Creates a new schedule that will repeat every given month on a specific day and time.
        /// </summary>
        /// <param name="dayOfMonth">The day of the month to schedule the task.</param>
        /// <param name="timeOfDay">The time of day to schedule the task.</param>
        /// <returns>A new Schedule instance.</returns>
        public static Schedule Monthly(int dayOfMonth, TimeSpan timeOfDay)
        {
            return new Schedule(() => CalculateTimeToNextMonthlyOccurrence(dayOfMonth, timeOfDay));
        }

        private static TimeSpan CalculateTimeToNextMonthlyOccurrence(int dayOfMonth, TimeSpan timeOfDay)
        {
            DateTime now = DateTime.Now;
            DateTime thisMonth = new DateTime(now.Year, now.Month, 1).Add(timeOfDay);
            DateTime nextRun = thisMonth.AddDays(dayOfMonth - 1);
            if (nextRun < now) nextRun = nextRun.AddMonths(1);
            return nextRun - now;
        }

        private static TimeSpan CalculateTimeToNextTimeOfDay(TimeSpan timeOfDay)
        {
            DateTime now = DateTime.Now;
            DateTime todayRun = now.Date.Add(timeOfDay);
            DateTime nextRun = todayRun > now ? todayRun : todayRun.AddDays(1);
            return nextRun - now;
        }

        private static TimeSpan CalculateNextOccurrenceFromCron(string cronExpression)
        {
            var cron = CrontabSchedule.Parse(cronExpression);
            var now = DateTime.Now;
            var nextOccurrence = cron.GetNextOccurrence(now);
            return nextOccurrence - now;
        }

        private static TimeSpan CalculateTimeToNextDayOfWeek(DayOfWeek dayOfWeek)
        {
            DateTime now = DateTime.Now;
            int daysUntilNextDayOfWeek = ((int)dayOfWeek - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilNextDayOfWeek == 0)
            {
                daysUntilNextDayOfWeek = 7; // If today is the dayOfWeek, schedule for the next week
            }
            DateTime nextDayOfWeek = now.AddDays(daysUntilNextDayOfWeek).Date;
            return nextDayOfWeek - now;
        }
    }
}
