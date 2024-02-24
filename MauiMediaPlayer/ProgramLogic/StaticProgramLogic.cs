using System.Collections.Concurrent;
using System.Diagnostics;
using DataLibrary;

namespace MauiMediaPlayer.ProgramLogic
{
    public class StaticProgramLogic

    {
        // VRB, DBG, INF, WRN, ERR, FTL
        public static async Task DeliverMessageQueue(ConcurrentQueue<string> messageQueue, Label spinBox, Label messageBox)
        {
            int spinner = 0;
            bool _lastmessage = false;
            string tag;
            string msg;

            while (true)
            {
                messageQueue.TryDequeue(out string message);
                var spin = " .".Substring(spinner++ % 2, 1);
                if (message == null)
                {
                    if (_lastmessage)
                    {
                        await Task.Delay(1500);
                        _lastmessage = false;
                    }
                    msg = "Angel Hornet Media Player";
                    await sendMessage(spin, msg, spinBox, messageBox); 
                    await Task.Delay(1000); 
                    continue;
                }
                tag = message.Substring(32, 3);
                msg = message.Substring(37);
                if (tag == "VRB" || tag == "DBG") 
                {
                    await sendMessage(spin, msg, spinBox, messageBox);
                    await Task.Delay(1);    // - cjm - 17000 songs even at this speed could be a relatively long time.  May need to remove it entirely.
                    _lastmessage = false;
                    continue;
                }
                else
                {
                    await sendMessage(spin, msg, spinBox, messageBox);
                    if (tag == "INF") await Task.Delay(1500);
                    else await Task.Delay(9000);
                    _lastmessage = true;
                    continue;
                }
            }
        }
        public static async Task sendMessage(string spin, string message, Label spinBox, Label messageBox)
        {
            if (message != null && messageBox != null)
                await messageBox.Dispatcher.DispatchAsync(() => {
                    spinBox.Text = spin;
                    messageBox.Text = message;
                    });
        }
    }
}
