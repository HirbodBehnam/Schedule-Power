using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Schedule_Power
{
    public partial class Main : Form
    {
        int type = 0;
        bool toolTipShown = false;
        TimeSpan Remaining;
        readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        public Main()
        {
            InitializeComponent();
            TypeOfAction.SelectedIndex = Properties.Settings.Default.IndexOfAction;
            if (Properties.Settings.Default.FirstRun)
            {
                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.LastTime = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                Properties.Settings.Default.Save();
            }
            else
            {
                TimeSpan ts = Properties.Settings.Default.LastTime;
                DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, ts.Hours, ts.Minutes, ts.Seconds);
                dateTimePicker1.Value = dateTime;
            }
        }

        private void AbortActionBTN_Click(object sender, EventArgs e)
        {
            GoBTN.Text = "Go";
            AbortActionBTN.Enabled = false;
            GoBTN.Enabled = true;
            TypeOfAction.Enabled = true;
            dateTimePicker1.Enabled = true;
            TimerTo.Stop();
        }

        private void GoBTN_Click(object sender, EventArgs e)
        {
            TimeSpan s = TimeSpan.Parse(dateTimePicker1.Value.ToString("HH:mm:ss"));
            TimeSpan n = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
            Remaining = s - n;
            type = TypeOfAction.SelectedIndex;
            //Save Values
            Properties.Settings.Default.IndexOfAction = type;
            Properties.Settings.Default.LastTime = s;
            Properties.Settings.Default.Save();
            //Check if time is negative
            if (Remaining.TotalSeconds < 0)
            {
                Remaining = TimeSpan.Parse("23:59:59") - Remaining.Duration();
            }
            TypeOfAction.Enabled = false;
            AbortActionBTN.Enabled = true;
            GoBTN.Enabled = false;
            GoBTN.Text = Remaining.ToString();
            dateTimePicker1.Enabled = false;
            TimerTo.Start();
        }
        private void TimerTo_Tick(object sender, EventArgs e)
        {
            Remaining -= OneSecond;
            if (Remaining.TotalSeconds < 1)
            {
                TimerTo.Stop();
                DoAction();
            }
            GoBTN.Text = Remaining.ToString();
        }
        private void DoAction()
        {
            bool Force = checkBox1.Checked;
            switch (type)
            {
                case 0:
                    Process.Start("shutdown", "/s /t 0 " + (Force ? "/f" : ""));
                    break;
                case 1:
                    Process.Start("shutdown", "/r /t 0 " + (Force ? "/f" : ""));
                    break;
                case 2:
                    Process.Start("rundll32.exe ", "powrprof.dll,SetSuspendState 0,1,0");
                    Application.Exit();
                    break;
                case 3:
                    Process.Start("shutdown", "/l /t 0 " + (Force ? "/f" : ""));
                    break;
            }
        }
        private void Main_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
                if (!toolTipShown)
                {
                    notifyIcon.ShowBalloonTip(1000);
                    toolTipShown = true;
                }
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TimerTo.Enabled)
                if (MessageBox.Show("You have a countdown set. Do you want to quit?", "Quit?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2) == DialogResult.No)
                    e.Cancel = true;
        }
    }
}
