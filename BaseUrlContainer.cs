using System;
using System.Text;
using System.IO;

namespace XMLTVGrabber
{
	public class BaseUrlContainer
	{
		DateTime date = DateTime.MinValue;
		String url = "";
		StringBuilder pageData = new StringBuilder();

		public BaseUrlContainer()
		{

		}

        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

		public void setURL(String pageurl)
		{
			url = pageurl;
		}

		public String getURL()
		{
			return url;
		}

		public void resetPageData()
		{
			pageData = new StringBuilder();
		}

		public StringBuilder getPageData()
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
