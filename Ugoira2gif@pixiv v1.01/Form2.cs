using System;
using System.IO;
using System.Collections;
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

namespace Ugoira2gif_pixiv_v1._01
{
    public partial class Form2 : Form
    {

        private int percent = 100;


        volatile bool shouldRunning = true;
        volatile bool conti;

        FolderBrowserDialog fbd = new FolderBrowserDialog();
        public event EventHandler updateEvent;
        Thread clipBoardListener;
            
        private Form1 mainForm = null;


        public Form2()
        {
            InitializeComponent();
            textBox2.GotFocus += textBox2_GotFocus;
            textBox2.Leave += textBox2_Leave;
            textBox2.MouseUp += textBox2_MouseUp;
        }



        

        public Form2(Form callingF)
        {
            mainForm = callingF as Form1;
            InitializeComponent();
            fbd.Description = "Select a folder to save the gif";
            loadSetting();
           // getClipBoard();
            //getClipHelp();
            textBox2.Text = percent.ToString();
            textBox2.GotFocus += textBox2_GotFocus;
            textBox2.Leave += textBox2_Leave;
            textBox2.MouseUp += textBox2_MouseUp;


        }


        private void getClipHelp()
        {

            clipBoardListener = new Thread(getClipBoard);
            clipBoardListener.SetApartmentState(ApartmentState.STA);
            clipBoardListener.Start();
            //clipBoardListener.Join();
        
        }

        public void killlistening()
        {

            shouldRunning = false;
        
        }

        public void listentingStateChange(bool state)
        {

            conti = state;
        
        }

        

        private void  getClipBoard()
        {
            //return;
            string curURL = "";
            while(shouldRunning)
            {
                if (!conti)
                    continue;

                string clip = Clipboard.GetText(TextDataFormat.Text);

                if (clip == "")
                    continue;

                if (curURL != clip)
                {
                    string pixivURL = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=";
                    curURL = clip;
                    if (!curURL.Contains(pixivURL))
                    {

                        double num;
                        if (double.TryParse(curURL, out num))
                        {
                            //mainForm.Invoke(mainForm.delegateUpdateStatus, new Object[] { 1 + 100, 4, 1 });
                            //texturl.Text = clip;
                            if (!this.Visible)
                                mainForm.Invoke(mainForm.delegateShowF2, new Object[] { clip });

                        }
                        else
                        {



                        }

                    }
                    else
                    {

                        //texturl.Text = clip;
                        if (!this.Visible)
                            mainForm.Invoke(mainForm.delegateShowF2, new Object[] {  clip });

                    }
                }

            }
        }

        public void changeUrl(string updateurl)
        {

            texturl.Text = updateurl;
        
        }

        

        


        private void saveSetting()
        {

            try
            {
                File.Delete(Application.StartupPath + "\\" + "setting.sl");
            }
            catch (Exception)
            { }

            try
            {
                string data = this.textfolder.Text;
                System.IO.File.WriteAllText(Application.StartupPath + "\\" + "setting.sl", data);
                File.SetAttributes(Application.StartupPath + "\\" + "setting.sl", FileAttributes.Hidden);
            }
            catch (Exception)
            { MessageBox.Show("something wrong, I can't save the setting"); }
        
        }


        private void loadSetting()
        {

            try
            {
                using (StreamReader sr = new StreamReader(Application.StartupPath + "\\" + "setting.sl"))
                {
                    while (-1 < sr.Peek())
                    {
                        try
                        {
                            string data = sr.ReadLine();
                            this.textfolder.Text = data;

                        }
                        catch (Exception)
                        {

                        }
                    }
                    //sr.Close();
                }

            }
            catch (Exception)
            { 
            

            
            }
        
        }


        private void button1_Click(object sender, EventArgs e)
        {
            
            
            this.Hide();
            saveSetting();
            
            if (updateEvent != null)
            {
                //fire the event and give our custom event args some text
                updateEvent(sender, e);
            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void Start_Click(object sender, EventArgs e)
        {

            saveSetting();

            if (!Directory.Exists(textfolder.Text))
            {
                MessageBox.Show("folder not exist");
                this.Hide();
                //this.Dispose();
                if (updateEvent != null)
                {
                    //fire the event and give our custom event args some text
                    updateEvent(sender, e);
                }
                return;
            }


            this.mainForm.updateFromForm2(this.texturl.Text, this.textfolder.Text, hScrollBar1.Value, checkBox1.Checked, percent);
            


            mainForm.clicl_help();
            texturl.Text = "";
            if (updateEvent != null)
            {
                //fire the event and give our custom event args some text
                updateEvent(sender, e);
            }
            this.Hide();
        }

        private void SelFolder_Click(object sender, EventArgs e)
        {
            
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {

                textfolder.Text = fbd.SelectedPath;
                
            
            }
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            QualityText.Text = hScrollBar1.Value.ToString();
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        //    int num;
        //    if (int.TryParse(textBox2.Text, out num))
        //    {

        //        if (num > 500 || num < 1)
        //        {

        //            MessageBox.Show("must be in range 1~500");
        //            textBox2.Text = percent.ToString();
                
        //        }
        //        percent = num;
        //    }
        //    else
        //    {

        //        percent = 100;
        //        textBox2.Text = percent.ToString();
            
        //    }
        }

        bool textbox2focus = false;


        void textBox2_Leave(object sender, EventArgs e)
        {
            textbox2focus = false;
            int num;
            if (int.TryParse(textBox2.Text, out num))
            {

                if (num > 500 || num < 10)
                {

                    MessageBox.Show("must be in range 10~500");
                    textBox2.Text = percent.ToString();

                }
                percent = num;
            }
            else
            {

                percent = 100;
                textBox2.Text = percent.ToString();

            }
        }


        void textBox2_GotFocus(object sender, EventArgs e)
        {
            // Select all text only if the mouse isn't down.
            // This makes tabbing to the textbox give focus.
            if (MouseButtons == MouseButtons.None)
            {
                this.textBox2.SelectAll();
                textbox2focus = true;
            }
        }

        void textBox2_MouseUp(object sender, MouseEventArgs e)
        {
            // Web browsers like Google Chrome select the text on mouse up.
            // They only do it if the textbox isn't already focused,
            // and if the user hasn't selected all text.
            if (!textbox2focus && this.textBox1.SelectionLength == 0)
            {
                textbox2focus = true;
                this.textBox2.SelectAll();
            }
        }
        
    }

    












}
