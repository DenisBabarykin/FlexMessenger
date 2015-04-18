﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FlexMessengerClient
{
    public partial class TalkForm : Form
    {
        public TalkForm(Client fm, string user)
        {
            InitializeComponent();
            this.fm = fm;
            this.sendTo = user;
        }

        public Client fm;
        public string sendTo;

        AvailEventHandler availHandler;
        ReceivedEventHandler receivedHandler;
        private void TalkForm_Load(object sender, EventArgs e)
        {
            this.Text = sendTo;
            availHandler = new AvailEventHandler(UserAvailable);
            receivedHandler = new ReceivedEventHandler(MessageReceived);
            fm.UserAvailable += availHandler;
            fm.MessageReceived += receivedHandler;
            fm.IsAvailable(sendTo);
        }
        private void TalkForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            fm.UserAvailable -= availHandler;
            fm.MessageReceived -= receivedHandler;
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            fm.SendMessage(sendTo, sendText.Text);
            talkText.Text += String.Format("[{0}] {1}\r\n", fm.UserName, sendText.Text);
            sendText.Text = "";
        }

        bool lastAvail = false;
        void UserAvailable(object sender, AvailEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                if (e.UserName == sendTo)
                {
                    if (lastAvail != e.IsAvailable)
                    {
                        lastAvail = e.IsAvailable;
                        string avail = (e.IsAvailable ? "available" : "unavailable");
                        this.Text = String.Format("{0} - {1}", sendTo, avail);
                        talkText.Text += String.Format("[{0} is {1}]\r\n", sendTo, avail);
                    }
                }
            }));
        }
        void MessageReceived(object sender, ReceivedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                if (e.From == sendTo)
                {
                    talkText.Text += String.Format("[{0}] {1}\r\n", e.From, e.Message);
                }
            }));
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            fm.IsAvailable(sendTo);
        }
    }
}
