using System;
using System.Text;
using System.Threading;
using mshtml;

namespace XMLTVGrabber
{
	public class IEWrapper
	{
		String url = "";
		String headers = null;
		String postData = null;
		int timeout = 150;

		public IEWrapper()
		{

		}

		public void setURL(String loc)
		{
			url = loc;
		}

		public void setHeaders(String head)
		{
			headers = head;
		}

		public void setPostData(String post)
		{
			postData = post;
		}

		public void setTimeOut(int time)
		{
			timeout = time;
		}

		public int getData(StringBuilder data)
		{
			if(url == null || url.Length == 0)
			{
				return -1;
			}

			bool timoutHappened = false;

			try
			{
				Type IETipo = Type.GetTypeFromProgID("InternetExplorer.Application");

				object IEX = Activator.CreateInstance(IETipo);

				IETipo.InvokeMember("Visible",
					System.Reflection.BindingFlags.SetProperty, null, IEX, new object [] { false });

				IETipo.InvokeMember("Silent",
					System.Reflection.BindingFlags.SetProperty, null, IEX, new object [] { true });

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


				object[] funcParams = new object[5];
				funcParams[0] = url;
				funcParams[1] = 0x4 | 0x8 | 0x2;// | 0x1;
				funcParams[2] = 0;
				funcParams[3] = 0;//postData; // for now just use GET request
				funcParams[4] = headers;

				IETipo.InvokeMember("Navigate",
					System.Reflection.BindingFlags.InvokeMethod, null, IEX, funcParams);

				int counter = 0;
				timoutHappened = false;

				while(getIEState(IETipo, IEX) != 4)
				{
					Console.Write(".");
					//Console.WriteLine("waiting (" + counter++ + ") ...");
					if(counter > timeout)
					{
						
						Console.WriteLine("Download Timed OUT!!");
						timoutHappened = true;
						break;
					}
					Thread.Sleep(1000);
				}
				Console.WriteLine("");

				HTMLDocument myDoc = (HTMLDocument)IETipo.InvokeMember("Document",
					System.Reflection.BindingFlags.GetProperty, null, IEX, null);
				String pageData = myDoc.documentElement.outerHTML;

				IETipo.InvokeMember("Quit",
					System.Reflection.BindingFlags.InvokeMethod, null, IEX, new object [] {  });

				IEX = null;
				IETipo = null;

				data.Append(pageData);
			}
			catch(Exception e)
			{
				Console.WriteLine("Error getting Data: \r\n" + e.ToString());
				return -3;
			}

			if(timoutHappened)
				return -2;

			return 0;
		}

		private int getIEState(Type ieType, object ie)
		{
			return (int)ieType.InvokeMember("ReadyState",
				System.Reflection.BindingFlags.GetProperty, null, ie, null);
		}
	}
}
