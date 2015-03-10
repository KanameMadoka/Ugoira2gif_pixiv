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

using System.Text.RegularExpressions;

using Ionic.Zlib;
using Ionic.Zip;

using System.Net;
using System.IO;


using Gif.Components;
using System.Runtime.InteropServices;





namespace Ugoira2gif_pixiv_v1._01
{

    public partial class Form1 : Form
    {
        
        public delegate int updateBar(int value, int id, int pos, string name, double size, string status, string fileLocation);
        public delegate void delreleaseSlot(int ind);
        public delegate void updateStatus(int pos, int Num, string s);
        public delegate void showF2(string updateurl);

        private static Mutex mut;
        public static volatile int totalNumThread;
       
        public static int[] available;
        int MAXSIZE;
        List<ProgressBar> bar = new List<ProgressBar>();

        private int form2quality;
        private int form2size;
        private string form2url;
        private string form2folder;
        private bool form2keep;

        private Form2 frm = null;

        

        
        public updateBar delegateUpdateBar;
        public delreleaseSlot delegateReleaseSlot;
        public updateStatus delegateUpdateStatus;
        public showF2 delegateShowF2;
        

        private int NextPos;



        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        

        IntPtr nextClipboardViewer;






        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            if (checkBox1.Checked)
            {
                switch (m.Msg)
                {
                    case WM_DRAWCLIPBOARD:
                        DisplayClipboardData();
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                        break;

                    case WM_CHANGECBCHAIN:
                        if (m.WParam == nextClipboardViewer)
                            nextClipboardViewer = m.LParam;
                        else
                            SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                        break;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }


        }

        //int count = 0;
        string curURL = "";
        private void DisplayClipboardData()
        {
            //if (count == 0)
            //{
            //    count++;
            //    return;
            //}
            if (frm == null)
            {
                frm = new Form2(this);
                frm.updateEvent += new EventHandler(handleUpdateEvent);

                //Register form closed event
                frm.FormClosed += new FormClosedEventHandler(form2_FormClosed);
            }
            
            string clip = Clipboard.GetText(TextDataFormat.Text);

            bool visible = this.Visible;

            if (curURL != clip)
            {
                string pixivURL = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=";
                curURL = clip;
                if (!curURL.Contains(pixivURL))
                {

                    double num;
                    if (double.TryParse(curURL, out num))
                    {
                        frm.changeUrl(curURL);
                        //texturl.Text = clip;
                        
                            
                            frm.Show();
                        


                    }
                    else
                    {



                    }

                }
                else
                {
                    frm.changeUrl(curURL);
                    //texturl.Text = clip;
                    
                        frm.Show();

                }
            }
            
        }









        protected override void OnClosed(EventArgs e)
        {
            listV2file();
            frm.killlistening();
            base.OnClosed(e);
        }

        

        public Form1()
        {
            InitializeComponent();
            mut = new Mutex();
            
            available = new int [10] {0,0,0,0,0,0,0,0,0,0};
            MAXSIZE = 10;
            bar.Add(progressBar1);
            bar.Add(progressBar2);
            bar.Add(progressBar3);
            bar.Add(progressBar4);
            bar.Add(progressBar5);
            bar.Add(progressBar6);
            bar.Add(progressBar7);
            bar.Add(progressBar8);
            bar.Add(progressBar9);
            bar.Add(progressBar10);

            foreach (ProgressBar myBar in bar)
            {
                myBar.Visible = false;
                
            }

            delegateUpdateBar = new updateBar(methodUpdateBar);
            delegateReleaseSlot = new delreleaseSlot(releaseSlot);
            delegateUpdateStatus = new updateStatus(myupdateStatus);
            delegateShowF2 = new showF2(showForm2);
            NextPos = -1;
            file2listV();
            listeningClip();
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
            frm = new Form2(this);
            frm.updateEvent += new EventHandler(handleUpdateEvent);

            //Register form closed event
            frm.FormClosed += new FormClosedEventHandler(form2_FormClosed);
            frm.listentingStateChange(checkBox1.Checked);

        }

        public void showForm2(string updateurl)
        {

            frm.changeUrl(updateurl);
            frm.Show();
        
        }

        public void updateFromForm2(string url, string folder, int quality, bool Keep, int Size)
        {

            form2folder = folder;
            form2url = url;
            form2quality = quality;
            form2keep = Keep;
            form2size = Size;
        
        }

        private void listeningClip()
        {

            //Thread listeningThread = new Thread(new ThreadStart(listeningClip_helper));
            //listeningThread.Start();

        
        }

        private async void listeningClip_helper()
        {

            
            await Task.Delay(100);


            if (frm == null)
            {
                frm = new Form2(this);
                frm.updateEvent += new EventHandler(handleUpdateEvent);

                //Register form closed event
                frm.FormClosed += new FormClosedEventHandler(form2_FormClosed);
            }


            
            
            for (; ; )
            {
                

                if (!frm.Visible)
                {
                    string clip = Clipboard.GetText(TextDataFormat.Text);


                    string pixivURL = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=";
                    string curURL = clip;
                    if (!curURL.Contains(pixivURL))
                    {

                        double num;
                        if (double.TryParse(curURL, out num))
                        {

                            if (frm == null)
                            {

                                frm = new Form2(this);

                            }

                            frm.updateEvent += new EventHandler(handleUpdateEvent);

                            //Register form closed event
                            frm.FormClosed += new FormClosedEventHandler(form2_FormClosed);
                            frm.Show();

                        }
                        else
                        {



                        }

                    }
                    else
                    {

                        if (frm == null)
                        {

                            frm = new Form2(this);

                        }

                        frm.updateEvent += new EventHandler(handleUpdateEvent);

                        //Register form closed event
                        frm.FormClosed += new FormClosedEventHandler(form2_FormClosed);
                        frm.Show();

                    }
            
            
                }
            }
        
        
        }

        private void myupdateStatus(int pos, int num, string s)
        {
            
            if (pos < 1)
                return;

            mut.WaitOne();
            if (pos <= 10)
            {
                if (num == 0)
                    listView1.Items[pos - 1].Text = s;
                else 
                    listView1.Items[pos - 1].SubItems[num-1].Text = s;

                mut.ReleaseMutex();
                return;
            }
            if (pos > 100)
            {
                if (num == 0)
                    listView2.Items[pos - 101].Text = s;
                else
                    listView2.Items[pos - 101].SubItems[num - 1].Text = s;
            
            }
            mut.ReleaseMutex();
        
        }

        private void updateNextPos()
        {

            NextPos++;
            if (NextPos > totalNumThread)
                NextPos = -1;
        
        }

        private int methodUpdateBar(int value, int id, int pos, string name, double size, string status, string fileLocation)
        {
            mut.WaitOne();
            if (NextPos != pos)
            {

                if (value != 100)
                {
                    if (bar[pos-1].Visible == false)
                    {

                        bar[pos-1].Visible = true;
                        ListViewItem lvt = new ListViewItem(name);
                        //lvt.SubItems.Add(name);
                        size = Math.Round(size, 2);
                        lvt.SubItems.Add(String.Format("{0:0.00}", size.ToString()) + "Mb");
                        lvt.SubItems.Add("");
                        lvt.SubItems.Add(status);
                        listView1.Items.Add(lvt);

                    }


                    bar[pos-1].Value = value;




                }
                else
                {

                    ListViewItem lvt = new ListViewItem(name);
                    //lvt.SubItems.Add(name);
                    size = Math.Round(size, 2);
                    lvt.SubItems.Add(String.Format("{0:0.00}", size.ToString()) + "Mb");
                    lvt.SubItems.Add(status);
                    lvt.SubItems.Add("Please Wait");
                    lvt.SubItems.Add(fileLocation);
                    listView1.Items.RemoveAt(pos-1);
                    bar[pos-1].Visible = false;
                    listView2.Items.Add(lvt);
                    
                    
                        mut.ReleaseMutex();
                        for (; ; )
                        {
                            mut.WaitOne();
                            if (NextPos == -1)
                            {
                                NextPos = pos+1;

                                if (NextPos > totalNumThread)
                                    NextPos = -1;

                                mut.ReleaseMutex();
                                return listView2.Items.Count;
                            }
                            mut.ReleaseMutex();
                        }
                    
                    
                    
                
                }

                
            
            }
            else
            {
                if (value != 100)
                {
                    
                    pos--;
                    
                    bar[NextPos-1].Visible = false;
                    bar[pos-1].Value = value;
                    bar[pos-1].Visible = true;
                    updateNextPos();
                }
                else
                {

                    ListViewItem lvt = new ListViewItem();
                    lvt.SubItems.Add(name);
                    size = Math.Round(size, 2);
                    lvt.SubItems.Add(String.Format("{0:0.00}", size.ToString()) + "Mb");
                    lvt.SubItems.Add("");
                    lvt.SubItems.Add(status);
                   
                    pos--;
                    listView2.Items.Add(lvt);
                    bar[pos].Visible = false;
                    bar[pos-1].Value = value;
                    bar[pos-1].Visible = true;
                    updateNextPos();
                    

                    
                        mut.ReleaseMutex();
                        for (; ; )
                        {
                            mut.WaitOne();
                            if (NextPos == -1)
                            {
                                NextPos = pos+1;

                                if (NextPos > totalNumThread)
                                    NextPos = -1;

                                mut.ReleaseMutex();
                                return listView2.Items.Count;
                            }
                            mut.ReleaseMutex();
                        }


                    
                    
                
                }

            }


            mut.ReleaseMutex();
            return pos;

        }

        /*getSlot()
         * Return Value: index of the slot, -1 indicate full
         * Other: aquire the slot when available, increase totalNumThread
         */
        private int getSlot()
        {

            for (int i = 0; i < MAXSIZE; i++)
            {

                if (available[i] == 0)
                {

                    available[i] = 1;
                    totalNumThread++;
                    return i;
                
                }
            
            }
            return -1;
        
        
        }


        /*releaseSlot()
         * Return Value: none
         * Other: release the slot, decrease totalNumThread
         */
        private void releaseSlot(int ind)
        {

            if (ind < 0 || ind > 9)
                MessageBox.Show("releaseSlot error");
            mut.WaitOne();
            available[ind] = 0;
            totalNumThread--;
            mut.ReleaseMutex();
        
        }

        private void listV2file()
        {


            String st = "";

            if (listView2.Items.Count > 0)
            {
                // the actual data
                foreach (ListViewItem lvi in listView2.Items)
                {


                    foreach (ListViewItem.ListViewSubItem listViewSubItem in lvi.SubItems)
                    {
                        st += (listViewSubItem.Text + "\n");
                    }

                }


                try
                {
                    File.Delete(Application.StartupPath + "\\" + "data.sl");
                }
                catch (Exception)
                { }



                try
                {

                    System.IO.File.WriteAllText(Application.StartupPath + "\\" + "data.sl", st);
                    File.SetAttributes(Application.StartupPath + "\\" + "data.sl", FileAttributes.Hidden);
                }
                catch { }
            }
            else
            {
                try
                {
                    File.Delete(Application.StartupPath + "\\" + "data.sl");
                }
                catch (Exception)
                { }
            
            }

        }

        private void file2listV()
        {
            try
            {
                using (StreamReader sr = new StreamReader(Application.StartupPath + "\\" + "data.sl"))
                {
                    while (-1 < sr.Peek())
                    {
                        try
                        {
                            string name = sr.ReadLine();
                            string size = sr.ReadLine();
                            string status = sr.ReadLine();
                            string completeTime = sr.ReadLine();
                            string fileLocation = sr.ReadLine();
                            var lvi = new ListViewItem(name);
                            lvi.SubItems.Add(size);
                            lvi.SubItems.Add(status);
                            lvi.SubItems.Add(completeTime);
                            lvi.SubItems.Add(fileLocation);

                            listView2.Items.Add(lvi);
                        }
                        catch (Exception)
                        {
                            listView2.Items.RemoveAt(listView2.Items.Count);
                        }
                    }
                    //sr.Close();
                }

            }
            catch (Exception)
            { }
        
        }



        private void button1_Click(object sender, EventArgs e)
        {

            

            
            //myThread.Join();
            
            //new Thread(()=>newForm.threadWait(url, ref totalNumThread, ref available)).Start();

            //clicl_help();
            //Form2 frm = new Form2(this);
            //frm.Show();

            if (frm == null)
            {

                frm = new Form2(this);
            
            }

            frm.updateEvent += new EventHandler(handleUpdateEvent);

            //Register form closed event
            frm.FormClosed += new FormClosedEventHandler(form2_FormClosed);
            frm.Show();



        
        
        }

        void handleUpdateEvent(object sender, EventArgs e)
        {
            this.Activate();
        }

        void form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Visible = true;
        }

        public void clicl_help()
        {

            //string url = textBox1.Text;
            string url = form2url;

            Thread myThread = new Thread(new ThreadStart(() => threadWait(url)));
            myThread.Start();
        
        }


        private void threadWait(string url)
        {
            
            for (; ; )
            {
                mut.WaitOne();
                //total++;
                //int ind = getSlot();
                //int ind = (int)this.Invoke(this.delegateGetSlot, new object [] {});
                int ind = -1;



                for (int i = 0; i < MAXSIZE; i++)
                {

                    if (available[i] == 0)
                    {

                        available[i] = 1;
                        totalNumThread++;
                        ind = i;
                        break;
                    }

                }
                







                if (ind == -1)
                {

                    mut.ReleaseMutex();
                    continue;
                
                }
                if (totalNumThread <= 4)
                {
                    

                    ThreadWorker c = new ThreadWorker(this);
                    int temp = totalNumThread;
                    //Thread othread = new Thread(() => c.GetWebContent(url, ind, temp, form2folder, form2keep, form2quality));
                    //methodUpdateBar(0, ind, totalNumThread, "Unkown", 0, "Analyzing", "fileLocation");
                    //othread.Start();
                    ThreadWorker myThreadWorker = new ThreadWorker(this);
                    mut.ReleaseMutex();
                    myThreadWorker.GetWebContent(url, ind, temp, form2folder, form2keep, form2quality, form2size);
                    
                    //othread.Join();
                    //releaseSlot(ind);
                    
                    



                    return;
                }
                else {
                    int pos = totalNumThread - 1;
                    mut.ReleaseMutex();
                    for (; ; )
                    {

                        mut.WaitOne();
                        if (pos <= 4)
                        {
                            
                            //ThreadWorker c = new ThreadWorker(this);
                            //Thread othread = new Thread(() => c.GetWebContent(url, ind, pos, form2folder, form2keep, form2quality));

                            ThreadWorker myThreadWorker = new ThreadWorker(this);
                            mut.ReleaseMutex();
                            myThreadWorker.GetWebContent(url, ind, pos, form2folder, form2keep, form2quality, form2size);


                            //othread.Start();
                            mut.ReleaseMutex();
                            //pos = methodUpdateBar(0, ind, totalNumThread, "Unkown", 0, "Analyzing", "fileLocation");
                            //othread.Join();
                            //releaseSlot(ind);
                            
                            
                            return;
                        
                        
                        }
                        mut.ReleaseMutex();
                        pos = methodUpdateBar(0, ind, totalNumThread, "Unkown", 0, "Analyzing", "fileLocation");
 
                    
                    }
                
                }
            
            
            
            }
        
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (listView2.SelectedItems[0].SubItems[2].Text == "Ready For View")
            //{

            //    MessageBox.Show("correct");
            
            //}
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count != 0)
            {
                try
                {
                    string folder = listView2.SelectedItems[0].SubItems[4].Text;
                    string name = listView2.SelectedItems[0].SubItems[0].Text;
                    string total = folder + "\\" + name + ".html";
                    System.Diagnostics.Process.Start(total);
                }
                catch (Exception)
                {
                    MessageBox.Show("Does not exist");
                    listView2.Items.RemoveAt(listView2.SelectedItems[0].Index);
                
                }
            
            }
        }

        private void openFileFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count != 0)
            {
                try
                {
                    string folder = listView2.SelectedItems[0].SubItems[4].Text;
                    string name = listView2.SelectedItems[0].SubItems[0].Text;
                    string total = folder + "\\" + name + ".html";
                    System.Diagnostics.Process.Start(folder);
                }
                catch (Exception)
                {
                    MessageBox.Show("Does not exist");
                    listView2.Items.RemoveAt(listView2.SelectedItems[0].Index);

                }

            }
        }

        private void deleteFileFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count != 0)
            {
                try
                {
                    string folder = listView2.SelectedItems[0].SubItems[4].Text;
                    Directory.Delete(folder, true);
                    listView2.Items.RemoveAt(listView2.SelectedItems[0].Index);
                }
                catch (Exception)
                {
                    MessageBox.Show("Does not exist");
                    listView2.Items.RemoveAt(listView2.SelectedItems[0].Index);

                }
            }
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Ugoira2gif_pixiv_v1._04.Properties.Resources.String2);
        }
            

        private void deleteEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count != 0)
            {
                listView2.Items.RemoveAt(listView2.SelectedItems[0].Index);
            }
            //listView2.Items.RemoveAt(listView2.SelectedItems[0].Index);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void listView1_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {

            e.Cancel = true;

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            frm.listentingStateChange(checkBox1.Checked);
        }

        













    }


    public class ThreadWorker
    {



        















        Form1 myform;
        HttpWebRequest req;
        IAsyncResult _responseAsyncResult;
        List<int> delay = new List<int>();
        string fileName = "\\temp.gif";
        string downloadURL = "";
        string title = "";
        int tid;
        int position;
        string folder;
        bool keep;
        int quality;
        int resize;

        public ThreadWorker(Form1 temp)
        {

            myform = temp;
        
        }

        public void GetWebContent(string url, int ind, int pos, string folder1, bool keep1, int quality1, int form2size)
        {

            folder = folder1;
            keep = keep1;
            quality = quality1;
            tid = ind;
            position = pos;
            resize = form2size;
            if (!analyze(url))
                return;

            


            req = WebRequest.Create(downloadURL) as HttpWebRequest;

            req.Method = "GET";
            req.Headers.Add("Origin", "http://www.pixiv.net");
            req.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");

            req.Host = "i2.pixiv.net";
            req.Headers.Add("Accept-Language", "en-US,en;q=0.8,ja;q=0.6,zh-CN;q=0.4");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
            req.AddRange(0);
            req.AddRange(299999);
            req.Accept = "*/*";
            req.Referer = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=44310909";

            req.Headers.Add("Cache-Control", "max-age=0");
            
            _responseAsyncResult = req.BeginGetResponse(ResponseCallback, null);
        }


        private bool analyze(string url)
        {
            //myform.Invoke(myform.delegateUpdateStatus, new Object[] { position, 3, "Analyzing" });
            delay.Clear();
            downloadURL = "";
            string pixivURL = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=";
            string curURL = url;
            if (!curURL.Contains(pixivURL))
            {

                double num;
                if (double.TryParse(curURL, out num))
                {
                    title = curURL;
                    curURL = pixivURL + curURL;

                }
                else
                {

                    MessageBox.Show("Input illegal");

                    return false;

                }

            }
            else
            {
                MatchCollection m3 = Regex.Matches(url, @"illust_id=(.+?)$", RegexOptions.Singleline);
                title = m3[0].Groups[1].Value;
            
            }
            UpdateProgressBar(0, 0);
            try
            {
                WebClient wc = new WebClient();
                String myhtml = wc.DownloadString(curURL);
                MatchCollection m1 = Regex.Matches(myhtml, @"src"":""(.+?)600x600.zip""", RegexOptions.Singleline);
                Match mymatch;
                if (m1.Count == 0)
                {

                    MessageBox.Show("ugoira not find, please check your input");
                    downloadURL = "";


                    return false;

                }
                else if (m1.Count == 1)
                    mymatch = m1[0];
                else
                    mymatch = m1[1];

                string tempurl = mymatch.Groups[1].Value;
                string replacedurl = tempurl.Replace("\\", "");
                replacedurl = replacedurl.Replace(@"src"":""", "");
                //MessageBox.Show(replacedurl);
                downloadURL = replacedurl + "1920x1080.zip";

                MatchCollection m2 = Regex.Matches(myhtml, @"delay"":(.+?)}", RegexOptions.Singleline);
                //MessageBox.Show(m2.Count.ToString());

                foreach (Match m in m2)
                {
                    string temp = m.Groups[1].Value;
                    delay.Add(Convert.ToInt32(temp));

                }

                
                myform.Invoke(myform.delegateUpdateStatus, new Object[] { position, 0, title });

                fileName = "\\"+title+".gif";
                //MessageBox.Show("ugoira find, ready for generate");

                return true;

            }
            catch (Exception exceptoion)
            {

                MessageBox.Show(exceptoion.Data.ToString());
                
                return false;

            }
        
        
        }

        private void cleanUp(string tempfolder, string tempgif)
        {

            File.Copy(tempgif, folder + "\\" + title + ".gif", true);
            long length = new System.IO.FileInfo(folder + "\\" + title + ".gif").Length;
            string time = new System.IO.FileInfo(folder + "\\" + title + ".gif").CreationTime.ToString();
            double size = (double)length / 1024 / 1024;
            size = Math.Round(size, 2);


            
            myform.Invoke(myform.delegateUpdateStatus, new Object[] { position + 100, 2, String.Format("{0:0.00}", size.ToString()) + "Mb" });
            myform.Invoke(myform.delegateUpdateStatus, new Object[] { position + 100, 4, time });
            if (keep)
            {

                foreach (string newPath in Directory.GetFiles(tempfolder, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(tempfolder, folder+"\\"), true);
            
            }
            try
            {
                Directory.Delete(tempfolder, true);
                //File.Delete(tempgif);
            }
            catch (Exception)
            { 
            
            }
            try
            {
                //Directory.Delete(tempfolder, true);
                File.Delete(tempgif);
            }
            catch (Exception)
            {

            }
        }

        private void ResponseCallback(object state)
        {
            var response = req.EndGetResponse(_responseAsyncResult) as HttpWebResponse;
            long contentLength = response.ContentLength;
            if (contentLength == -1)
            {
                // You'll have to figure this one out.
            }
            Stream responseStream = response.GetResponseStream();
            Stream sm = new MemoryStream (GetContentWithProgressReporting(responseStream, contentLength));
            response.Close();
            string outfolder = Application.StartupPath;
            
            Directory.CreateDirectory(outfolder + "//"+title);
            var fileStream = File.Create(outfolder + "//"+title+"//temp.zip");
            sm.CopyTo(fileStream);
            fileStream.Close();
            sm.Close();


            myform.Invoke(myform.delegateUpdateStatus, new Object[] { position + 100 , 3, "Encoding" });
            myform.Invoke(myform.delegateUpdateStatus, new Object[] { position + 100, 5, folder });
            myform.Invoke(myform.delegateReleaseSlot, new Object[] { tid });

            string zipToUnpack = outfolder + "//"+title+"//temp.zip";
            string unpackDirectory = outfolder + "//"+title+"//";
            int totalSize;
            using (ZipFile zip1 = ZipFile.Read(zipToUnpack))
            {
                // here, we extract every entry, but we could extract conditionally
                // based on entry name, size, date, checkbox status, etc.  
                totalSize = zip1.Count;
                foreach (ZipEntry ent in zip1)
                {
                    ent.Extract(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }




            List<string> gifStringList = new List<string>();
            for (int i = 0; i < totalSize; i++)
            {
                gifStringList.Add(unpackDirectory + i.ToString("D6") + ".jpg");

            }

            string[] imageFilePaths = gifStringList.ToArray();


            String outputFilePath = outfolder + fileName;
            AnimatedGifEncoder e1 = new AnimatedGifEncoder();
            e1.Start(outputFilePath);
            int n = 100;
            e1.SetDelay(n);
            //-1:no repeat,0:always repeat
            e1.SetRepeat(0);
            if (quality >= 100)
                quality = 100;
            double dq = (double)quality / 100*254;

            int tem = 255 - (int)dq;

            
            if (tem >= 254)
                tem = 254;

            /////////////////todo////////////////
            //tem = 1;

            e1.SetQuality(tem);
            int count;
            count = imageFilePaths.Length;
            for (int i = 0 ; i < count; i++)
            {
                Image myimage = Image.FromFile(imageFilePaths[i]);

                myimage = ScaleImage(myimage, resize);
                e1.SetDelay(delay[i]);
                e1.AddFrame(myimage);
                //e1.SetSizePercent(resize);
                myimage.Dispose();
                
                ///////////debug////////
                

            }

            if (count != delay.Count)
                MessageBox.Show("DEBUG frame doesn't match");
            //e1.SetSizePercent(resize);
            e1.Finish();

            cleanUp(unpackDirectory, outputFilePath);
            string myhtml = Ugoira2gif_pixiv_v1._04.Properties.Resources.String1;
            myhtml = myhtml.Replace("out.gif", title + ".gif");
            try
            {
                System.IO.File.WriteAllText(folder + "\\" + title + ".html", myhtml);
                File.SetAttributes(folder + "\\" + title + ".html", FileAttributes.Hidden);
            }
            catch (Exception)
            { }
            
            myform.Invoke(myform.delegateUpdateStatus, new Object[] { position + 100, 3, "Ready For View" });
            myform.Invoke(myform.delegateUpdateStatus, new Object[] { position + 100, 3, "Ready For View" });
            if (ThreadDone != null)
                ThreadDone(this, EventArgs.Empty);



        }

        public static Image ScaleImage(Image image, int ratio)
        {

            int newWidth = (int)(image.Width * ratio / 100);
            int newHeight = (int)(image.Height * ratio / 100);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            image.Dispose();
            return newImage;
        }

        

        private byte[] GetContentWithProgressReporting(Stream responseStream, long contentLength)
        {
            UpdateProgressBar(0, contentLength);

            double size = contentLength / 1024 / 1024;

            myform.Invoke(myform.delegateUpdateStatus, new Object[] { position, 2, String.Format("{0:0.00}", size.ToString()) + "Mb" });

            // Allocate space for the content
            var data = new byte[contentLength];
            int currentIndex = 0;
            int bytesReceived = 0;
            var buffer = new byte[1024*128];
            do
            {
                bytesReceived = responseStream.Read(buffer, 0, 1024);
                Array.Copy(buffer, 0, data, currentIndex, bytesReceived);
                currentIndex += bytesReceived;

                // Report percentage
                double percentage = (double)currentIndex / contentLength;
                UpdateProgressBar((int)(percentage * 100), contentLength);
            } while (currentIndex < contentLength);

            //UpdateProgressBar(100);
            return data;
        }

        private void UpdateProgressBar(int percentage, long contentLength)
        {
            // If on a worker thread, marshal the call to the UI thread
            //if (progressBar1.InvokeRequired)
            //{
            //    progressBar1.Invoke(new Action<int>(UpdateProgressBar), percentage);
            //}
            //else
            //{
            //    progressBar1.Value = percentage;
            //}


            position = (int)myform.Invoke(myform.delegateUpdateBar, new Object[] { percentage, tid, position, title, (double)contentLength/1024/1024, "Downloading", "fileLocation" });
        }

        public event EventHandler ThreadDone;

        //public void process(List<int> delay, string url)
        //{

        //    string outfolder = Application.StartupPath;


        //    if (url == "")
        //    {

        //        MessageBox.Show("please analyze first");
        //        return;

        //    }

        //    //WebClient wb = new WebClient();
        //    //wb.Headers["Host"] = "i2.pixiv.net";

        //    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);

        //    req.Method = "GET";
        //    req.Headers.Add("Origin", "http://www.pixiv.net");
        //    req.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");

        //    req.Host = "i2.pixiv.net";
        //    req.Headers.Add("Accept-Language", "en-US,en;q=0.8,ja;q=0.6,zh-CN;q=0.4");
        //    req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
        //    req.AddRange(0);
        //    req.AddRange(299999);
        //    req.Accept = "*/*";
        //    req.Referer = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=44310909";

        //    req.Headers.Add("Cache-Control", "max-age=0");
        //    WebResponse resp = req.GetResponse();
            

        //    Stream sm = resp.GetResponseStream();
        //    Directory.CreateDirectory(outfolder + "//temp");
        //    var fileStream = File.Create(outfolder + "//temp//temp.zip");
        //    sm.CopyTo(fileStream);
        //    fileStream.Close();
        //    sm.Close();
        //    resp.Close();




        //    string zipToUnpack = outfolder + "//temp//temp.zip";
        //    string unpackDirectory = outfolder + "//temp//";
        //    int totalSize;
        //    using (ZipFile zip1 = ZipFile.Read(zipToUnpack))
        //    {
        //        // here, we extract every entry, but we could extract conditionally
        //        // based on entry name, size, date, checkbox status, etc.  
        //        totalSize = zip1.Count;
        //        foreach (ZipEntry ent in zip1)
        //        {
        //            ent.Extract(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
        //        }
        //    }




        //    List<string> gifStringList = new List<string>();
        //    for (int i = 0; i < totalSize; i++)
        //    {
        //        gifStringList.Add(unpackDirectory + i.ToString("D6") + ".jpg");

        //    }

        //    string[] imageFilePaths = gifStringList.ToArray();


        //    String outputFilePath = outfolder + "\\out.gif";
        //    AnimatedGifEncoder e1 = new AnimatedGifEncoder();
        //    e1.Start(outputFilePath);
        //    int n = 100;
        //    e1.SetDelay(n);
        //    //-1:no repeat,0:always repeat
        //    e1.SetRepeat(0);
        //    e1.SetQuality(20);
        //    for (int i = 0, count = imageFilePaths.Length; i < count; i++)
        //    {
        //        Image myimage = Image.FromFile(imageFilePaths[i]);


        //        e1.AddFrame(myimage);
        //        myimage.Dispose();
        //        e1.SetDelay(delay[i]);

        //    }
        //    e1.Finish();

        //    if (ThreadDone != null)
        //        ThreadDone(this, EventArgs.Empty);

        //}


    }

}
