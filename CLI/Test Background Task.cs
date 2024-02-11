using AngelHornetLibrary.CLI;

namespace CLI
{
    class Test_Background_Class
    {
        void Method()
        {
            Console.WriteLine("Hello, World!");

            List<string> result = new List<string>();
            string searchStatus = "Searching...";

            Task task = new Task(() =>
            {
                new AhsUtil().GetFilesRef("C:\\users\\cjmetcalfe\\Music", "*.mp3", ref result, ref searchStatus, SearchOption.AllDirectories);
            }, TaskCreationOptions.LongRunning);
            task.Start();

            Task.Run(() =>
            {
                Console.WriteLine("Searching...");
                do
                {
                    string tmp = searchStatus;
                    tmp = $"[{result.Count,10}] {result.LastOrDefault()} Searching: {tmp,80}\r";
                    Console.WriteLine(tmp);
                    Task.Delay(250).Wait();
                } while (task.Status == TaskStatus.Running);

                foreach (string s in result)
                {
                    Console.WriteLine(s);
                }

                Console.WriteLine($"Found [{result.Count}] Mp3 files.");
            }).Wait();

            while (task.Status == TaskStatus.Running) ;

            foreach (string s in result)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine($"Found [{result.Count}] Mp3 files.");
        }
    }
}


