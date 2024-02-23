using System.Collections.Concurrent;
using System.Diagnostics;
using DataLibrary;

namespace MauiMediaPlayer.ProgramLogic
{
    public class StaticProgramLogic

    {
        // VRB, DBG, INF, WRN, ERR, FTL
        public static async Task DeliverMessageQueue(ConcurrentQueue<string> messageQueue, Label messageBox)
        {
            int spinner = 0;
            bool _lastmessage = false;

            while (true)
            {
                messageQueue.TryDequeue(out string message);
                if (message == null)
                {
                    if (_lastmessage)
                    {
                        await Task.Delay(1500);
                        _lastmessage = false;
                    }
                    // 1000/25fps = 40
                    var spin = " .".Substring(spinner++%2, 1);
                    await sendMessage(spin, messageBox, 180); 
                    await Task.Delay(1000); 
                    continue;
                }
                var tag = message.Substring(32, 3);
                var msg = message.Substring(37);
                if (tag == "VRB" || tag == "DBG") 
                {
                    await sendMessage(msg, messageBox);
                    await Task.Delay(1);    // - cjm - 17000 songs even at this speed could be a relatively long time.  May need to remove it entirely.
                    _lastmessage = false;
                    continue;
                }
                else
                {
                    await sendMessage(msg, messageBox);
                    if (tag == "INF") await Task.Delay(1500);
                    else await Task.Delay(9000);
                    _lastmessage = true;
                    continue;
                }
            }
        }
        public static async Task sendMessage(string message, Label messageBox, int Rotation = 0)
        {
            if (message != null && messageBox != null)
                await messageBox.Dispatcher.DispatchAsync(() => {
                    messageBox.Rotation = Rotation;
                    messageBox.Text = message;
                    });
        }
    }
}
