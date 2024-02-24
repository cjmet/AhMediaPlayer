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
            int _lastmessage = 0;
            string tag;
            string msg;

            while (true)
            {
                messageQueue.TryDequeue(out string message);
                if (message == null)
                {
                    if (_lastmessage > 0)
                    {
                        await Task.Delay(_lastmessage);
                        _lastmessage = 0;
                        continue;
                    }
                    else
                    {
                        msg = "Angel Hornet Media Player";
                        var spin = " .".Substring(spinner++ % 2, 1);
                        await sendMessage(spin, msg, spinBox, messageBox);
                        await Task.Delay(1000);
                        continue;
                    }
                }
                
                tag = message.Substring(32, 3);
                msg = message.Substring(37);

                // If possible do NOT queue VRB messages
                if (tag == "VRB") { continue; }
                else if (tag == "DBG") 
                {
                    
                    await sendMessage("*", msg, spinBox, messageBox);
                    await Task.Delay(1);    
                    _lastmessage = 1000;
                    continue;
                }
                else
                {
                    await sendMessage("*", msg, spinBox, messageBox);
                    if (tag == "INF") await Task.Delay(1500);
                    else await Task.Delay(9000);
                    _lastmessage = 1500;
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
