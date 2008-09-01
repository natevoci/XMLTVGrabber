using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace XMLTVGrabber
{

	public class XMLWriter
	{
		ConfigLoader config = null;

		public XMLWriter(ConfigLoader conf)
		{
			config = conf;
		}

        public void writeCSVFile(List<ProgramInfo> programs)
        {
            String outFile = config.getOption("/XMLTVGrabber_Config/XMLCreation/CSVFile", true);
            if ((outFile == null) || (outFile.Trim().Length == 0))
                return;

            Console.WriteLine("Saving csv to (" + outFile + ")");
            FileInfo fi = new FileInfo(outFile);
            if (fi.Directory.Exists == false)
                fi.Directory.Create();

            System.Reflection.FieldInfo[] fieldInfos = typeof(ProgramInfo).GetFields();

            using (StreamWriter sw = new StreamWriter(outFile))
            {
                List<string> values = new List<string>();

                foreach (System.Reflection.FieldInfo fieldInfo in fieldInfos)
                {
                    values.Add("\"" + fieldInfo.Name + "\"");
                }
                sw.WriteLine(string.Join(",", values.ToArray()));

                foreach (ProgramInfo info in programs)
                {
                    values.Clear();

                    foreach (System.Reflection.FieldInfo fieldInfo in fieldInfos)
                    {
                        object obj = fieldInfo.GetValue(info);
                        if (obj != null)
                            values.Add("\"" + obj.ToString().Replace("\"", "\"\"") + "\"");
                        else
                            values.Add("\"null\"");
                    }

                    sw.WriteLine(string.Join(",", values.ToArray()));
                }
            }
        }

		public int writeXMLTVFile(List<ProgramInfo> programs)
		{
			int count = 0;

			String zone = config.getOption("/XMLTVGrabber_Config/XMLCreation/TimeZone");
			Console.WriteLine("Using Time Zone (" + zone + ")");

			String outFile = config.getOption("/XMLTVGrabber_Config/XMLCreation/OutputFile");
			Console.WriteLine("Saving to (" + outFile + ")");
			FileInfo fi = new FileInfo(outFile);
			if(fi.Directory.Exists == false)
				fi.Directory.Create();

			XmlDocument doc = new XmlDocument();

			XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
			doc.AppendChild(decl);

			XmlElement el = doc.CreateElement("tv");
			el.SetAttribute("generator-info-name", "XMLTVGrabber");
			doc.AppendChild(el);

			// add channels
			addChannels(programs, doc, el);

			foreach (ProgramInfo info in programs)
			{
				info.addToXML(doc, el, zone);
				count++;
			}

			FileStream fsxml = new FileStream(outFile, FileMode.Create, FileAccess.ReadWrite);
			doc.Save(fsxml);

			return count;
		}

		private int addChannels(List<ProgramInfo> programs, XmlDocument doc, XmlElement parent)
		{
			int count = 0;
			List<string> channelList = new List<string>();

			foreach (ProgramInfo info in programs)
			{
				if(!channelList.Contains(info.channel))
				{
					channelList.Add(info.channel);
				}
			}

			foreach (string chan in channelList)
			{
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
