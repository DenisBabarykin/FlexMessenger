using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FlexMessengerClient
{
    public partial class MainForm : Form
    {
        Client fm = new Client();

        public MainForm()
        {
            InitializeComponent();

            fm.LoginOK += new EventHandler(LoginOK);
            fm.RegisterOK += new EventHandler(RegisterOK);
            fm.LoginFailed += new FMErrorEventHandler(LoginFailed);
            fm.RegisterFailed += new FMErrorEventHandler(RegisterFailed);
            fm.Disconnected += new EventHandler(Disconnected);
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            LogRegForm info = new LogRegForm();
            if (info.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fm.Register(info.UserName, info.Password);
                status.Text = "Registering...";
            }
        }
        private void loginButton_Click(object sender, EventArgs e)
        {
            LogRegForm info = new LogRegForm();
            if (info.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fm.Login(info.UserName, info.Password);
                status.Text = "Login...";
            }
        }

        void LoginOK(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                status.Text = "Logged in!";
                registerButton.Enabled = false;
                loginButton.Enabled = false;
                logoutButton.Enabled = true;
                talkButton.Enabled = true;
            }));
        }
        void RegisterOK(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                status.Text = "Registered!";
                registerButton.Enabled = false;
                loginButton.Enabled = false;
                logoutButton.Enabled = true;
                talkButton.Enabled = true;
            }));
        }
        void LoginFailed(object sender, FMErrorEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                status.Text = "Login failed!";
            }));
        }
        void RegisterFailed(object sender, FMErrorEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                status.Text = "Register failed!";
            }));
        }
        void Disconnected(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                status.Text = "Disconnected!";
                registerButton.Enabled = true;
                loginButton.Enabled = true;
                logoutButton.Enabled = false;
                talkButton.Enabled = false;

                foreach (TalkForm tf in talks)
                    tf.Close();
            }));
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            fm.Disconnect();
        }

        List<TalkForm> talks = new List<TalkForm>();
        private void talkButton_Click(object sender, EventArgs e)
        {
            TalkForm tf = new TalkForm(fm, sendTo.Text);
            sendTo.Text = "";
            talks.Add(tf);
            tf.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            fm.Disconnect();
        }
    }
}
