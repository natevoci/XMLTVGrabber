using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;

namespace XMLTVGrabber
{

	public class BasePageParser
	{
		ConfigLoader config = null;

		public BasePageParser(ConfigLoader conf)
		{
			config = conf;
		}

		public int parsePage(BaseUrlContainer baseURL, ArrayList programs)
		{
			if(baseURL.getPageDate().Length == 0)
				return 0;

			int count = 0;

			String basePageData = baseURL.getPageDate().ToString();
			basePageData = basePageData.Replace("\r\n", "");
			basePageData = basePageData.Replace("\n", "");

			String baseItemRegEx = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/ItemRegEx");
			Console.WriteLine("Base Item RegEx:\n" + baseItemRegEx);

			String baseItemMatchOrder = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/ItemMatchOrder");
			Console.WriteLine("Base Item Match Order:" + baseItemMatchOrder);

			String baseItemTimeFormat = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/TimeFormat");
			Console.WriteLine("Base Item Time Format: " + baseItemTimeFormat);

			String detailsURL = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/DetailsURL");
			Console.WriteLine("Details URL: " + detailsURL);

			Regex exp = new Regex(@baseItemRegEx, RegexOptions.IgnoreCase);

			MatchCollection matchList = exp.Matches(basePageData);

			for(int x = 0; x < matchList.Count; x++)
			{
				Match match = matchList[x];
				
				ProgramInfo info = new ProgramInfo();

				//Console.WriteLine(match.Value);

				for (int i = 1; i < match.Groups.Count; i++)
				{
					Group group = match.Groups[i];
					String groupItemData = group.Value;

					if(baseItemMatchOrder.Length >= i)
					{
						String actionChar = baseItemMatchOrder.Substring(i-1, 1);
						//Console.WriteLine("Doing group action: " + actionChar + "(" + groupItemData + ")");

						/*
						I = prog ID
						T = Time
						C = Channel Name
						N = Program Name
						L = Duration
						R = Rating
						D = Desription
						*/

						if(actionChar == "I")
						{
							info.progID = groupItemData;
							info.detailsURL = detailsURL.Replace("(PID)", groupItemData);
						}
						else if(actionChar == "T")
						{
							info.startTime = parseStartDate(baseURL.getDate() + " " + groupItemData, baseItemTimeFormat);
						}
						else if(actionChar == "C")
						{
							info.channel = groupItemData;
						}
						else if(actionChar == "N")
						{
							info.title = groupItemData;
						}
						else if(actionChar == "L")
						{
							info.duration = int.Parse(groupItemData);
						}
						else if(actionChar == "R")
						{
							info.rating = groupItemData;
						}
						else if(actionChar == "D")
						{
							info.description = groupItemData;
						}
					}
				}

				//Console.WriteLine(info.toString());
				programs.Add(info);
				count++;

				//Console.WriteLine("\r\n");
			}


			return count;
		}

		private DateTime parseStartDate(String dateString, String format)
		{
			CultureInfo cultureInfo = new CultureInfo("en-AU");
			DateTime MyDateTime = DateTime.ParseExact(dateString, format, cultureInfo);
			return MyDateTime;
		}

	}
}
