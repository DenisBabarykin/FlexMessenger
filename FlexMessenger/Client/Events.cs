using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlexMessengerClient
{
    public enum FMError : byte
    {
        TooUserName = Client.IM_TooUsername,
        TooPassword = Client.IM_TooPassword,
        Exists = Client.IM_Exists,
        NoExists = Client.IM_NoExists,
        WrongPassword = Client.IM_WrongPass
    }

    public class FMErrorEventArgs : EventArgs
    {
        FMError err;

        public FMErrorEventArgs(FMError error)
        {
            this.err = error;
        }

        public FMError Error
        {
            get { return err; }
        }
    }
    public class AvailEventArgs : EventArgs
    {
        string user;
        bool avail;

        public AvailEventArgs(string user, bool avail)
        {
            this.user = user;
            this.avail = avail;
        }

        public string UserName
        {
            get { return user; }
        }
        public bool IsAvailable
        {
            get { return avail; }
        }
    }
    public class ReceivedEventArgs : EventArgs
    {
        string user;
        string msg;

        public ReceivedEventArgs(string user, string msg)
        {
            this.user = user;
            this.msg = msg;
        }

        public string From
        {
            get { return user; }
        }
        public string Message
        {
            get { return msg; }
        }
    }

    public delegate void FMErrorEventHandler(object sender, FMErrorEventArgs e);
    public delegate void AvailEventHandler(object sender, AvailEventArgs e);
    public delegate void ReceivedEventHandler(object sender, ReceivedEventArgs e);
}
