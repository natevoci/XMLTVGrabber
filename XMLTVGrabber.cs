using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Globalization;

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
			string commandPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
			commandPath = commandPath.Substring(0, commandPath.LastIndexOf("\\") + 1);
			
			ConfigLoader config = new ConfigLoader(commandPath + "config.xml");
			BasePageParser parser = new BasePageParser(config);
			
			ArrayList programs = new ArrayList();

			BaseUrlConstructor urlConstruct = new BaseUrlConstructor(config);

			BaseUrlContainer[] baseURLS = urlConstruct.getBaseURLS();

			// get download working dir
			String workingDir = config.getOption("/XMLTVGrabber_Config/DownloadOptions/WorkingDir");
            workingDir = workingDir.TrimEnd('\\');
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
				Console.WriteLine("(" + (x+1) + " of " + baseURLS.Length + ") " + baseURLS[x].URL);

				bool gotData = false;
				for(int tryCount = 0; tryCount < retryCount && !gotData; tryCount++)
				{
					IEWrapper ie = new IEWrapper();
					ie.setURL(baseURLS[x].URL);
					if(header.Length > 0)
					{
						Console.WriteLine("Setting IE request HEADER (" + header + ")");
						ie.setHeaders(header);
					}
					ie.setTimeOut(timout);

					Console.WriteLine("Getting Data, try (" + tryCount + ")");
					int result = ie.getData(baseURLS[x].getPageData());

					if(result == 0)
						gotData = true;
				}

                String filename = workingDir + "\\" + baseURLS[x].Date.ToString("yyyy-MM-dd") + " - Url " + baseURLS[x].PageId.ToString() + ".html";
                
                // if we still do not have the data
				if(gotData == false)
				{
                    // try for a cached file
					if (File.Exists(filename))
					{
                        Console.WriteLine("Reading data from cached file \"" + filename + "\"");
						StreamReader sr = new StreamReader(filename);
                        baseURLS[x].resetPageData();
						baseURLS[x].getPageData().Append(sr.ReadToEnd());
					}
                    // otherwise exit
					else
					{
						Console.WriteLine("No Data after " + retryCount + " tries so exiting");
						System.Environment.Exit(-1);
						return;
					}
				}

				int found = parser.parsePage(baseURLS[x], programs);
				totalCount += found;
				Console.WriteLine("Found and Added (" + found + ") programs.");

				if ((found > 0) && gotData)
					baseURLS[x].dumpPageData(filename);
			}

			Console.WriteLine("Found total of " + totalCount + " items.");

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

            //
            // Clean up old cached pages
            //
            Console.WriteLine("Cleaning up old cache files");

            String daysOffset = config.getOption("/XMLTVGrabber_Config/BaseUrl/DaysOffset");
            int offset = int.Parse(daysOffset);

            string[] files = Directory.GetFiles(workingDir);
            foreach (string filename in files)
            {
                if (!filename.StartsWith(workingDir))
                    continue;
                string file = filename.Substring(workingDir.Length).TrimStart('\\');

                int index = file.IndexOf(" - ");
                if (index < 0)
                    continue;
                string dateString = file.Substring(0, index);

                DateTime fileDate = DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime compareDate = DateTime.Now.AddDays((offset < 0) ? offset - 1 : -1);
                if (fileDate < compareDate)
                {
                    Console.WriteLine("  Deleting " + file);
                    File.Delete(file);
                }
            }
		}

	}
}
