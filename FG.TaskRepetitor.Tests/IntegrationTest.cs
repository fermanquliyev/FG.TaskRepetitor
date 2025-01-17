using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics;

namespace FG.TaskRepetitor.Tests
{
    [TestClass]
    public sealed class IntegrationTest
    {
        private TestServer _server;
        private HttpClient _client;

        [TestInitialize]
        public void Setup()
        {
            File.Delete("./TestFile.txt");
            File.Delete("./AsyncTestFile.txt");
            var webHostBuilder = new WebHostBuilder();

            webHostBuilder.UseStartup<Startup>();

            _server = new TestServer(webHostBuilder);
            _client = _server.CreateClient();
        }

        [TestMethod]
        public async Task TestServiceCollectionAndRunning()
        {
            // Access the IServiceCollection
            var services = _server.Host.Services;

            using (var scope = _server.Host.Services.CreateScope())
            {
                // Example: Check if a specific service is registered
                var myService = scope.ServiceProvider.GetService<RepetitiveTask>();
                Assert.IsNotNull(myService, "TestRepetitiveTask is not registered in the service collection.");
            }

            Task.Delay(5000).Wait(); // Wait for the task to run

            var lines = await File.ReadAllLinesAsync("./TestFile.txt");
            Assert.IsTrue(lines.Length > 1, "The task did not run.");
            foreach (var item in lines)
            {
                Debug.WriteLine(item);
            }
            lines = await File.ReadAllLinesAsync("./AsyncTestFile.txt");
            Assert.IsTrue(lines.Length > 1, "The async task did not run.");
            foreach (var item in lines)
            {
                Debug.WriteLine(item);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register services required for testing
            services.AddTaskRepetitor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the application pipeline if needed
        }
    }
}
