using System;
using System.Text;
using System.IO;

namespace XMLTVGrabber
{
	public class BaseUrlContainer
	{
		String date = "";
		String url = "";
		StringBuilder pageData = new StringBuilder();

		public BaseUrlContainer()
		{

		}

		public void setDate(String pageDate)
		{
			date = pageDate;
		}

		public void setURL(String pageurl)
		{
			url = pageurl;
		}

		public String getURL()
		{
			return url;
		}

		public String getDate()
		{
			return date;
		}

		public void resetPageData()
		{
			pageData = new StringBuilder();
		}

		public StringBuilder getPageDate()
		{
			return pageData;
		}

		public void dumpPageData(String name)
		{
			FileStream file = new FileStream(name, FileMode.Create, FileAccess.ReadWrite);
			StreamWriter sw = new StreamWriter(file);
			sw.Write(pageData.ToString());
			sw.Close();
		}
	}
}
