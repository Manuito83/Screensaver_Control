using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;


namespace SS_Control
{
 
    public partial class Form1 : Form
    {
        bool auto_change = true;    // Were the dateTimePickers changed manually or automatically?
        bool active = false;        // Is the counter active?
        bool itsTime = false;       // Are we actually inside the defined time frame?
        int countdown = 0;

        // Initialize start and end times
        DateTime start_time = DateTime.Now;
        DateTime end_time = DateTime.Now;
                
        public Form1()
        {
            InitializeComponent();

            pictureBox1.Visible = false;
            label6.Text = DateTime.Now.ToShortTimeString();     // Label with current time

            // Checking whether no time has been saved (firt program execution)
            if (Properties.Settings.Default["ActivateTime"].ToString() != "")
            {
                // Return dateTimePicker1 to its saved value
                string recorded_activate = Properties.Settings.Default["ActivateTime"].ToString();
                dateTimePicker1.Value = Convert.ToDateTime(recorded_activate);

                start_time = dateTimePicker1.Value;     // Refresh start_time to do the calculations
            }
            else
            {
                // We save again in value text has not been changed
                Properties.Settings.Default["ActivateTime"] = dateTimePicker1.Text;
                Properties.Settings.Default.Save();

                start_time = dateTimePicker1.Value;
            }

            // Same as above with dateTimePicker2
            if (Properties.Settings.Default["DeactivateTime"].ToString() != "")
            {
                auto_change = true;

                string recorded_deactivate = Properties.Settings.Default["DeactivateTime"].ToString();
                dateTimePicker2.Value = Convert.ToDateTime(recorded_deactivate);

                end_time = dateTimePicker2.Value;
            }
            else
            {
                Properties.Settings.Default["DeactivateTime"] = dateTimePicker2.Text;
                Properties.Settings.Default.Save();

                end_time = dateTimePicker2.Value;
            }


            auto_change = false;

            timer1.Start();
        }


        //********************
        // Launch Screen Saver
        //********************
        [DllImport("User32.dll")]
        public static extern int PostMessage
            (IntPtr hWnd,
            uint Msg,
            uint wParam,
            uint lParam);

        public const uint WM_SYSCOMMAND = 0x112;
        public const uint SC_SCREENSAVE = 0xF140;

        public enum SpecialHandles
        {
            HWND_DESKTOP = 0x0,
            HWND_BROADCAST = 0xFFFF
        }

        public static void TurnOnScreenSaver()
        {
            PostMessage(
                new IntPtr((int)SpecialHandles.HWND_BROADCAST),
                WM_SYSCOMMAND,
                SC_SCREENSAVE,
                0);
        }
        //********************
        //********************
        //********************

        private void button1_Click(object sender, EventArgs e)
        {
            // Enable or disable "active"
            if (active == false)
            {
                // If we did NOT activate when inside the time frame, just activate
                if (itsTime == false)
                {
                    label2.Text = "ACTIVADO";
                    label2.ForeColor = Color.Green;
                    button1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    button1.Text = "DESACTIVAR";
                    active = true;
                }
                // But if we are inside the time frame, make a countdown!
                else
                {
                    button1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    button1.Text = "LAUNCH IN 4";
                    button1.ForeColor = Color.Green;
                    button1.Enabled = false;
                    countdown++;
                }
            }
            else
            {
                label2.Text = "DESACTIVADO";
                label2.ForeColor = Color.Red;
                button1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                button1.Text = "ACTIVAR";
                active = false;
                timer1.Interval = 1000;     // Back to normal timer interval
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool timeChecks = false;
            
            DateTime local_time = DateTime.Now;

            ScreenSaver.SetScreenSaverTimeout(900);   // Change screensaver timeout to 900 seconds

            label6.Text = local_time.ToShortTimeString();        // Updates the current time


            // Checks if time checks with adding 24 hours in case end_time goes beyond midnight
            if (end_time > start_time)
            {
                if ((local_time > start_time) && (local_time < end_time))
                {
                    timeChecks = true;
                }
            }
            else if ((local_time < start_time) && (local_time.AddDays(1) < end_time.AddDays(1)))
            {
                timeChecks = true;
            }
            else if ((local_time > start_time) && (local_time.AddDays(1) > end_time.AddDays(1)))
            {
                timeChecks = true;
            }
            

            // Then we proceed if time checks
            if (timeChecks)
            {
                itsTime = true;
                label6.ForeColor = Color.Green;
                if (active)
                {
                    // We only want to launch the screensaver once (if it isn't running yet!)
                    if (ScreenSaver.GetScreenSaverRunning() == false)
                    {
                        timer1.Interval = 300000;    // Give some time to move mouse and deactivate screen saver
                        TurnOnScreenSaver();        // Launch!
                        label2.Text = "ACTIVADO";
                        button1.Enabled = true;
                    }
                }
            }
            else
            {
                itsTime = false;
                label6.ForeColor = Color.Black;
                if (active)
                {
                    // Just try to kill if it's running, otherwise do nothing
                    if (ScreenSaver.GetScreenSaverRunning() == true)
                    {
                        ScreenSaver.KillScreenSaver();      // Deactivate screen saver
                        timer1.Interval = 1000;             // Back to normal timer interval
                        label2.Text = "DESACTIVADO";
                    }
                }
            }

            // Countdown in button
            if (countdown == 4)
            {
                button1.Text = "DESACTIVAR";
                button1.ForeColor = Color.Black;
                countdown = 0;
            }
            if (countdown == 3)
            {
                button1.Text = "LAUNCH IN 1";
                button1.ForeColor = Color.Green;
                countdown++;
                active = true;
            }
            if (countdown == 2)
            {
                button1.Text = "LAUNCH IN 2";
                button1.ForeColor = Color.Green;
                countdown++;
            }
            if (countdown == 1)
            {
                button1.Text = "LAUNCH IN 3";
                button1.ForeColor = Color.Green;
                countdown++;
            }
        
            if (itsTime && active)
            {
                pictureBox1.Visible = true;
            }
            else
            {
                pictureBox1.Visible = false;
            }
        }


        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // If the change is performed manually we do not want to call this function
            if (auto_change == false)
            {
                // Save value in dateTimePicker1
                Properties.Settings.Default["ActivateTime"] = dateTimePicker1.Text;
                Properties.Settings.Default.Save();
               
                start_time = dateTimePicker1.Value;      // Also update start_time with the new value
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (auto_change == false)
            {
                // Save value in dateTimePicker2
                Properties.Settings.Default["DeactivateTime"] = dateTimePicker2.Text;
                Properties.Settings.Default.Save();

                end_time = dateTimePicker2.Value;       // Also update end_time with the new value
            }
        }
    }
}
