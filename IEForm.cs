using System;
using System.ComponentModel;
using System.Windows.Forms;
using mshtml;

namespace XMLTVGrabber
{
    public class IEForm : Form
    {
        private string _documentText;
        private bool _complete;

        private string _url;
        private string _headers;
        private int _timeout;
        private bool _error;

        private Timer _timer;

        public string DocumentText
        {
            get { return _documentText; }
        }

#region Windows Form Designer generated code

        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.WebBrowser webBrowser;

        protected override void Dispose(bool disposing)
        {
            RemoveWebBrowser();

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void RemoveWebBrowser()
        {
            if (webBrowser != null)
            {
                //this.Controls.Remove(this.webBrowser);
                try
                {
                    webBrowser.Dispose();
                }
                catch (Exception)
                {
                }
                webBrowser = null;
            }
        }

        private void InitializeComponent()
        {
            this.webBrowser = new WebBrowser();
            this.SuspendLayout();
			this.webBrowser.Dock = DockStyle.Fill;
            this.webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);

            this.Controls.Add(this.webBrowser);
            this.Name = "IEForm";
            this.ShowInTaskbar = false;
            this.Text = "IEForm";
			this.WindowState = FormWindowState.Minimized;
			this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
		}


#endregion

        public IEForm(string url, string headers, int timeout)
        {
            _url = url;
            _headers = headers;
            _timeout = timeout;
            _error = false;
            _complete = false;

			_timer = new Timer();
			_timer.Interval = _timeout * 1000;
			_timer.Tick += new EventHandler(_timer_Tick);

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadPage();
        }

        public void LoadPage()
        {
            // Start the timeout timer
            _timer.Start();

            /*
                navOpenInNewWindow = 0x1,
                navNoHistory = 0x2,
                navNoReadFromCache = 0x4,
                navNoWriteToCache = 0x8,
                navAllowAutosearch = 0x10,
                navBrowserBar = 0x20,
                navHyperlink = 0x40,
                navEnforceRestricted = 0x80,
                navNewWindowsManaged = 0x0100,
                navUntrustedForDownload = 0x0200,
                navTrustedForActiveX = 0x0400,
                navOpenInNewTab = 0x0800,
                navOpenBackgroundTab = 0x1000,
                navKeepWordWheelText = 0x2000
            */
            object flags = 0x4 | 0x8 | 0x2;
            object frame = "";
            byte[] postData = new byte[] { };
            string headers = _headers;

            this.webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.Navigate(_url, "", postData, headers);
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            if ((_documentText != null) && (_documentText.Length > 0))
            {
                if (_error)
                    this.DialogResult = DialogResult.No;
                else
                    this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.Retry;
            }
            this.Close();
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser.ReadyState == WebBrowserReadyState.Complete)
            {
                _documentText = webBrowser.DocumentText;
                if (_error)
                    this.DialogResult = DialogResult.No;
                else
                    this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }


        //void webBrowser_NavigateError(object sender, AxSHDocVw.DWebBrowserEvents2_NavigateErrorEvent e)
        //{
        //    _error = true;
        //}
        //void webBrowser_NavigateError(object pDisp, ref object URL, ref object Frame, ref object StatusCode, ref bool Cancel)
        //{
        //    _error = true;
        //}


    }
}