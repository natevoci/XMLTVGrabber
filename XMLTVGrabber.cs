using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
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

            List<ProgramInfo> programs = new List<ProgramInfo>();

			BaseUrlConstructor urlConstruct = new BaseUrlConstructor(config);

			List<BaseUrlContainer> baseURLS = urlConstruct.getBaseURLS();

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

            // reuse hours count
            String reuseHoursCountString = config.getOption("/XMLTVGrabber_Config/DownloadOptions/WorkingDirHoursToReuse");
            Console.WriteLine("Will reuse cached pages if less than (" + retryCountString + ") hours old");
            int reuseHoursCount = int.Parse(reuseHoursCountString);

			int totalCount = 0;

			for(int x = 0; x < baseURLS.Count; x++)
			{
				Console.WriteLine("");
				Console.WriteLine("(" + (x+1) + " of " + baseURLS.Count + ") " + baseURLS[x].getURL());

				bool gotData = false;
				for(int tryCount = 0; tryCount < retryCount && !gotData; tryCount++)
				{
                    int result = -1;
                    string dumpFile = workingDir + "\\pageDump" + x + ".html";
                    FileInfo fi = new FileInfo(dumpFile);
                    if (fi.Exists && (fi.LastWriteTime.AddHours(reuseHoursCount) > DateTime.Now))
                    {
                        StreamReader sr = new StreamReader(dumpFile);
                        baseURLS[x].getPageData().Append(sr.ReadToEnd());
                        sr.Close();
                        result = 0;
                    }
                    else
                    {
                        IEWrapper ie = new IEWrapper();
                        ie.setURL(baseURLS[x].getURL());
                        if (header.Length > 0)
                        {
                            Console.WriteLine("Setting IE request HEADER (" + header + ")");
                            ie.setHeaders(header);
                        }
                        ie.setTimeOut(timout);

                        Console.WriteLine("Getting Data, try " + tryCount);
                        result = ie.getData(baseURLS[x].getPageData());

                        if (result == 0)
                        {
                            baseURLS[x].dumpPageData(dumpFile);

                        }
                    }

                    if (result == 0)
                    {
                        int found = parser.parsePage(baseURLS[x], programs);
                        totalCount += found;
                        Console.WriteLine("Found and Added (" + found + ") programs.");
                        if (found > 0)
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

            CheckFields(programs);

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


        public void CheckFields(List<ProgramInfo> programs)
        {
            for (int index=0 ; index<programs.Count ; index++ )
            {
                ProgramInfo prog = programs[index];
                if (prog.duration == 0)
                {
                    int offset = 1;
                    while (index+offset < programs.Count)
                    {
                        ProgramInfo nextProg = programs[index + offset];
                        if ((nextProg.channel != prog.channel) || (nextProg.startTime == prog.startTime))
                        {
                            offset++;
                            continue;
                        }

                        TimeSpan span = nextProg.startTime.Subtract(prog.startTime);
                        prog.duration = (int)span.TotalMinutes;
                        break;
                    }
                }
            }
        }
	}
}
