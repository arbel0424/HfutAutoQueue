using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading;

namespace AutoQueue
{
    public partial class QueueForm : Form
    {
        protected System.Net.CookieCollection ckCollection = null;
        protected const string LoginUrl = "http://210.45.241.169/login_gr.aspx";
        protected const string QueueUrl = "http://210.45.241.169/baobiao/Queue/QueueSystem.aspx?deptID=1&dateType=Today&timeType=AM";
        protected const string UrlPrefix = "http://210.45.241.169/baobiao/Queue/";
        protected const string strFilePrefix = "print";
        private Thread runThread = null;
        private Mutex gM;
        delegate void SetTextCallback(string text);
        delegate void SetButtonCallback(bool bStatus);
        protected string strNumResult;
        protected int nSkipNum;

        // 使用TraceSource记录日志
        private static TraceSource mySource = new TraceSource("HfutQueueLog");

        public InternetRequest req = new InternetRequest();

        private const int MAXREQUESTCOUNT = 5000;
        private const int MINREQUESTCOUNT = 5;


        public QueueForm()
        {
            InitializeComponent();
            DownloadPicFiles();

            // 添加测试代码
            // 使用TraceSource
            mySource.Switch = new SourceSwitch("sourceSwitch", "Error");
            mySource.Listeners.Remove("Default");
            mySource.Switch.Level = SourceLevels.All;
            // 添加文件记录
            TextWriterTraceListener textListener = new TextWriterTraceListener("log.txt");
            textListener.TraceOutputOptions = TraceOptions.DateTime; // | TraceOptions.Callstack;
            textListener.Filter = new EventTypeFilter(SourceLevels.Information);
            mySource.Listeners.Add(textListener);

            // cookie容器
            ckCollection = new CookieCollection();
            // 线程互斥体
            gM = new Mutex(true, "QueueMutex");
            StopButton.Enabled = false;
            InfoText.Text = "当前状态-空闲";
            NumLimit.Value = 0;
            LimitInfo.Text = "取最前号";

            mySource.TraceInformation("程序启动");
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartQueue();


            //// 尝试验证输入的用户名密码
            //if (UserNameText.Text.Length == 0 || PasswordText.Text.Length == 0)
            //{
            //    MessageBox.Show("请输入用户名和密码", "错误");
            //    return;
            //}
            //UpdateInfo("验证用户名密码...");
            //if (Login(UserNameText.Text, PasswordText.Text))
            //{
            //    UpdateInfo("验证成功!");
            //    mySource.TraceInformation("验证用户名密码成功");
            //}
            //string strPage = File.ReadAllText("test.htm", Encoding.GetEncoding("GBK"));
            //DownloadPrintPage(strPage);

        }

        /// <summary>
        /// 开始取票函数
        /// </summary>
        public void StartQueue()
        {
            // 尝试验证输入的用户名密码
            if (UserNameText.Text.Length == 0 || PasswordText.Text.Length == 0)
            {
                MessageBox.Show("请输入用户名和密码", "错误");
                return;
            }
            ResetButtonStatus(false);
            UpdateInfo("验证用户名密码...");
            if (Login(UserNameText.Text, PasswordText.Text))
            {
                UpdateInfo("验证成功!");
                mySource.TraceInformation("验证用户名密码成功");
            }
            else
            {
                UpdateInfo("用户名或密码错误，请验证后重试");
                ResetButtonStatus(true);
                PasswordText.Text = "";
                return;
            }
            nSkipNum = NumLimit.Value * 10;
            if (null == this.runThread)
            {
                this.runThread = new Thread(new ThreadStart(this.OnTimeRun));
                this.runThread.Start();
            }
            gM.ReleaseMutex();
        }

        public void WaitNextDay()
        {
            string strTime;
            DateTime target;
            DateTime current;
            TimeSpan ts;

            // 非取票时间，等待取票开始
            current = DateTime.Now;
            mySource.TraceInformation("非取票时间，等待取票开始");
            target = DateTime.Now;
            if (current.Hour > 9)
            {
                // 取的是明天的票
                target = target.AddDays(1);
            }
            strTime = target.ToLongDateString() + " 08:00:00";
            //strTime = target.ToLongDateString() + " 00:12:00";
            target = DateTime.Parse(strTime);
            while (true)
            {
                gM.WaitOne();
                current = DateTime.Now;
                ts = target.Subtract(current);
                if (ts.TotalMinutes < 5)
                {
                    gM.ReleaseMutex();
                    break;
                }
                strTime = "离取票时间还有" + ts.Hours.ToString() + "小时"
                    + ts.Minutes + "分钟" + ts.Seconds + "秒";
                UpdateInfo(strTime);
                gM.ReleaseMutex();
                Thread.Sleep(1000);
            }

        }

        public bool SkipTicket()
        {
            // 在特定序号过去后才取号

            string pattern1 = @"<span\sid=""Repeater1_ctl00_Literal1""\sclass=""txt"">(?<key>.+)</span>";
            int nPreNum = 0;
            int nNum = 0;
            int nChange = 0;
            int nCount = 0;

            UpdateInfo("跳过特定序号...");
            while (nCount < 100 && nChange < 1000)
            {
                if (Login(UserNameText.Text, PasswordText.Text))
                {
                    string page = req.GetUrl(QueueUrl);
                    Match mc = Regex.Match(page, pattern1);
                    if (mc.Success)
                    {
                        string info = mc.Groups["key"].Value;
                        int nStart = info.IndexOf('[');
                        int nEnd = info.IndexOf(']');
                        string strNum = info.Substring(nStart + 1, nEnd - nStart - 1);
                        nPreNum = nNum;
                        // 报销业务的票从1000开始
                        nNum = Convert.ToInt16(strNum);
                        if (nPreNum == nNum && nNum != 0)
                            nChange++;
                        else
                            nChange = 0;
                        if (nSkipNum < nNum)
                            return true;
                    }
                    Thread.Sleep(1000);
                }
            }
            return false;
        }

        public void OnTimeRun()
        {
            DateTime current;
            string pattern0 = @"综合报销业务取号暂停办理";

            string page = req.GetUrl(QueueUrl);
            // 首先获得网页内容
            if (page.IndexOf(pattern0) != -1)
            {
                // 今天不能取票，取明天的票
                WaitNextDay();
            }

            // 开始取票
            if (nSkipNum != 0)
            {
                // 如果设置了定点取票，则等待特定时间
                SkipTicket();
            }
            mySource.TraceInformation("开始取票");
            // 设置最大取票次数
            int i = 0;
            int MaxTryCount = MAXREQUESTCOUNT;
            bool bStatus = false;
            current = DateTime.Now;
            if (current.Hour > 9)
                MaxTryCount = MINREQUESTCOUNT;

            while (i < MaxTryCount)
            {
                i++;
                gM.WaitOne();
                if (Login(UserNameText.Text, PasswordText.Text))
                {
                    UpdateInfo("提交请求...");
                    if (PostRequest())
                    {
                        bStatus = true;
                        break;
                    }
                }

                gM.ReleaseMutex();
                Thread.Sleep(1000);
            }
            if (!bStatus)
            {
                // 取票失败
                mySource.TraceInformation("取票失败");
                if (MaxTryCount == MINREQUESTCOUNT)
                    UpdateInfo("取票失败,请检查今天是否可以取票");
                else
                    UpdateInfo("取票失败");
            }
            // 恢复控件状态
            ResetButtonStatus(true);
        }

        public void UpdateInfo(string strInfo)
        {
            if (this.InfoText.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(UpdateInfo);
                this.Invoke(d, new object[] { strInfo });
            }
            else
                InfoText.Text = strInfo;
        }

        public void UpdateResult(string strInfo)
        {
            if (this.InfoText.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(UpdateResult);
                this.Invoke(d, new object[] { strInfo });
            }
            else
                ResultText.Text = "取到的号码-" + strInfo;
        }

        public void ResetButtonStatus(bool bStatus)
        {
            if (this.StartButton.InvokeRequired || this.StopButton.InvokeRequired)
            {
                SetButtonCallback d = new SetButtonCallback(ResetButtonStatus);
                this.Invoke(d, new object[] { bStatus });
            }
            else
            {
                UserNameText.Enabled = bStatus;
                PasswordText.Enabled = bStatus;
                StartButton.Enabled = bStatus;
                StopButton.Enabled = !bStatus;
                NumLimit.Enabled = bStatus;
            }
        }
        public bool Login(string strUid, string strPwd)
        {
            string postData = string.Format("uid={0}&pwd={1}", strUid, strPwd);
            try
            {
                string result = req.PostToUrl(LoginUrl, postData);
                if (result.IndexOf("密码错误") == -1)
                {
                    return true;
                }
                else
                {
                    mySource.TraceInformation("登录密码错误");
                    return false;
                }
            }
            catch (WebException we)
            {
                string msg = we.Message;
                mySource.TraceEvent(TraceEventType.Error, 15, msg);
                return false;
            }
            catch
            {
                mySource.TraceEvent(TraceEventType.Error, 16, "访问网页出现未知错误");
                return false;
            }
        }
        public bool PostRequest()
        {
            string strViewState = null;
            string strValidation = null;
            string strResult = null;
            string pattern1 = @"id=""__VIEWSTATE""\svalue=""(?<key>.+)""";
            string pattern2 = @"id=""__EVENTVALIDATION""\svalue=""(?<key>.+)""";
            string pattern4 = @"javascript'>alert\('(?<key>.+)'\)";
            string pattern5 = @"window\.location='(?<key>.+)'";
            string pattern6 = @"\[.+\].+\[(?<key>.+)\]";
            string pattern7 = @"\[综合报销业务].+\[(?<key>.+)\]";
            string paramPattern = @"__VIEWSTATE={0}&__EVENTVALIDATION={1}&Repeater1$ctl00$ImageButton1.x={2}&Repeater1$ctl00$ImageButton1.y={3}&deptID=1&dateType=Today&timeType=AM";

            try
            {
                string result = req.GetUrl(QueueUrl);
                // 利用正则进行提取
                // 获得__VIEWSTATE
                Match mc1 = Regex.Match(result, pattern1);
                if (mc1.Success)
                    strViewState = mc1.Groups["key"].Value;
                else
                {
                    mySource.TraceEvent(TraceEventType.Error, 20, "正则解析失败");
                    mySource.TraceEvent(TraceEventType.Error, 20, pattern1);
                    mySource.TraceEvent(TraceEventType.Error, 20, result);
                    return false;
                }
                // 获得__EVENTVALIDATION
                Match mc2 = Regex.Match(result, pattern2);
                if (mc2.Success)
                    strValidation = mc2.Groups["key"].Value;
                else
                {
                    mySource.TraceEvent(TraceEventType.Error, 20, "正则解析失败");
                    mySource.TraceEvent(TraceEventType.Error, 20, pattern2);
                    mySource.TraceEvent(TraceEventType.Error, 20, result);
                    return false;
                }
            }
            catch (WebException we)
            {
                string msg = we.Message;
                mySource.TraceEvent(TraceEventType.Error, 25, msg);
                return false;
            }
            catch
            {
                mySource.TraceEvent(TraceEventType.Error, 26, "获取网页提交参数时发生未知错误");
                return false;
            }

            // 构造提交参数
            Random rd = new Random();
            int x = rd.Next(150) + 10;
            int y = rd.Next(20) + 5;
            strValidation = HttpUtility.UrlEncode(strValidation);
            strViewState = HttpUtility.UrlEncode(strViewState);
            string postData = string.Format(paramPattern, strViewState,
                                        strValidation, x, y);
            // 提交请求
            try
            {
                string result = req.PostToUrl(QueueUrl, postData);

                Match mc4 = Regex.Match(result, pattern4);
                if (mc4.Success)
                {
                    strResult = mc4.Groups["key"].Value;
                    UpdateInfo(strResult);
                    if (strResult.IndexOf("取号成功") != -1)
                    {
                        // 访问打印页面
                        mySource.TraceInformation(strResult);
                        Match mc5 = Regex.Match(result, pattern5);
                        if (mc5.Success)
                        {
                            string strPrint = mc5.Groups["key"].Value;
                            Uri prefix = new Uri(QueueUrl);
                            Uri PrintUrl = new Uri(prefix, strPrint);
                            string print = req.GetUrl(PrintUrl.AbsoluteUri);
                            // 保存打印页面
                            DownloadPrintPage(print);
                        }
                        else
                        {
                            mySource.TraceEvent(TraceEventType.Error, 20, "正则解析失败");
                            mySource.TraceEvent(TraceEventType.Error, 20, pattern5);
                            mySource.TraceEvent(TraceEventType.Error, 20, result);
                        }
                        // 获取取到的号
                        Match mc6 = Regex.Match(strResult, pattern6);
                        if (mc6.Success)
                        {
                            mySource.TraceInformation(strResult);
                            string strNum = mc6.Groups["key"].Value;
                            UpdateResult(strNum);
                        }
                        else
                        {
                            mySource.TraceEvent(TraceEventType.Error, 20, "正则解析失败");
                            mySource.TraceEvent(TraceEventType.Error, 20, pattern6);
                            mySource.TraceEvent(TraceEventType.Error, 20, result);
                        }
                        return true;
                    }
                    if (strResult.IndexOf("取号个人额度已满") != -1)
                    {
                        // 获取已经取到的号
                        UpdateInfo("已经取过号");
                        mySource.TraceInformation(strResult);
                        Match mc7 = Regex.Match(result, pattern7);
                        if (mc7.Success)
                        {
                            string strNum = mc7.Groups["key"].Value;
                            UpdateResult(strNum);
                        }
                        else
                        {
                            mySource.TraceEvent(TraceEventType.Error, 20, "正则解析失败");
                            mySource.TraceEvent(TraceEventType.Error, 20, pattern7);
                            mySource.TraceEvent(TraceEventType.Error, 20, result);
                        }
                        return true;
                    }
                }
                else
                {
                    mySource.TraceEvent(TraceEventType.Error, 20, "正则解析失败");
                    mySource.TraceEvent(TraceEventType.Error, 20, pattern4);
                    mySource.TraceEvent(TraceEventType.Error, 20, result);
                    return false;
                }
            }
            catch (WebException we)
            {
                string msg = we.Message;
                mySource.TraceEvent(TraceEventType.Error, 27, msg);
                return false;
            }

            return false;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            UpdateInfo("正在暂停...");
            gM.WaitOne();
            UserNameText.Enabled = true;
            PasswordText.Enabled = true;
            StartButton.Enabled = true;
            UpdateInfo("程序已经被暂停");
        }

        private void QueueForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (null != this.runThread)
                if (this.runThread.IsAlive)
                    this.runThread.Abort();
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            Form frm = new About();
            frm.ShowDialog();
        }

        private void NumLimit_Scroll(object sender, EventArgs e)
        {
            int nValue = NumLimit.Value * 10;
            string strTemp = Convert.ToString(nValue) + "号以后";
            if (nValue == 0)
                LimitInfo.Text = "取最前号";
            else
                LimitInfo.Text = strTemp;
        }

        /// <summary>下载打印页面所需要的图片</summary>
        /// 用于离线打印表单
        private void DownloadPicFiles()
        {
            const string strUrlLogo = "images/logo.png";
            const string strUrlTitle = "images/titlebg.png";

            Uri prefix = new Uri(UrlPrefix);
            Uri uriLogo = new Uri(prefix, strUrlLogo);
            Uri uriTitle = new Uri(prefix, strUrlTitle);
            WebClient wbClinet = new WebClient();

            // 创建相关目录
            if (Directory.Exists(strFilePrefix) == false)
                Directory.CreateDirectory(strFilePrefix);
            if (Directory.Exists(strFilePrefix) == false)
                return;
            if (Directory.Exists(strFilePrefix + "/images") == false)
                Directory.CreateDirectory(strFilePrefix + "/images");
            if (Directory.Exists(strFilePrefix + "/images") == false)
                return;

            // 下载图片文件
            string strFile = strFilePrefix + "/";
            if (File.Exists(strFile + strUrlLogo) == false)
                wbClinet.DownloadFile(uriLogo, strFile + strUrlLogo);
            if (File.Exists(strFile + strUrlTitle) == false)
                wbClinet.DownloadFile(uriTitle, strFile + strUrlTitle);
        }

        /// <summary>下载打印页面内容</summary>
        private void DownloadPrintPage(string page)
        {
            //string pattern = @"<img\sid=""BarCode_Image""\ssrc=""(?<key>.+)""\sstyle=""border-width:0px;""\s/>";
            string pattern = @"<img\sid=""BarCode_Image""\ssrc=""(?<key>.+)""\sborder=""0""\s/>";
            string strUrl;
            Match mc = Regex.Match(page, pattern);
            if (mc.Success)
                strUrl = mc.Groups["key"].Value;
            else
                return;
            Uri prefix = new Uri(QueueUrl);
            Uri ImageUrl = new Uri(prefix, strUrl);
            string strFileName = UserNameText.Text + "-" + DateTime.Now.ToString("MMdd");
            string strSaveName;
            if (Directory.Exists(strFilePrefix) == true)
                strSaveName = strFilePrefix + "/" + strFileName + ".html";
            else
                strSaveName = strFileName + ".html";
            StreamWriter file = new StreamWriter(strSaveName, false, Encoding.GetEncoding("gbk"));
            file.Write(page.Replace(strUrl, strFileName + ".jpg"));
            file.Close();
            // 下载二维码图片
            Image img = req.DownloadImg(ImageUrl.AbsoluteUri);
            if (Directory.Exists(strFilePrefix) == true)
                strSaveName = strFilePrefix + "/" + strFileName + ".jpg";
            else
                strSaveName = strFileName + ".jpg";
            img.Save(strSaveName);
        }
    }

    public class InternetRequest
    {
        protected System.Net.CookieCollection ckCollection = null;

        public InternetRequest()
        {
            // cookie容器
            ckCollection = new CookieCollection();
        }

        public string PostToUrl(string strUrl, string strParams)
        {
            // 添加Cookie
            CookieContainer cc = new CookieContainer();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strUrl);
            req.CookieContainer = cc;
            req.CookieContainer.Add(ckCollection);
            // 设置POST相关参数
            byte[] param = System.Text.Encoding.GetEncoding("gb2312").GetBytes(strParams);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            req.ContentLength = param.Length;
            req.AllowAutoRedirect = false;
            req.Timeout = 5000;
            Stream webStream = req.GetRequestStream();
            webStream.Write(param, 0, param.Length);
            webStream.Close();
            // 获得网站返回内容
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            // 保存Cookie
            ckCollection.Add(response.Cookies);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream, Encoding.GetEncoding("gb2312"));
            string result = reader.ReadToEnd();
            return result;
        }
        public string GetUrl(string strUrl)
        {
            // 添加Cookie
            CookieContainer cc = new CookieContainer();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strUrl);
            req.CookieContainer = cc;
            req.CookieContainer.Add(ckCollection);
            req.Method = "GET";
            // 防止重定向无法获取Cookie
            req.AllowAutoRedirect = false;
            req.Timeout = 5000;
            // 获得网站返回内容
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            // 添加Cookie
            ckCollection.Add(response.Cookies);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream, Encoding.GetEncoding("gb2312"));
            string result = reader.ReadToEnd();
            return result;
        }
        public Image DownloadImg(string strUrl)
        {
            Image img;
            // 添加Cookie
            CookieContainer cc = new CookieContainer();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strUrl);
            req.CookieContainer = cc;
            req.CookieContainer.Add(ckCollection);
            req.Method = "GET";
            // 防止重定向无法获取Cookie
            //req.AllowAutoRedirect = false;
            req.Timeout = 5000;
            // 获得网站返回内容
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            // 添加Cookie
            ckCollection.Add(response.Cookies);
            Stream dataStream = response.GetResponseStream();
            img = Image.FromStream(dataStream);

            return img;
        }
    }
}
