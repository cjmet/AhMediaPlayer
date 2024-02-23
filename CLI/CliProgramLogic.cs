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



        public static int RollD20() => RollDie(20);
        public static int RollDie(int sides = 6)
        {
            int die = (int)new Random().Next(1, sides + 1);
            LogInfo($"Die: {die}");
            return die;
        }
        


        public static bool FlipCoin(int sides = 2)
        {
            int coin = (int)new Random().Next(0, sides);
            LogInfo($"Coin: {coin}");
            if (coin == 0) return true;
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