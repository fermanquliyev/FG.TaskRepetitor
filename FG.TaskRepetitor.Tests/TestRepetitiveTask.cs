namespace FG.TaskRepetitor.Tests
{
    public class TestRepetitiveTask : RepetitiveTask
    {
        public override required Schedule Schedule { get; init; } = Schedule.EverySecond(1);

        public override void Execute()
        {
            using var writer = File.AppendText("./TestFile.txt");
            writer.WriteLine("Executed at " + DateTime.UtcNow.ToString());
            writer.Flush();
        }
    }
}
