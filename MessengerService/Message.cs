using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace MessengerService
{
    public class Message : EventArgs
    {
        public int ID { get; set; }
        public string SenderName { get; set; }
        public DateTime Time { get; set; }
        public string Text { get; set; }

        public Message() { }

        public Message(string sender, DateTime time, string message)
        {
            SenderName = sender;
            Time = time;
            Text = message;
        }

        public override string ToString()
        {
            return Time.ToShortTimeString() + ": " + SenderName + "    " + Text;
        }
    }
}
