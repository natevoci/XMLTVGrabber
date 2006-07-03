using System;
using System.Text;
using System.IO;

namespace XMLTVGrabber
{
	public class BaseUrlContainer
	{
		DateTime date;
		String url = "";
		int pageId = -1;
		StringBuilder pageData = new StringBuilder();

		public BaseUrlContainer()
		{

		}

		public DateTime Date
		{
			get { return date; }
			set { date = value; }
		}

		public String URL
		{
			get { return url; }
			set { url = value; }
		}

		public int PageId
		{
			get { return pageId; }
			set { pageId = value; }
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
            file.Close();
		}
	}
}
