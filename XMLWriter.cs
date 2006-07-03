using System;
using System.Xml;
using System.IO;
using System.Collections;

namespace XMLTVGrabber
{

	public class XMLWriter
	{
		ConfigLoader config = null;

		public XMLWriter(ConfigLoader conf)
		{
			config = conf;
		}

		public int writeXMLTVFile(ArrayList programs)
		{
			int count = 0;

			String zone = config.getOption("/XMLTVGrabber_Config/XMLCreation/TimeZone");
			Console.WriteLine("Using Time Zone (" + zone + ")");

			String outDir = config.getOption("/XMLTVGrabber_Config/XMLCreation/OutputDir");
			Console.WriteLine("Saving to (" + outDir + ")");
			DirectoryInfo dirTest = new DirectoryInfo(outDir);
			if(dirTest.Exists == false)
				dirTest.Create();

			XmlDocument doc = new XmlDocument();

			XmlElement el = doc.CreateElement("tv");
			doc.AppendChild(el);

			// add channels
			addChannels(programs, doc, el);

			IEnumerator it = programs.GetEnumerator();

			while(it.MoveNext())
			{
				ProgramInfo info = (ProgramInfo)it.Current;
				info.addToXML(doc, el, zone);
				count++;
			}

			FileStream fsxml = new FileStream(outDir + "\\xmltv.xml", FileMode.Create, FileAccess.ReadWrite);
			doc.Save(fsxml);
            fsxml.Close();

			return count;
		}

		private int addChannels(ArrayList programs, XmlDocument doc, XmlElement parent)
		{
			int count = 0;
			ArrayList channelList = new ArrayList();

			IEnumerator it = programs.GetEnumerator();
			while(it.MoveNext())
			{
				ProgramInfo info = (ProgramInfo)it.Current;

				if(!channelList.Contains(info.channel))
				{
					channelList.Add(info.channel);
				}
			}

			it = channelList.GetEnumerator();
			while(it.MoveNext())
			{
				String chan = (String)it.Current;

				XmlElement channel = doc.CreateElement("channel");
				channel.SetAttribute("id", chan);

				XmlElement disp = doc.CreateElement("display-name");
				disp.InnerText = chan;

				channel.AppendChild(disp);
				parent.AppendChild(channel);

				count++;
			}

			return count;
		}
	}
}
