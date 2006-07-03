using System;
using System.ComponentModel;
using System.Windows.Forms;
using mshtml;

namespace XMLTVGrabber
{
    public class IEForm : Form
    {
        private string _documentText;

        private string _url;
        private string _headers;
        private int _timeout;

        private Timer _timer;

        public string DocumentText
        {
            get { return _documentText; }
        }

#region Windows Form Designer generated code

        private System.ComponentModel.IContainer components = null;
		private AxSHDocVw.AxWebBrowser webBrowser;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
			this.webBrowser = new AxSHDocVw.AxWebBrowser();
            this.SuspendLayout();
			this.webBrowser.Dock = DockStyle.Fill;
			this.webBrowser.DocumentComplete += new AxSHDocVw.DWebBrowserEvents2_DocumentCompleteEventHandler(WebBrowser_DocumentComplete);

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

			_timer = new Timer();
			_timer.Interval = _timeout * 1000;
			_timer.Tick += new EventHandler(_timer_Tick);

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
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
			object postData = new byte[] { };
			object headers = _headers;

			this.webBrowser.Silent = true;
			webBrowser.Navigate(_url, ref flags, ref frame, ref postData, ref headers);
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

		protected void WebBrowser_DocumentComplete(object sender, AxSHDocVw.DWebBrowserEvents2_DocumentCompleteEvent e)
		{
			HTMLDocument myDoc = (HTMLDocument)webBrowser.Document;
			_documentText = myDoc.documentElement.outerHTML;
			
			this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}