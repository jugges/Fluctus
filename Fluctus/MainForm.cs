﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using Fluctus.Properties;
using System.Resources;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Media;
using System.Collections.Generic;

namespace Fluctus
{
    public partial class MainForm : Form
    {
        Stopwatch timer = new Stopwatch();
        Stopwatch break_timer = new Stopwatch();
        TimeSpan onemin = new TimeSpan(0, 1, 0);
        TimeSpan tenmin = new TimeSpan(0, 10, 0);
        TimeSpan changeme = new TimeSpan(9, 9, 9);
        Font regtime;
        Stream str;
        SoundPlayer snd;
        string m30;
        string m1;
        string displayme;
        public static bool settings_Alert = true;
        public static bool gamemode = Settings.Default.gamemode;
        //public static bool powersaving = Settings.Default.savepower;
        bool in_Break = false;
        bool break_Type;
        bool trueafk = false;
        public static ResourceManager res_man = new ResourceManager("Fluctus.Lang.lang", Assembly.Load("Fluctus"));
        public static CultureInfo cul;
        public MainForm()
        {
            InitializeComponent();
            regtime = time_lbl.Font;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                //Hide();
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            PlaceLowerRight();
            cul = new CultureInfo(Settings.Default.lang);
            lang_Refresh();
            timer.Start();
            break_Refresh();
            sep_lbl.AutoSize = false;
            sep_lbl.Height = 2;
            sep_lbl.BorderStyle = BorderStyle.Fixed3D;
        }

        public void lang_Refresh()
        {
            label1.Text = res_man.GetString("LabelText", cul);
            m30 = res_man.GetString("Message30", cul);
            m1 = res_man.GetString("Message1", cul);
            finish_btn.Text = res_man.GetString("ButtonText", cul);
            skip_btn.Text = res_man.GetString("skipBtn", cul);
            openToolStripMenuItem.Text = res_man.GetString("contextOpen", cul);
            aboutToolStripMenuItem.Text = res_man.GetString("contextAbout", cul);
            settingsToolStripMenuItem.Text = res_man.GetString("contextSettings", cul);
            exitToolStripMenuItem.Text = res_man.GetString("contextExit", cul);
            if (Settings.Default.Sound == "relax")
            {
                str = Properties.Resources.relaxing;
                snd = new System.Media.SoundPlayer(str);
            }
            if (Settings.Default.gamemode&&!gamemodechecker.Enabled)
            {
                gamemodechecker.Start();
            }
            else if(!Settings.Default.gamemode && gamemodechecker.Enabled)
            {
                gamemodechecker.Stop();
                if(!trueafk)
                aFKToolStripMenuItem.Checked = false;

            }
            //forceon_Top = Settings.Default.forceontop;
            //forcecenter = Settings.Default.forcecenter;
            //gamemode = Settings.Default.gamemode;
            //powersaving = Settings.Default.savepower;
            //note30 = Settings.Default.note30;
            //note2 = Settings.Default.note2;
        }

        private void break_Refresh()
        {
            Size startsize;
            Font timesize;
            Point timeplace;
            if (!in_Break)
            {
                startsize = new Size(459, 140);
                timesize = regtime;
                timeplace = new Point(184, 24);
            }
            else
            {
                startsize = new Size(459, 242);
                timesize = label3.Font;
                timeplace = new Point(277, -2);
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                //this.Show();
            }
            skip_btn.Enabled = true;
            finish_btn.Enabled = false;
            this.Size = startsize;
            this.MaximumSize = startsize;
            this.MinimumSize = startsize;
            time_lbl.Visible = true;
            sep_lbl.Visible = in_Break;
            label2.Text = displayme;
            label2.Visible = in_Break;
            this.ControlBox = !in_Break;
            time_lbl.Font = timesize;
            time_lbl.Location = timeplace;
            breaktime_lbl.Visible = in_Break;
            progressBar1.Visible = in_Break;
            finish_btn.Visible = in_Break;
            skip_btn.Visible = in_Break;
            alarm_img.Visible = in_Break;
        }

        private void PlaceLowerRight()
        {
            //Determine "rightmost" screen
            Screen rightmost = Screen.AllScreens[0];
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                    rightmost = screen;
            }

            this.Left = rightmost.WorkingArea.Right - this.Width;
            this.Top = rightmost.WorkingArea.Bottom - this.Height;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                PlaceLowerRight();
                //this.Show();
            }
            else
            {
                //this.Hide();
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private bool checkprocess(int place, bool prev)
        {
            if (place >= Settings.Default.processlist.Count)
            {
                return prev;
            }
            Process[] pname = Process.GetProcessesByName(Settings.Default.processlist.ToArray().GetValue(place).ToString());
            if (pname.Length != 0)
            {
                return true;
            }
            place++;
            return checkprocess(place, prev | false);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //if (this.Visible == true)
            //if (this.WindowState == FormWindowState.Normal)
            //{

            time_lbl.Text = (timer.Elapsed.ToString("hh\\:mm"));
            //}
            if (timer.Elapsed.Minutes == 30 && progressBar1.Value == 0)
            {
                func30();
            }
            if (timer.Elapsed.Hours != 0 && timer.Elapsed.Hours % 2 != 0 && timer.Elapsed.Minutes == 0 && progressBar1.Value == 0)
            {
                func30();
            }
            void func30()
            {
                in_Break = true;
                break_Type = true;
                progressBar1.Maximum = 600;
                displayme = m30;
                break_Refresh();
                timer.Stop();
                break_timer.Start();
                prog.Start();
                adder.Start();
                //this.Show();
                //skip_btn.Enabled = true;
                //finish_btn.Enabled = false;
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.CenterToScreen();
                if (Settings.Default.Sound == "relax")
                {
                    snd.Play();
                }
                progressBar1.Value += 10;
                if (Settings.Default.note30 && Settings.Default.note30msg != "")
                {
                    MessageBox.Show(Settings.Default.note30msg);
                }
            }

            if (timer.Elapsed.Hours != 0 && timer.Elapsed.Hours % 2 == 0 && timer.Elapsed.Minutes == 0 && progressBar1.Value == 0)
            {
                in_Break = true;
                break_Type = false;
                progressBar1.Maximum = 600;
                displayme = m1;
                break_Refresh();
                timer.Stop();
                break_timer.Start();
                prog.Start();
                adder.Start();
                //this.Show();
                //skip_btn.Enabled = true;
                //finish_btn.Enabled = false;
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.CenterToScreen();
                if (Settings.Default.Sound == "relax")
                {
                    snd.Play();
                }
                progressBar1.Value += 1;
                if (Settings.Default.note2 && Settings.Default.note2msg != "")
                {
                    MessageBox.Show(Settings.Default.note2msg);
                }
            }
            //if (changeme.Minutes == 0 && changeme.Seconds == 0)
            //{
            if (in_Break)
            {
                if (progressBar1.Value == 600)
                {
                    skip_btn.Enabled = false;
                    finish_btn.Enabled = true;
                    break_timer.Stop();
                }

            }
            //}
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this.Visible = true;

            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            PlaceLowerRight();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm settings = new SettingsForm();
            settings.yourAction = lang_Refresh;
            settings.Visible = true;
        }

        private void finish_btn_Click(object sender, EventArgs e)
        {
            if (progressBar1.Value == 600)
            {
                in_Break = false;
                break_Refresh();
                timer.Start();
                break_timer.Stop();
                break_timer.Reset();
                reverseprog.Start();
            }
        }

        private void prog_Tick(object sender, EventArgs e)
        {
            //if (progressBar1.Value != 600)
            if (progressBar1.Value < 600)
            {
                if (break_Type)
                {
                    progressBar1.Value += 10;

                }
                else
                {
                    progressBar1.Value += 1;
                }
            }
            else
            {
                prog.Stop();
            }
        }

        private void reverseprog_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Value > 0)
            {
                //if (progressBar1.Value != 0)
                progressBar1.Value -= 1;
            }
            else
            {
                reverseprog.Stop();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.Show();
        }

        private void sep_lbl_Click(object sender, EventArgs e)
        {

        }

        private void adder_Tick(object sender, EventArgs e)
        {
            breaktime_lbl.Text = (changeme.ToString("mm\\:ss"));
            if (Settings.Default.forceontop)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
            if (Settings.Default.forcecenter)
            {
                this.CenterToScreen();
            }
            if (break_Type)
            {
                //if (break_timer.Elapsed.Minutes >= 1)
                //{
                //    adder.Stop();
                //}
                //else
                //{
                changeme = onemin - break_timer.Elapsed;
                //}
            }
            else
            {
                //if (break_timer.Elapsed.Minutes >= 10)
                //{
                //    adder.Stop();
                //}
                //else
                //{
                changeme = tenmin - break_timer.Elapsed;
                //}
            }
            if (!break_timer.IsRunning)
            {
                adder.Stop();
            }
        }

        //private void aFKToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (aFKToolStripMenuItem.Checked)
        //    {
        //        aFKToolStripMenuItem.BackColor = Color.Lime;
        //        aFKToolStripMenuItem.Image = Fluctus.Properties.Resources.on;
        //        timer.Stop();
        //        counter.Stop();
        //    }
        //    else
        //    {
        //        aFKToolStripMenuItem.BackColor = Color.Red;
        //        aFKToolStripMenuItem.Image = Fluctus.Properties.Resources.off;
        //        timer.Start();
        //        counter.Start();
        //    }
        //}

        private void skip_btn_Click(object sender, EventArgs e)
        {
            //skipped = true;
            in_Break = false;
            break_Refresh();
            timer.Start();
            break_timer.Stop();
            break_timer.Reset();
            prog.Stop();
            progressBar1.Value = 600;
            reverseprog.Start();
        }

        private void aFKToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (aFKToolStripMenuItem.Checked)
            {
                aFKToolStripMenuItem.BackColor = Color.Lime;
                aFKToolStripMenuItem.Image = Fluctus.Properties.Resources.on;
                timer.Stop();
                counter.Stop();
            }
            else
            {
                aFKToolStripMenuItem.BackColor = SystemColors.Control;
                aFKToolStripMenuItem.Image = Fluctus.Properties.Resources.off;
                timer.Start();
                counter.Start();
            }
        }

        private void gamemodechecker_Tick(object sender, EventArgs e)
        {
            if(checkprocess(0, false) && !aFKToolStripMenuItem.Checked)
            {
                aFKToolStripMenuItem.Checked = true;
            }
            else if(!checkprocess(0, false) && aFKToolStripMenuItem.Checked && !trueafk)
            {
                aFKToolStripMenuItem.Checked = false;
            }
            //aFKToolStripMenuItem.Checked = checkprocess(0, false);
        }

        private void aFKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trueafk = !trueafk;
        }
    }
}
