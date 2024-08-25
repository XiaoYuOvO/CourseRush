using System.Diagnostics;

namespace CourseRush.CLI;

public class Program
{

    class TestTask
    {
        private static int _index;
        private readonly int _id;
        public TestTask()
        {
            _id = _index++;
        }

        public async Task<int> DoTask()
        {
            Console.WriteLine($"Task started at {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(1000);
            Console.WriteLine($"Doing work at {Thread.CurrentThread.ManagedThreadId}");
            return 0;
        }
    }
    public static async Task Main(string[] args)
    {
        Console.WriteLine($"Starting at {Thread.CurrentThread.ManagedThreadId}");
        var startNew = Stopwatch.StartNew();
        List<TestTask> tasks = [new(), new(), new(), new(), new(), new(),  new(), new(), new(), new(), new()];
        await Task.WhenAll(tasks.Select(t => t.DoTask())).ContinueWith(task => Console.WriteLine(task.Status));
        startNew.Stop();
        Console.WriteLine($"Tasks completed {startNew.Elapsed} at {Environment.CurrentManagedThreadId}");
    }
}