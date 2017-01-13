using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RimTrans.Builder
{
    public static class TransLog
    {
        public enum Type
        {
            Message, Warning, Error,
        }

        public class MessageArgs : EventArgs
        {
            public MessageArgs(Type type, string titile, string detail)
            {
                this.Type = type;
                this.Title = titile;
                this.Detail = detail;
            }
            public Type Type { get; private set; }
            public string Title { get; private set; }
            public string Detail { get; private set; }
        }

        public static void Message(object sender, MessageArgs e)
        {
            if (MessageEventHandler != null)
            {
                MessageEventHandler(sender, e);
            }
        }
        public static event EventHandler<MessageArgs> MessageEventHandler;
        
    }
}
