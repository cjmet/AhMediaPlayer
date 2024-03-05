
// Main Loop, Simulating a GUI Application
async void mainloop()
{
    int spinner = 0;
    string message = "Hello World!";
    Console.WriteLine($"{message}");


    ButtonClickedAction((y) => message = y);        // Simulated Event Handler
    var tmp = IOWrapperAsync();                     // without await, this is equivalent to Task.Run


    int i = 0;
    do
    {
        // spin for a while
        var spin = "|/-\\"[spinner++ % 4];
        Console.Write($"[{i++,2}]{spin} {message}\r");
        Thread.Sleep(1000);
    } while (message == "Hello World!");
    Console.WriteLine(message);
}



// Async Task or Async Void, since this is in theory simulating a button click event
async Task ButtonClickedAction(Action<string> CallBack)
{
    string message = await IOWrapperAsync();
    CallBack(message);
}



// Primary Program and Interactive Client
// ----
// Asynchronous Interface



async Task<string> IOWrapperAsync()     // This is effectively an interface between synchronous and asynchronous code
{
    string buffer = "";
    var task = new Task(() => buffer = BlockingIoApiCall(), TaskCreationOptions.LongRunning);
    task.Start();
    await task;
    return buffer;
}



// Asnchronous Interface
// ---
// Synchronous Blocking IO API Call



string BlockingIoApiCall()
{
    Task.Delay(3000).Wait();
    return "World Goodbye!     ";
}
