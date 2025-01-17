using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FG.TaskRepetitor.Tests
{
    public class AsyncTestRepetitiveTask : AsyncRepetitiveTask
    {
        public override required Schedule Schedule { get ; init ; } = Schedule.EverySecond(2);

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://www.google.com/");
            using var writer = File.AppendText("./AsyncTestFile.txt");
            writer.WriteLine("Executed at " + DateTime.UtcNow.ToString());
            writer.WriteLine("Response status code: " + response.StatusCode);
            writer.Flush();
        }
    }
}
