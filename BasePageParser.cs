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

            int timeRollover = Int32.Parse(config.getOption("/XMLTVGrabber_Config/ParseBasePageInfo/TimeRollover"));
            Console.WriteLine("Base Item Time Rollover: " + timeRollover.ToString());

			Regex exp = new Regex(@baseItemRegEx, RegexOptions.IgnoreCase);

			MatchCollection matchList = exp.Matches(basePageData);

			for(int x = 0; x < matchList.Count; x++)
			{
				Match match = matchList[x];
				
				ProgramInfo info = new ProgramInfo();
                info.channel = HttpUtility.HtmlDecode(channelId);

                string timeString = "";
                string timeOfDay = "";

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
                        P = Previously Shown
                        U = Url
                        . = Ignore
						*/

						if(actionChar == "I")
						{
							info.progID = groupItemData;
						}
						else if(actionChar == "T")
						{
                            timeString = FixTime(groupItemData);
                        }
                        else if (actionChar == "A")
                        {
                            timeOfDay = groupItemData;
                        }
                        else if (actionChar == "C")
                        {
                            info.channel = HttpUtility.HtmlDecode(groupItemData);
                        }
                        else if (actionChar == "N")
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
                        else if (actionChar == "R")
                        {
                            info.rating = groupItemData;
                        }
                        else if (actionChar == "D")
                        {
                            info.description = HttpUtility.HtmlDecode(groupItemData);
                        }
                        else if (actionChar == "G")
                        {
                            info.category = groupItemData;
                        }
                        else if (actionChar == "P")
                        {
                            info.previouslyShown = groupItemData.Length > 0;
                        }
                        else if (actionChar == "U")
                        {
                            info.detailsURL = groupItemData;
                        }
                    }
				}

                if (timeString.Length > 0)
                {
                    info.startTime = parseStartDate(baseURL.Date.ToString("dd/MM/yyyy") + " " + timeString, "dd/MM/yyyy " + baseItemTimeFormat);

                    if (timeOfDay.ToLower() == "morning")
                    {
                        if (info.startTime.Hour >= 12)
                            info.startTime = info.startTime.AddHours(-12.0);
                    }
                    else if (timeOfDay.ToLower() == "afternoon")
                    {
                        if (info.startTime.Hour < 12)
                            info.startTime = info.startTime.AddHours(12.0);
                    }
                    else if (timeOfDay.ToLower() == "evening")
                    {
                        if (info.startTime.Hour < 12)
                            info.startTime = info.startTime.AddHours(12.0);
                    }
                    else if (timeOfDay.ToLower() == "later")
                    {
                        if (info.startTime.Hour >= 12)
                            info.startTime = info.startTime.AddHours(-12.0);
                        info.startTime = info.startTime.AddDays(1.0);
                    }
                    else
                    {
                        // if it's before the rollover time (eg. 5am) then it belongs to the next day.
                        if (info.startTime.TimeOfDay.Hours < timeRollover)
                            info.startTime = info.startTime.AddDays(1.0);
                    }
                }

				//Console.WriteLine(info.toString());
				programs.Add(info);
				count++;

				//Console.WriteLine("\r\n");
			}


			return count;
		}

        private string FixTime(string timeString)
        {
            Regex regex = new Regex(@"(\d{1,2}):(\d{1,2})", RegexOptions.IgnoreCase);
            Match match = regex.Match(timeString);
            if (match.Success == false)
                throw new FormatException("Date format is incorrect.");

            int hours = Int32.Parse(match.Groups[1].Value);
            int minutes = Int32.Parse(match.Groups[2].Value);

            while (hours >= 24)
                hours -= 24;

            string timeStr = hours.ToString("00") + ":" + minutes.ToString("00");

            return timeStr;
        }

		private DateTime parseStartDate(String dateString, String format)
		{
			try
			{
                CultureInfo cultureInfo = new CultureInfo("en-US", true);
                DateTime myDateTime = DateTime.ParseExact(dateString, format, cultureInfo);
                return myDateTime;
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
