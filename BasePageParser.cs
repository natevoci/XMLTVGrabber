using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web;

namespace XMLTVGrabber
{

	public class BasePageParser
	{
		ConfigLoader config = null;

		public BasePageParser(ConfigLoader conf)
		{
			config = conf;
		}

		public int parsePage(BaseUrlContainer baseURL, List<ProgramInfo> programs)
		{
			if(baseURL.getPageData().Length == 0)
				return 0;

			int count = 0;

			String basePageData = baseURL.getPageData().ToString();
			basePageData = basePageData.Replace("\r\n", "");
			basePageData = basePageData.Replace("\n", "");

			String baseItemRegEx = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/ItemRegEx");
			Console.WriteLine("Base Item RegEx:\n" + baseItemRegEx);

			String baseItemMatchOrder = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/ItemMatchOrder");
			Console.WriteLine("Base Item Match Order:" + baseItemMatchOrder);

			String baseItemTimeFormat = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/TimeFormat");
			Console.WriteLine("Base Item Time Format: " + baseItemTimeFormat);

            String channelId = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/ChannelID");

			Regex exp = new Regex(@baseItemRegEx, RegexOptions.IgnoreCase);

			MatchCollection matchList = exp.Matches(basePageData);

			for(int x = 0; x < matchList.Count; x++)
			{
				Match match = matchList[x];
				
				ProgramInfo info = new ProgramInfo();
                info.channel = HttpUtility.HtmlDecode(channelId);

				//Console.WriteLine(match.Value);

				for (int i = 1; i < match.Groups.Count; i++)
				{
					Group group = match.Groups[i];
					String groupItemData = group.Value.Trim();

					if(baseItemMatchOrder.Length >= i)
					{
						String actionChar = baseItemMatchOrder.Substring(i-1, 1);
						//Console.WriteLine("Doing group action: " + actionChar + "(" + groupItemData + ")");

						/*
						I = prog ID
						T = Time
						C = Channel Name
						N = Program Name (Title)
                        S = Subtitle
						L = Duration
						R = Rating
						D = Desription
                        G = Category / Genre
                        . = Ignore
						*/

						if(actionChar == "I")
						{
							info.progID = groupItemData;
						}
						else if(actionChar == "T")
						{
							info.startTime = parseStartDate(baseURL.Date.ToString("dd/MM/yyyy") + " " + groupItemData, "dd/MM/yyyy " + baseItemTimeFormat);
						}
						else if(actionChar == "C")
						{
							info.channel = HttpUtility.HtmlDecode(groupItemData);
						}
						else if(actionChar == "N")
						{
							info.title = HttpUtility.HtmlDecode(groupItemData);
						}
                        else if (actionChar == "S")
                        {
                            info.subtitle = HttpUtility.HtmlDecode(groupItemData);
                        }
                        else if (actionChar == "L")
						{
							info.duration = int.Parse(groupItemData);
						}
						else if(actionChar == "R")
						{
							info.rating = groupItemData;
						}
						else if(actionChar == "D")
						{
							info.description = HttpUtility.HtmlDecode(groupItemData);
                        }
                        else if(actionChar == "G")
                        {
                            info.category = groupItemData;
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
			try
			{
				CultureInfo cultureInfo = new CultureInfo("en-US", true);
				DateTime MyDateTime = DateTime.ParseExact(dateString, format, cultureInfo);
				return MyDateTime;
			}
			catch(Exception e)
			{
				Console.WriteLine("Date parsing failed!");
				Console.WriteLine("Format     : " + format);
				Console.WriteLine("DateString : " + dateString);
				throw e;
			}
		}

	}
}
