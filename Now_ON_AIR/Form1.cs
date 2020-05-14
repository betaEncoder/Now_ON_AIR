using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace Now_ON_AIR
{
    public partial class Form1 : Form
    {
        bool on_air = false;
        Icon icon_OA;
        Icon icon_nOA;

        public Form1()
        {
            InitializeComponent();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            bool current_state = false;
            // triggered 1sec period

            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();    // get all of processes
            foreach (System.Diagnostics.Process p in ps)
            {
                try
                {
                    if (p.ProcessName.Contains("CptHost"))
                    {
                        Console.WriteLine("ID:{0}", p.Id);
                        current_state = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("エラー: {0}", ex.Message);
                }
            }
            if (current_state == true)
            {
                this.label_status.Text = "Status: ON AIR";
            }
            else
            {
                this.label_status.Text = "Status: OFF_AIR";
            }

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                /** POSTするデータなし **/
            });
            string ifttt_URL = "https://maker.ifttt.com/trigger/";
            string key = this.textBox_key.Text;
            string ON_event = this.textBox_on_event.Text;
            string OFF_event = this.textBox_off_event.Text;
            if (key == "" || ON_event == "" || OFF_event == "")
            {
                return;
            }
            if (on_air == false && current_state == true)
            {
                // turn on trigger
                client.PostAsync(ifttt_URL + ON_event + "/with/key/" + key, content);
                this.Icon = icon_OA;
                notifyIcon1.Icon = icon_OA;
            }
            else if (on_air == true && current_state == false)
            {
                // turn off trigger
                client.PostAsync(ifttt_URL + OFF_event + "/with/key/" + key, content);
                this.Icon = icon_nOA;
                notifyIcon1.Icon = icon_nOA;
            }
            on_air = current_state;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.saved_key = this.textBox_key.Text;
            Properties.Settings.Default.saved_on_event = this.textBox_on_event.Text;
            Properties.Settings.Default.saved_off_event = this.textBox_off_event.Text;
            Properties.Settings.Default.Save();
            notifyIcon1.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_key.Text = Properties.Settings.Default.saved_key;
            textBox_on_event.Text = Properties.Settings.Default.saved_on_event;
            textBox_off_event.Text = Properties.Settings.Default.saved_off_event;
            //現在のコードを実行しているAssemblyを取得
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            icon_OA = new Icon(myAssembly.GetManifestResourceStream("Now_ON_AIR.Resources.OA.ico"));
            icon_nOA = new Icon(myAssembly.GetManifestResourceStream("Now_ON_AIR.Resources.nOA.ico"));
        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
            {                // フォームが最小化の状態であればフォームを非表示にする
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            this.Activate();
        }

    }
}