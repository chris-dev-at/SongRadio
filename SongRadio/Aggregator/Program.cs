namespace Aggregator;

class Program
{
    static void Main(string[] args)
    {
        // Set up a way to keep the application running
        ManualResetEvent _quitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (sender, eArgs) =>
        {
            _quitEvent.Set();
            eArgs.Cancel = true;
        };
        
        
        var azureStorageAccountConnectionString = 
            Environment.GetEnvironmentVariable("rabbitMqHostname", EnvironmentVariableTarget.Process) ?? "localhost";
        var rabbitMqHost = 
            Environment.GetEnvironmentVariable("azureStorageServiceConnectionString", EnvironmentVariableTarget.Process) ?? "UseDevelopmentStorage=true;";
        
        
        
        
        //keep the application running
        _quitEvent.WaitOne();
    }
}