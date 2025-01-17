# FG Task Repetitor Library

[![NuGet Version](https://img.shields.io/nuget/v/FG.TaskRepetitor.svg)](https://www.nuget.org/packages/FG.TaskRepetitor/)

The Task Repetitor library provides a simple and flexible way to schedule and execute repetitive tasks, both synchronous and asynchronous, in .NET applications. This library is designed to streamline task scheduling with a wide range of scheduling options, error-handling mechanisms, and retry logic.

---

## Features

- **Easy Task Scheduling**: Define schedules using built-in methods such as daily, weekly, hourly, or custom intervals.
- **Synchronous and Asynchronous Task Support**: Create repetitive tasks using `RepetitiveTask` (for synchronous tasks) or `AsyncRepetitiveTask` (for asynchronous tasks).
- **Custom Retry Logic**: Define retry intervals for failed tasks.
- **Cron Expression Support**: Schedule tasks using cron expressions.
- **Dependency Injection Ready**: Integrates seamlessly with .NET's `IServiceCollection`.

---

## Installation

1. Add the library to your project via NuGet:
   ```bash
   dotnet add package FG.TaskRepetitor
   ```
2. Register the library in your `Program.cs`:
   ```csharp
   builder.Services.AddTaskRepetitor();
   ```

---

## Getting Started

### 1. Create a Task

#### Synchronous Task
Create a class that inherits from `RepetitiveTask`:
```csharp
public class MyRepetitiveTask : RepetitiveTask
{
    public override required Schedule Schedule { get; init; } = Schedule.EveryMinute();

    public override void Execute()
    {
        Console.WriteLine($"Task executed at: {DateTime.Now}");
    }

    public override void OnError(Exception ex)
    {
        Console.WriteLine($"Error occurred: {ex.Message}");
    }
}
```

#### Asynchronous Task
Create a class that inherits from `AsyncRepetitiveTask`:
```csharp
public class MyAsyncRepetitiveTask : AsyncRepetitiveTask
{
    public override required Schedule Schedule { get; init; } = Schedule.EveryHour();

    public override async Task ExecuteAsync()
    {
        Console.WriteLine($"Task executed asynchronously at: {DateTime.Now}");
        await Task.Delay(1000); // Simulate work
    }

    public override Task OnErrorAsync(Exception ex)
    {
        Console.WriteLine($"Error occurred: {ex.Message}");
        return Task.CompletedTask;
    }
}
```

---

### 2. Define a Schedule
The `Schedule` class provides several pre-defined methods to configure repetitive intervals:

- **Daily**: `Schedule.EveryDay()`
- **Weekly**: `Schedule.EveryWeekDay(DayOfWeek.Monday)`
- **Hourly**: `Schedule.EveryHour()`
- **Hourly**: `Schedule.EveryHour(4)` (Runs every 4 hours)
- **Minutes**: `Schedule.EveryMinute(5)`
- **Minutes**: `Schedule.EveryMinute()` (Runs every minute)
- **Seconds**: `Schedule.EverySecond(30)` (Runs every 30 seconds)
- **At Time of Day**: `Schedule.AtTimeOfDay(new TimeSpan(20, 0, 0))` (Runs at 8 PM)
- **Monthly**: `Schedule.EveryMonth(15, new TimeSpan(8, 0, 0))` (Runs on the 15th of every month at 8 AM)
- **Cron**: `Schedule.Cron("0 0 * * *")` (Runs at midnight daily)
- **Custom**: `Schedule.Custom(now => TimeSpan.FromMinutes(15))`

---

### 3. Register and Run the Tasks

1. Register tasks using the `AddTaskRepetitor` extension:
   ```csharp
   builder.Services.AddTaskRepetitor();
   ```

2. The library automatically detects and registers all `RepetitiveTask` and `AsyncRepetitiveTask` implementations in your project.

3. Start the application. The tasks will execute based on their defined schedules.

---

## Advanced Usage

### Retry Logic
You can define retry logic by setting the `RetryTimeIfFailed` property:
```csharp
public class MyTaskWithRetry : RepetitiveTask
{
    public override required Schedule Schedule { get; init; } = Schedule.EveryDay();

    public override TimeSpan RetryTimeIfFailed { get; protected set; } = TimeSpan.FromMinutes(5);

    public override void Execute()
    {
        Console.WriteLine($"Executing with retry logic.");
        throw new Exception("Simulated failure");
    }

    public override void OnError(Exception ex)
    {
        Console.WriteLine($"Retrying after error: {ex.Message}");
    }
}
```

### Custom Error Handling
Override `OnError` or `OnErrorAsync` to handle task errors:
```csharp
public override void OnError(Exception ex)
{
    // Log error or implement fallback logic
    Console.WriteLine($"Task failed: {ex.Message}");
}
```
Exceptions thrown in the `Execute` or `ExecuteAsync` methods are automatically caught and passed to the `OnError` or `OnErrorAsync` methods. 
It is optional to override these methods, except when custom error handling is required.
Errors that occur in the `OnError` or `OnErrorAsync` methods are logged but do not affect the task execution.

---

## Examples

### Example 1: Scheduling a Weekly Task
```csharp
public class WeeklyReportTask : RepetitiveTask
{
    public override required Schedule Schedule { get; init; } = Schedule.EveryWeekDay(DayOfWeek.Friday);

    public override void Execute()
    {
        Console.WriteLine($"Generating weekly report at {DateTime.Now}");
    }
}
```

### Example 2: Scheduling an Asynchronous Task
```csharp
public class CleanupTask : AsyncRepetitiveTask
{
    public override required Schedule Schedule { get; init; } = Schedule.AtTimeOfDay(new TimeSpan(2, 0, 0));

    public override async Task ExecuteAsync()
    {
        Console.WriteLine($"Cleaning up resources at {DateTime.Now}");
        await Task.Delay(2000); // Simulate cleanup
    }
}
```

---

## FAQ

### Q: How are tasks discovered and registered?
The library scans the calling assembly for all classes inheriting from `RepetitiveTask` and `AsyncRepetitiveTask`. These tasks are automatically registered as scoped services.

### Q: How do I handle task dependencies?
Define dependencies in your task constructor, and they will be injected via .NET's DI system:
```csharp
public class DependencyInjectedTask : RepetitiveTask
{
    private readonly IService _service;

    public DependencyInjectedTask(IService service)
    {
        _service = service;
    }

    public override required Schedule Schedule { get; init; } = Schedule.EveryHour();

    public override void Execute()
    {
        _service.DoWork();
    }
}
```

### Q: Can I disable certain tasks?
Yes, simply exclude the task from your project or register it conditionally based on application settings.

---

## Contributing
Contributions are welcome! Feel free to open issues or submit pull requests.

---

## License
This library is licensed under the MIT License. See the [LICENSE](https://opensource.org/license/mit) file for details.

