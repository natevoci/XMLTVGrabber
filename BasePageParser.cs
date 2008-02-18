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

            List<ProgramInfo> newPrograms = new List<ProgramInfo>();

			int count = 0;

			String basePageData = baseURL.getPageData().ToString();
			basePageData = basePageData.Replace("\r\n", "");
			basePageData = basePageData.Replace("\n", "");

            String baseDateRegEx = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/DateRegEx");
            Console.WriteLine("Base Date RegEx:\n" + baseDateRegEx);

			String baseDateFormat = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/DateFormat");
			Console.WriteLine("Base Date Format: " + baseDateFormat);

            String baseItemRegEx = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/ItemRegEx");
			Console.WriteLine("Base Item RegEx:\n" + baseItemRegEx);

			String baseItemMatchOrder = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/ItemMatchOrder");
			Console.WriteLine("Base Item Match Order:" + baseItemMatchOrder);

			String baseItemTimeFormat = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/TimeFormat");
			Console.WriteLine("Base Item Time Format: " + baseItemTimeFormat);

            String baseChannelRegEx = config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/ChannelRegEx");
            Console.WriteLine("Base Channel RegEx: " + baseChannelRegEx);

            Regex dateExp = new Regex(@baseDateRegEx, RegexOptions.IgnoreCase);
			Regex itemExp = new Regex(@baseItemRegEx, RegexOptions.IgnoreCase);
            Regex channelExp = new Regex(@baseChannelRegEx, RegexOptions.IgnoreCase);

            MatchCollection dateMatchList = dateExp.Matches(basePageData);
            MatchCollection itemMatchList = itemExp.Matches(basePageData);
            MatchCollection channelMatchList = channelExp.Matches(basePageData);

            int dateIndex = 0;
            int channelIndex = 0;

            Match channelMatch = channelMatchList[channelIndex];
            String channelId = HttpUtility.HtmlDecode(channelMatch.Groups[1].Value).Trim();

            DateTime latestDate = DateTime.Now;

			for(int x = 0; x < itemMatchList.Count; x++)
			{
				Match match = itemMatchList[x];

                if (dateIndex < dateMatchList.Count)
                {
                    Match dateMatch = dateMatchList[dateIndex];
                    if (dateMatch.Index < match.Index)
                    {
                        string dateString = HttpUtility.HtmlDecode(dateMatch.Groups[1].Value).Trim();
                        // for some reason HtmlDecode turns &nbsp; into char 160 instead of 32
                        dateString = dateString.Replace((char)160, (char)32);
                        latestDate = ParseDate(dateString, baseDateFormat);
                        dateIndex++;
                    }
                }

				ProgramInfo info = new ProgramInfo();
                info.channel = HttpUtility.HtmlDecode(channelId);


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
                            info.startTime = ParseDate(latestDate.ToString("dd/MM/yyyy") + " " + groupItemData, "dd/MM/yyyy " + baseItemTimeFormat);
                            // if it's before 4 am then it belongs to the next day.
                            if (info.startTime.TimeOfDay.Hours < 4)
                                info.startTime = info.startTime.AddHours(24.0);
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
                int insertAt = newPrograms.Count;
                while (insertAt > 0)
                {
                    ProgramInfo prevInfo = newPrograms[insertAt - 1];
                    if ((info.startTime == prevInfo.startTime) && (info.title == prevInfo.title))
                    {
                        insertAt = -1;
                        break;
                    }
                    if (info.startTime >= prevInfo.startTime)
                        break;
                    insertAt--;
                }

                if (insertAt >= newPrograms.Count)
                    newPrograms.Add(info);
                else if (insertAt >= 0)
                    newPrograms.Insert(insertAt, info);

				count++;

				//Console.WriteLine("\r\n");
			}

            programs.AddRange(newPrograms);

			return count;
		}

		private DateTime ParseDate(String dateString, String format)
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
