using System;
using System.Text;
using System.Collections;
using System.IO;

namespace XMLTVGrabber
{
	class XMLTVGrabber
	{
		[STAThread]
		static void Main(string[] args)
		{
			XMLTVGrabber runner = new XMLTVGrabber();
			runner.run();
		}
		
		public XMLTVGrabber()
		{

		}

		public void run()
		{
			ConfigLoader config = new ConfigLoader("config.xml");
			BasePageParser parser = new BasePageParser(config);
			
			ArrayList programs = new ArrayList();

			BaseUrlConstructor urlConstruct = new BaseUrlConstructor(config);

			BaseUrlContainer[] baseURLS = urlConstruct.getBaseURLS();

			// get download working dir
			String workingDir = config.getOption("/XMLTVGrabber_Config/DownloadOptions/WorkingDir");
			Console.WriteLine("Working Directory (" + workingDir + ")");
			DirectoryInfo workingDirTest = new DirectoryInfo(workingDir);
			if(workingDirTest.Exists == false)
				workingDirTest.Create();

			// get download timeout
			String timoutSetting = config.getOption("/XMLTVGrabber_Config/DownloadOptions/TimeOut");
			Console.WriteLine("Download Timeout set to (" + timoutSetting + ") seconds");
			int timout = int.Parse(timoutSetting);

			// get referer is any
			String referer = config.getOption("/XMLTVGrabber_Config/DownloadOptions/Referer");
			Console.WriteLine("Referer option (" + referer + ")");
			String header = "";
			if(referer.Length > 0)
				header = "Referer: " + referer;

			// retry count
			String retryCountString = config.getOption("/XMLTVGrabber_Config/DownloadOptions/RetryCount");
			Console.WriteLine("Will retry  (" + retryCountString + ") times");
			int retryCount = int.Parse(retryCountString);

			int totalCount = 0;

			for(int x = 0; x < baseURLS.Length; x++)
			{
				Console.WriteLine("");
				Console.WriteLine("(" + x + " of " + baseURLS.Length + ") " + baseURLS[x].getURL());

				bool gotData = false;
				for(int tryCount = 0; tryCount < retryCount && !gotData; tryCount++)
				{
					IEWrapper ie = new IEWrapper();
					ie.setURL(baseURLS[x].getURL());
					if(header.Length > 0)
					{
						Console.WriteLine("Setting IE request HEADER (" + header + ")");
						ie.setHeaders(header);
					}
					ie.setTimeOut(timout);

					Console.WriteLine("Getting Data, try " + tryCount);
					int result = ie.getData(baseURLS[x].getPageDate());

					if(result == 0)
					{
						baseURLS[x].dumpPageData(workingDir + "\\pageDump" + x + ".html");

						int found = parser.parsePage(baseURLS[x], programs);
						totalCount += found;
						Console.WriteLine("Found and Added (" + found + ") programs.");
						if(found > 0)
							gotData = true;
					}
				}

				// if we still do not have the data then exit
				if(gotData == false)
				{
					Console.WriteLine("No Data after " + retryCount + " tries so exiting");
					System.Environment.Exit(-1);
					break;
				}
			}

			Console.WriteLine("\nFound total of " + totalCount + " items.");

			XMLWriter writer = new XMLWriter(config);
			writer.writeXMLTVFile(programs);

			//
			// Now reload WS if needed
			//
			String doReload = config.getOption("/XMLTVGrabber_Config/WebScheudler/DoReload");
			Console.WriteLine("Do WS Reload (" + doReload + ")");

			if(doReload.ToUpper() == "TRUE")
			{
				String wsURL = config.getOption("/XMLTVGrabber_Config/WebScheudler/URL");
				Console.WriteLine("WS Reload URL (" + wsURL + ")");

				StringBuilder buff = new StringBuilder();
				IEWrapper ieWS = new IEWrapper();
				ieWS.setURL(wsURL);
				ieWS.setTimeOut(120);
				int wsLoadResult = ieWS.getData(buff);

				Console.WriteLine("WS reloaded with result : " + wsLoadResult);
			}
		}

	}
}
