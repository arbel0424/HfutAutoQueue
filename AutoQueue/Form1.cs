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
        private Thread runThread = null;
        private Mutex gM;
        delegate void SetTextCallback(string text);
        delegate void SetButtonCallback(bool bStatus);
        protected string strNumResult;


        public QueueForm()
        {
            InitializeComponent();

            // 添加测试代码
            System.Diagnostics.Trace.Listeners.Clear();
            System.Diagnostics.Trace.AutoFlush = true;
            Debug.Listeners.Add(
                new System.Diagnostics.TextWriterTraceListener("log.txt"));
            Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("log.txt"));

            // cookie容器
            ckCollection = new CookieCollection();
            // 线程互斥体
            gM = new Mutex(true, "QueueMutex");
            StopButton.Enabled = false;
            InfoText.Text = "当前状态-空闲";
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            // 尝试验证输入的用户名密码
            ResetButtonStatus(false);
            UpdateInfo("验证用户名密码...");
            if (Login(UserNameText.Text, PasswordText.Text))
                UpdateInfo("验证成功!");
            else
            {
                UpdateInfo("用户名或密码错误，请验证后重试");
                ResetButtonStatus(true);
                PasswordText.Text = "";
                return;
            }
            if (null == this.runThread)
            {
                this.runThread = new Thread(new ThreadStart(this.OnTimeRun));
                this.runThread.Start();
            }
            gM.ReleaseMutex();
        }

        public void OnTimeRun()
        {
            DateTime target;
            DateTime current;
            TimeSpan ts;
            string strTime;
            current = DateTime.Now;
            if (current.Hour < 8 || current.Hour > 16)
            {
                // 非取票时间，等待取票开始
                target = DateTime.Now;
                if (current.Hour > 16)
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
            // 直接取当天的票
            int i = 0;
            while (i < 5000)
            {
                i++;
                gM.WaitOne();
                if (Login(UserNameText.Text, PasswordText.Text))
                {
                    UpdateInfo("提交请求...");
                    if (PostRequest())
                        break;
                }
                gM.ReleaseMutex();
                Thread.Sleep(1000);
            }
            // 恢复控件状态
            ResetButtonStatus(true);
        }

        public void UpdateInfo(string strInfo)
        {
            if (this.InfoText.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(UpdateInfo);
                this.Invoke(d, new object[] { strInfo});
            }
            else
                InfoText.Text = strInfo;
        }

        public void UpdateResult(string strInfo)
        {
            if (this.InfoText.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(UpdateResult);
                this.Invoke(d, new object[] { strInfo});
            }
            else
                ResultText.Text = "取到的号码-" + strInfo;
        }

        public void ResetButtonStatus(bool bStatus)
        {
            if (this.StartButton.InvokeRequired || this.StopButton.InvokeRequired)
            {
                SetButtonCallback d = new SetButtonCallback(ResetButtonStatus);
                this.Invoke(d, new object[] { bStatus});
            }
            else
            {
                UserNameText.Enabled = bStatus;
                PasswordText.Enabled = bStatus;
                StartButton.Enabled = bStatus;
                StopButton.Enabled = !bStatus;
            }
        }
        public bool Login(string strUid, string strPwd)
        {
            strUid = HttpUtility.UrlEncode(strUid);
            strPwd = HttpUtility.UrlEncode(strPwd);
            string postData = string.Format("uid={0}&pwd={1}", strUid, strPwd);
            byte[] param = System.Text.Encoding.GetEncoding("gb2312").GetBytes(postData);
            try
            {
                CookieContainer cc = new CookieContainer();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(LoginUrl);
                req.CookieContainer = cc;
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                req.ContentLength = param.Length;
                req.AllowAutoRedirect = false;
                Stream webStream = req.GetRequestStream();
                webStream.Write(param, 0, param.Length);
                webStream.Close();

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, Encoding.GetEncoding("gb2312"));
                string result = reader.ReadToEnd();

                // 检查获得的cookie
                foreach (Cookie ck in response.Cookies)
                {
                    if (ck.Domain == "210.45.241.169")
                    {
                        ckCollection = response.Cookies;
                        return true;
                    }
                }

                // 出现错误
                Trace.WriteLine("登录时出现错误");
                Trace.Write(result);
                return false;
            }
            catch (WebException we)
            {
                string msg = we.Message;
                return false;
            }
            catch 
            {
                return false;
            }
        }
        public bool PostRequest()
        {
            string strViewState = null;
            string strValidation = null;
            string strResult = null;
            string strListID = null;
            string pattern1 = @"id=""__VIEWSTATE""\svalue=""(?<key>.+)""";
            string pattern2 = @"id=""__EVENTVALIDATION""\svalue=""(?<key>.+)""";
            string pattern3 = @"<option\s.*value=""(?<key>.+)""";
            string pattern4 = @"javascript'>alert\('(?<key>.+)'\)";
            string pattern5 = @"window\.location='(?<key>.+)'";
            string pattern6 = @"\[.+\].+\[(?<key>.+)\]";
            string pattern7 = @"\[综合报销业务].+\[(?<key>.+)\]";
            string paramPattern = @"__VIEWSTATE={0}&__EVENTVALIDATION={1}&ImageButton1.x={2}&ImageButton1.y={3}&DropDownList2={4}&deptID=1&dateType=Today&timeType=AM";

            try
            {
                // 首先要提取相关要POST的值
                CookieContainer cc = new CookieContainer();
                HttpWebRequest KeyReq = (HttpWebRequest)WebRequest.Create(QueueUrl);
                KeyReq.CookieContainer = cc;
                KeyReq.CookieContainer.Add(ckCollection);
                KeyReq.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)KeyReq.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream,
                                            Encoding.GetEncoding("gb2312"));
                string result = reader.ReadToEnd();
                // 利用正则进行提取
                Match mc1 = Regex.Match(result, pattern1);
                if (mc1 != null)
                    strViewState = mc1.Groups["key"].Value;
                else
                    return false;
                Match mc2 = Regex.Match(result, pattern2);
                if (mc2 != null)
                    strValidation = mc2.Groups["key"].Value;
                else
                    return false;
                Match mc3 = Regex.Match(result, pattern3);
                if (mc3 != null)
                    strListID = mc3.Groups["key"].Value;
                else
                    return false;
            }
            catch (WebException we)
            {
                string msg = we.Message;
                Trace.Write(msg);
                return false;
            }
            catch
            {
                Trace.WriteLine("获取网页提交参数时发生未知错误");
                return false;
            }

            // 构造提交参数
            Random rd = new Random();
            int x = rd.Next(150) + 10;
            int y = rd.Next(20) + 5;
            strViewState = HttpUtility.UrlEncode(strViewState);
            strValidation = HttpUtility.UrlEncode(strValidation);
            string postData = string.Format(paramPattern, strViewState,
                                        strValidation, x, y, strListID);
            byte[] param = System.Text.Encoding.ASCII.GetBytes(postData);
            // 提交请求
            try
            {
                CookieContainer cc = new CookieContainer();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(QueueUrl);
                //req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:18.0) Gecko/20100101 Firefox/18.0";
                req.CookieContainer = cc;
                req.CookieContainer.Add(ckCollection);
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                req.ContentLength = param.Length;
                Stream webStream = req.GetRequestStream();
                webStream.Write(param, 0, param.Length);
                webStream.Close();

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, Encoding.GetEncoding("gb2312"));
                string result = reader.ReadToEnd();

                Match mc = Regex.Match(result, pattern4);
                if (mc != null)
                {
                    strResult = mc.Groups["key"].Value;
                    UpdateInfo(strResult);
                    if (strResult.IndexOf("取号成功") != -1)
                    {
                        // 访问打印页面
                        Match mc5 = Regex.Match(result, pattern5);
                        if (mc != null)
                        {
                            string strPrint = mc5.Groups["key"].Value;
                            CookieContainer cc2 = new CookieContainer();
                            HttpWebRequest req2 = (HttpWebRequest)WebRequest.Create(QueueUrl);
                            //req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:18.0) Gecko/20100101 Firefox/18.0";
                            req2.CookieContainer = cc2;
                            req2.CookieContainer.Add(ckCollection);
                            req2.ContentType = "application/x-www-form-urlencoded";
                            req2.Method = "GET";
                            HttpWebResponse resp2 = (HttpWebResponse)req.GetResponse();
                            Stream dataStream2 = resp.GetResponseStream();
                        }
                        // 获取取到的号
                        Match mc6 = Regex.Match(strResult, pattern6);
                        if (mc != null)
                        {
                            string strNum = mc6.Groups["key"].Value;
                            UpdateResult(strNum);
                        }
                        return true;
                    }
                    if (strResult.IndexOf("取号个人额度已满") != -1)
                    {
                        // 获取已经取到的号
                        UpdateInfo("已经取过号");
                        Match mc7 = Regex.Match(result, pattern7);
                        if (mc != null)
                        {
                            string strNum = mc7.Groups["key"].Value;
                            UpdateResult(strNum);
                        }
                        return true;
                    }
                }
                else
                {
                    Trace.WriteLine("无法解析网页内容");
                    Trace.Write(result);
                    return false;
                }
            }
            catch (WebException we)
            {
                string msg = we.Message;
                Trace.WriteLine(msg);
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
    }
}
