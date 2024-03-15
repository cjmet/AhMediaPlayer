using AhConfig;
using DataLibrary;
using System.Collections.Concurrent;


namespace MauiMediaPlayer.ProgramLogic
{
    public class StaticProgramLogic

    {
        // VRB, DBG, INF, WRN, ERR, FTL
        public static async Task DeliverMessageQueue(ConcurrentQueue<string> messageQueue, Label spinBox, Label messageBox)
        {
            int spinner = 0;
            string queuedMsg = "";
            int queuedMsgLevel = -1;
            string msg = "";
            string tag = "";
            int msgLevel = -1;
            int lastSongCount = 0;



            Dictionary<string, int> msgLevelDict = new Dictionary<string, int>
            {
                { "VRB", Const.VRB },
                { "DBG", Const.DBG },
                { "INF", Const.INF },
                { "WRN", Const.WRN },
                { "ERR", Const.ERR },
                { "FTL", Const.FTL }
            };

            // Sub-Task
            new Task(async () =>
            {
                while (true)
                {
                    if (queuedMsgLevel >= 0 && queuedMsg != "")
                    {
                        // 'x'=60, 'M'= 38, Pixels=390 - 20 = 370;  Avg=6.2, Wide=9.7
                        // Default FontSize is 12, and since the 12 is hard coded, might as well well card code the 6.5 too.
                        //var denom = 12 / Const.FontSizeDivisor;
                        var width = (int)(messageBox.Width / 6.5) - 1;
                        queuedMsg = AngelHornetLibrary.AhStrings.MiddleTruncate(queuedMsg, width);
                        var spin = Const.SpinChars.Substring(spinner++ % 2, 1);
                        sendMessages(queuedMsg, messageBox, spin, spinBox);
                        var displayDuration = queuedMsgLevel;
                        queuedMsg = "";
                        queuedMsgLevel = -1;
                        await Task.Delay(displayDuration);
                    }
                    else
                    {
                        var _songCount = new PlaylistContext().Songs.Count();   // cj ... kill this when we get a chance to refactor
                        if (_songCount != lastSongCount)
                        {
                            lastSongCount = _songCount;
                            queuedMsg = $"{_songCount} Songs Found";
                            queuedMsgLevel = 1000;
                        }
                        else
                        {
                            queuedMsg = $"Angel Hornet Media Player";
                            queuedMsgLevel = 1000;
                        }
                    }
                    await Task.Delay(250);
                }
            }, TaskCreationOptions.LongRunning).Start();
            // /Sub-Task

            while (true)
            {
                messageQueue.TryDequeue(out string message);
                if (message != null)
                {
                    if (message.Length > 37)
                    {
                        tag = message.Substring(32, 3);
                        msg = message.Substring(37);
                        msgLevelDict.TryGetValue(tag, out msgLevel);
                        if (msgLevel >= queuedMsgLevel)
                        {
                            queuedMsg = msg;
                            queuedMsgLevel = msgLevel;
                        }
                    }
                }
                else
                {
                    await Task.Delay(Const.ClockTick);
                }
            }
        }

        public static async Task sendMessages(string message, Label messageBox, string? spin = null, Label? spinBox = null)
        {
            if (message != null && messageBox != null)
                await messageBox.Dispatcher.DispatchAsync(() =>
                {
                    if (spin != null && spinBox != null) spinBox.Text = spin;
                    messageBox.Text = message;
                });
        }
    }
}
