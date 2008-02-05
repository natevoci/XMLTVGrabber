using System;
using System.Xml;

namespace XMLTVGrabber
{
	public class ProgramInfo
	{
		public String progID = "";
		public String title = "";
		public String subtitle = "";
		public DateTime startTime = new DateTime();
		public int duration = 0;
		public String channel = "";
		public String description = "";
		public String rating = "";
        public String category = "";
		public String detailsURL = "";

		public ProgramInfo()
		{

		}

		public bool addToXML(XmlDocument doc, XmlElement parent, String zone)
		{
			XmlElement prog = doc.CreateElement("programme");

			prog.SetAttribute("channel", channel);

			// build start time
			String xmlTVStart = startTime.ToString("yyyyMMddHHmmss ");
			if(zone.Length > 0 && zone != "AUTO")
			{
				xmlTVStart += zone;
			}
			else
			{
				xmlTVStart += startTime.ToString("zzz").Replace(":", "");
			}

			prog.SetAttribute("start", xmlTVStart);

			// Build Stop Time
			DateTime stop =  startTime.AddMinutes(duration);
			String xmlTVStop = stop.ToString("yyyyMMddHHmmss ");
			if(zone.Length > 0 && zone != "AUTO")
			{
				xmlTVStop += zone;
			}
			else
			{
				xmlTVStop += startTime.ToString("zzz").Replace(":", "");
			}
			prog.SetAttribute("stop", xmlTVStop);

			parent.AppendChild(prog);

			XmlElement titleEl = doc.CreateElement("title");
			titleEl.InnerText = title;
			prog.AppendChild(titleEl);

            if (subtitle.Length > 0)
            {
                XmlElement subtitleEl = doc.CreateElement("sub-title");
                subtitleEl.InnerText = subtitle;
                prog.AppendChild(subtitleEl);
            }

			XmlElement descEl = doc.CreateElement("desc");
			descEl.InnerText = description;
			prog.AppendChild(descEl);

            if (category.Length > 0)
            {
                XmlElement categoryEl = doc.CreateElement("category");
                categoryEl.InnerText = category;
                prog.AppendChild(categoryEl);
            }

            if (rating.Length > 0)
            {
                XmlElement ratingEl = doc.CreateElement("rating");
                ratingEl.SetAttribute("system", "ABA");
                XmlElement ratingElvalue = doc.CreateElement("value");
                ratingElvalue.InnerText = rating;
                ratingEl.AppendChild(ratingElvalue);
                prog.AppendChild(ratingEl);
            }

			XmlElement lenEl = doc.CreateElement("length");
			lenEl.SetAttribute("units", "minutes");
			lenEl.InnerText = duration.ToString();
			prog.AppendChild(lenEl);

			XmlElement catEl = doc.CreateElement("category");
			prog.AppendChild(catEl);

            if (detailsURL.Length > 0)
            {
                XmlElement urlEl = doc.CreateElement("url");
                urlEl.InnerText = detailsURL;
                prog.AppendChild(urlEl);
            }

			return true;
		}

		public String toString()
		{
			String data = "";

			data += "title      : " + title + "\r\n";
			data += "subtitle   : " + subtitle + "\r\n";
			data += "startTime  : " + startTime.ToString() + "\r\n";
			data += "duration   : " + duration + "\r\n";
			data += "channel    : " + channel + "\r\n";
			data += "rating     : " + rating + "\r\n";
			data += "description: " + description + "\r\n";
			data += "progID     : " + progID + "\r\n";
			data += "detailsURL : " + detailsURL + "\r\n";

			return data;
		}
	}
}
