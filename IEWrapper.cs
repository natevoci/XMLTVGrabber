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
                IEForm form = new IEForm(url, headers, timeout);
                System.Windows.Forms.DialogResult result = form.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.Retry)
                {
                    Console.WriteLine("Error getting Data: Timeout");
                    return -2;
                }
                else if (result == System.Windows.Forms.DialogResult.No)
                {
                    Console.WriteLine("Error getting Data: A navigation error occured. Check your internet connection.");
                    return -6;
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    Console.WriteLine("Error getting Data: IEForm was unexpected closed");
                    return -4;
                }
                else if (result != System.Windows.Forms.DialogResult.OK)
                {
                    Console.WriteLine("Error getting Data: Unknown");
                    return -5;
                }

                data.Append(form.DocumentText);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting Data: \r\n" + e.ToString());
                return -3;
            }

			if(timoutHappened)
				return -2;

			return 0;
		}
	}
}
