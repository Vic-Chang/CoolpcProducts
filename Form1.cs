using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.Runtime.InteropServices;
namespace CoolpcProucts
{

    struct ShopItem
    {
        public string Subitem { get; set; }
        public Categroy[] _Categroy;
    }
    struct Categroy
    {
        public string CateGloryName { get; set; }
        public CateSubItem _CataSubItem;
    }
    struct CateSubItem
    {
        public List<string> DetailItem { get; set; }
    }

    public partial class Form1 : Form
    {
        ShopItem[] si;
        string source = "";
        string UpdateDate = "";

        //多執行緒
        private BackgroundWorker bw;

        private void initProgressBar()
        {
            progressBar1.Step = 1;
        }

        private void initBackgroundWorker()
        {
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }

        //背景執行
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //for (int i = 1; (i <= 10); i++)
            //{
            //    if ((bw.CancellationPending == true))
            //    {
            //        e.Cancel = true;
            //        break;
            //    }
            //    else
            //    {
            //        // 使用sleep模擬運算時的停頓
            //        System.Threading.Thread.Sleep(500);
            //        bw.ReportProgress((i * 10));
            //    }
            //}


            bw.ReportProgress(10);
            getUpdateTime();


            List<string> BigList = new List<string>();
            string garbg = source;
            int start = garbg.IndexOf("<TR bgColor=efefe0>");
            int end = garbg.IndexOf("<TD class=d></TR>");

            while (start > 0 && end > 0)
            {
                BigList.Add(garbg.Substring(start, end - start + 17));
                garbg = garbg.Remove(start, end - start + 17);
                start = garbg.IndexOf("<TR bgColor=efefe0>");
                end = garbg.IndexOf("<TD class=d></TR>");
            }

            bw.ReportProgress(20);
            //取得所有大清單
            setStruct(BigList);
        }

        //處理進度
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            //this.lblMsg.Text = e.ProgressPercentage.ToString();
            if (e.ProgressPercentage < 20)
            {
                this.lblMsg.Text = "資料抓取中...";
            }
            else if (e.ProgressPercentage < 40)
            { this.lblMsg.Text = "正在序列化..."; }
            else if (e.ProgressPercentage < 70)
            {
                this.lblMsg.Text = "資料解析中...";
            }
            else if (e.ProgressPercentage < 100)
            {
                this.lblMsg.Text = "即將完成...";
            }
        }

        //執行完成
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if ((e.Cancelled == true))
            {
                this.lblMsg.Text = "取消!";
            }

            else if (!(e.Error == null))
            {
                this.lblMsg.Text = ("Error: " + e.Error.Message);
            }

            else
            {

                //資料綁定
                comboBox1.DataSource = si;
                comboBox1.DisplayMember = "Subitem";
                comboBox1.ValueMember = "Subitem";

                progressBar1.Visible = false;
                btn_copy.Enabled = true;
                this.lblMsg.Text = "完成!!";
                this.lblMsg.Text = UpdateDate;
                this.lblMsg.ForeColor = Color.Green;
                this.lblMsg.Font = new Font("Arial", 18);
            }
        }
        public Form1()
        {
            InitializeComponent();
            //多執行緒
            initProgressBar();
            initBackgroundWorker();

            //選單只讀
            Combobox_ReadOnly();
            getData();

        }
        private void getData()
        {
            // 3072 = TSL 1.2
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            WebClient wc = new WebClient();
            byte[] html = wc.DownloadData("http://www.coolpc.com.tw/evaluate.php");
            source = RemoveHTML(Encoding.GetEncoding("Big5").GetString(html), "", "");
        }



        //限制combobox只能讀
        //using System.Runtime.InteropServices;
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);
        int GW_CHILD = 5;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        public const int EM_SETREADONLY = 0xcf;
        //限制combobox只能讀
        private void Combobox_ReadOnly()
        {
            IntPtr editHandle = GetWindow(comboBox1.Handle, GW_CHILD);
            SendMessage(editHandle, EM_SETREADONLY, 1, 0);
            editHandle = GetWindow(comboBox2.Handle, GW_CHILD);
            SendMessage(editHandle, EM_SETREADONLY, 1, 0);
            editHandle = GetWindow(comboBox3.Handle, GW_CHILD);
            SendMessage(editHandle, EM_SETREADONLY, 1, 0);
        }



        public static string RemoveHTML(string strHtml)
        {
            string[] aryReg ={
          @"<script[^>]*?>.*?</script>",
          @"<?(data-adunit)?|(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
          @"([\r\n])[\s]+",
          @"&(quot|#34);",
          @"&(amp|#38);",
          @"&(lt|#60);",
          @"&(gt|#62);", 
          @"&(nbsp|#160);", 
          @"&(iexcl|#161);",
          @"&(cent|#162);",
          @"&(pound|#163);",
          @"&(copy|#169);",
          @"&#(\d+);",
          @"-->",
          @"<!--.*\n",
          @"\[.*?\]", //新增正規是來取代論壇之[*]特殊符號
          
          };
            string[] aryRep = { "", "", "", "\"", "&", "<", ">", " ", "\xa1", "\xa2", "\xa3", "\xa9", "", "\r\n", "", "" };
            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }
            strOutput.Replace("<", "");
            strOutput.Replace(">", "");
            strOutput.Replace("\r\n", "");
            return strOutput;
        }
        private string RemoveHTML(string strHtml, string _start, string _end)
        {
            string[,] a = { 
                          { "<script", "</script>","9" }, { "<style", "</style>","8" }, { "<caption", "</caption>","10" }
                          };
            for (int i = 0; i < a.GetLength(0); i++)
            {
                int start = strHtml.IndexOf(a[i, 0].ToString());
                int end = strHtml.IndexOf(a[i, 1].ToString());
                int stringLengh = Convert.ToInt32(a[i, 2]);
                while (start > 0 && end > 0 && end > start)
                {
                    strHtml = strHtml.Remove(start, end - start + stringLengh);
                    start = strHtml.IndexOf(a[i, 0].ToString());
                    end = strHtml.IndexOf(a[i, 1].ToString());
                }
            }
            return strHtml;
        }
        private void getUpdateTime()
        {
            string garbg = source;
            int start = garbg.IndexOf("品　名");
            int end = garbg.Substring(start).IndexOf("</font>");
            UpdateDate = garbg.Substring(start + 30, end - 122);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (bw.IsBusy != true)
            {
                //禁止再按
                button1.Enabled = false;
                button1.Text = "已啟動";
                progressBar1.Visible = true;

                this.lblMsg.Text = "開始";
                this.progressBar1.Value = 0;
                bw.RunWorkerAsync();
            }

        }
        private void setStruct(List<string> _list)
        {
            si = new ShopItem[_list.Count];
            int countSi = 0;
            int countCg = 0;

            //取得標題
            foreach (string a in _list)
            {
                Regex regex = new Regex("<TR bgColor=efefe0[^>]*?>.*?<TD noWrap>", RegexOptions.IgnoreCase);
                MatchCollection matches = regex.Matches(a);

                foreach (Match mat in matches)
                {
                    si[countSi].Subitem = Regex.Replace(mat.Value, @"<TD[^>]*?>.*?<TD>|<[^>]*>", string.Empty);
                }
                countSi++;
            }

            //取得項目
            countSi = 0;
            foreach (string a in _list)
            {
                Regex regex = new Regex("<OPTGROUP LABEL='.*?>", RegexOptions.IgnoreCase);
                MatchCollection matches = regex.Matches(a);
                //si[countSi]._Categroy.CateGloryName = new List<string>();

                si[countSi]._Categroy = new Categroy[matches.Count];
                foreach (Match mat in matches)
                {
                    //17個字 '<optgroup ...'
                    si[countSi]._Categroy[countCg].CateGloryName = mat.Value.Substring(17, mat.Value.Length - 2 - 17);
                    //si[countSi]._Categroy.CateGloryName.Add(mat.Value.Substring(17, mat.Value.Length - 2 - 17));
                    countCg++;
                }
                //歸0
                countCg = 0;
                countSi++;
            }

            bw.ReportProgress(30);
            //細項大綱段落
            List<string> CataList = new List<string>();
            for (int i = 0; i < _list.Count; i++)
            {
                var item = _list[i];
                int start = item.IndexOf("<OPTGROUP");
                int end = item.Substring(start).IndexOf("</OPTION>") + start;

                while (start > 0 && end > 0)
                {
                    CataList.Add(item.Substring(start, end - start + 9));
                    item = item.Remove(start, end - start + 9);
                    start = item.IndexOf("<OPTGROUP");
                    end = item.Substring((start < 1) ? 0 : start).IndexOf("</OPTION>") + start;
                }
            }


            //建立新struct (不然執行載入細項會變null無法給值)
            countCg = 0;
            countSi = 0;
            foreach (ShopItem a in si)
            {
                foreach (Categroy b in a._Categroy)
                {
                    si[countSi]._Categroy[countCg]._CataSubItem.DetailItem = new List<string>();
                    countCg++;
                }
                countCg = 0;
                countSi++;
            }




            bw.ReportProgress(40);
            countCg = 0;
            countSi = 0;
            //開始載入細項
            for (int i = 0; i < CataList.Count; i++)
            {
                foreach (ShopItem a in si)
                {
                    foreach (Categroy b in a._Categroy)
                    {
                        Regex regex = new Regex("<OPTGROUP LABEL='.*?>", RegexOptions.IgnoreCase);
                        MatchCollection matches = regex.Matches(CataList[i]);
                        foreach (Match c in matches)
                        {
                            if (b.CateGloryName == c.Value.Substring(17, c.Value.Length - 2 - 17))
                            {

                                regex = new Regex("<OPTION value=.*?★", RegexOptions.IgnoreCase);
                                matches = regex.Matches(CataList[i]);
                                foreach (Match mat in matches)
                                {
                                    b._CataSubItem.DetailItem.Add(Regex.Replace(mat.Value, @"<[^>]*>", string.Empty));
                                }
                            }
                        }
                        countCg++;
                    }
                    countSi++;
                }
                if (i > CataList.Count / 1.3)
                {
                    bw.ReportProgress(70);
                }
            }

            bw.ReportProgress(100);

            ////先解除selectChanged事件, 不然只要一綁定就觸發
            //this.comboBox1.SelectedIndexChanged -= new EventHandler(comboBox1_SelectedIndexChanged);
            //this.comboBox2.SelectedIndexChanged -= new EventHandler(comboBox2_SelectedIndexChanged);

            //comboBox1.DataSource = si;
            //comboBox1.DisplayMember = "Subitem";
            //comboBox1.ValueMember = "Subitem";

            ////綁定事件
            //this.comboBox1.SelectedIndexChanged += new EventHandler(comboBox1_SelectedIndexChanged);
            //this.comboBox2.SelectedIndexChanged += new EventHandler(comboBox2_SelectedIndexChanged);

        }

        //抓子項目
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //開始抓子項目
            foreach (ShopItem _ShopItem in si)
            {
                //如果有符合的就綁定
                if (_ShopItem.Subitem == comboBox1.Text.ToString())
                {
                    comboBox2.DataSource = _ShopItem._Categroy;
                    comboBox2.DisplayMember = "CateGloryName";
                }
            }
        }


        //抓細項目
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //開始抓子項目
            foreach (ShopItem _ShopItem in si)
            {
                foreach (Categroy _GloryName in _ShopItem._Categroy)
                {
                    //如果有符合的就綁定
                    if (_GloryName.CateGloryName == comboBox2.Text.ToString())
                    {
                        comboBox3.DataSource = _GloryName._CataSubItem.DetailItem;
                        comboBox3.DisplayMember = "DetailItem";
                    }
                }
            }
        }

        //顯示價格
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //開始抓子項目
            foreach (ShopItem _ShopItem in si)
            {
                foreach (Categroy _GloryName in _ShopItem._Categroy)
                {
                    //如果有符合的就綁定
                    if (_GloryName.CateGloryName == comboBox2.Text.ToString())
                    {
                        textBox1.Text = "";
                        foreach (string a in _GloryName._CataSubItem.DetailItem)
                            textBox1.Text += a + "\r\n";
                    }
                }
            }
        }

        private void btn_copy_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(textBox1.Text); //Copy text to Clipboard
            MessageBox.Show("已成功複製!");
        }

        private void lb_sign_Click(object sender, EventArgs e)
        {
            MessageBox.Show("版本: v1.0 \n用途:取得原價屋價目表，方便給Jenny使用  \n\nby Vic.Chang \nDate:2015/10/14", "原價屋 價目表 v1.0");
        }
    }
}
