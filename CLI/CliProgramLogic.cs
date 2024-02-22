using static AngelHornetLibrary.AhLog;
using static CommonNet8.SearchForMusic;

namespace MauiCli
{
    public static class CliProgramLogic
    {

        public static async Task FindFilesQueueFunc(string path)
        {
            Console.WriteLine($"FindFilesQueueFunc: {path}");
            await foreach (string filename in new AhGetFiles().GetFilesAsync(path, "*.mp3"))
            {
                Console.WriteLine($"Adding[61]: {filename}");
                AddFilenameToSongDb(filename);
            }
            return;
        }

        public static bool FlipCoin()
        {
            int coin = (int)new Random().Next(0, 2);
            LogInfo($"Coin: {coin}");
            if (coin == 1) return true;
            else return false;
        }

        public static string MiddleTruncate(string input)
        {
            var length = Console.BufferWidth - 1;
            if (input.Length <= length) return input;
            return input.Substring(0, length / 2 - 3) + " ... " + input.Substring(input.Length - length / 2 + 3);
        }

        public static void PlaylistWriteLine(string column1, string column2, string column3, string column4)
        {
            var width = 21;
            Console.WriteLine($"{column1.PadRight(width).Substring(0, width)}   {column2.PadRight(width).Substring(0, width)}   {column3.PadRight(width).Substring(0, width)}   {column4.PadRight(width).Substring(0, width - 2)}");
        }
    }
}