using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace XMLTVGrabber
{

	public class BaseUrlConstructor
	{
		private ConfigLoader config = null;

		public BaseUrlConstructor(ConfigLoader conf)
		{
			config = conf;
		}

		public BaseUrlContainer[] getBaseURLS()
		{
			String loc = config.getOption("/XMLTVGrabber_Config/BaseUrl/Location");
			Console.WriteLine("Getting data for location (" + loc + ")");
			
			DateHolder[] dates = getDates();

			String[] baseUrlList = config.getOptionList("/XMLTVGrabber_Config/BaseUrl/URL");

			ArrayList constructedURLS = new ArrayList();

			for(int x = 0; x < baseUrlList.Length; x++)
			{
				String baseURL01 = baseUrlList[x];
				baseURL01 = baseURL01.Replace("(LOCATION)", loc);

				for(int y = 0; y < dates.Length; y++)
				{
					String baseURL02 = "";
					if ((dates[y].dateHASH != null) && (dates[y].dateHASH.Length > 0))
					{
						baseURL02 = baseURL01;
						baseURL02 = baseURL02.Replace("(DATESTRING)", dates[y].dateHASH);
					}

					BaseUrlContainer urlContainer = new BaseUrlContainer();
					urlContainer.URL = baseURL02;
					urlContainer.Date = dates[y].dateValue;
                    urlContainer.PageId = x;
					constructedURLS.Add(urlContainer);
				}
			}

			return (BaseUrlContainer[])constructedURLS.ToArray(typeof(BaseUrlContainer));
		}

		private DateHolder[] getDates()
		{
			String numDays = config.getOption("/XMLTVGrabber_Config/BaseUrl/NumberOfDays");
			Console.WriteLine("Getting (" + numDays + ") of data");
			int numberOfDays = int.Parse(numDays);

			String daysOffset = config.getOption("/XMLTVGrabber_Config/BaseUrl/DaysOffset");
			Console.WriteLine("Offsetting data grab by (" + daysOffset + ") days");
			int offset = int.Parse(daysOffset);

			DateTime startDate = DateTime.Now;
			startDate = startDate.Subtract(startDate.TimeOfDay);
			startDate = startDate.AddDays(offset);
			DateTime endDate = startDate.AddDays(numberOfDays);

			SortedList hashes = LoadDateHashes();
			ArrayList dateList = new ArrayList();

			while (startDate < endDate)
			{
				DateHolder holder = new DateHolder();
				holder.dateHASH = (String)hashes[startDate.ToString("yyyy-MM-dd")];
				holder.dateValue = startDate;

				dateList.Add(holder);
				startDate = startDate.AddDays(1);
			}

			return (DateHolder[])dateList.ToArray(typeof(DateHolder));
		}

		private SortedList LoadDateHashes()
		{
			SortedList hashes = new SortedList();

            String loc = config.getOption("/XMLTVGrabber_Config/BaseUrl/Location");

            String defaultSearchPage = config.getOption("/XMLTVGrabber_Config/BaseUrl/DefaultSearchPage");
            defaultSearchPage = defaultSearchPage.Replace("(LOCATION)", loc);

			IEWrapper ie = new IEWrapper();
            ie.setURL(defaultSearchPage);
			StringBuilder buff = new StringBuilder();
			ie.setTimeOut(120);
			int wsLoadResult = ie.getData(buff);
			//Console.WriteLine(buff.ToString());

			String formAction = config.getOption("/XMLTVGrabber_Config/BaseUrl/FormAction");

			String formTagRegex = "<form";

			// find any number of key=value pairs
			formTagRegex += "(?: \\w+=(?:\"[^\"]*\"|[^\\s>]+))*?";

			// find the action= tag that matches the formAction value
			formTagRegex += " action=(?:" + formAction + "|\"" + formAction + "\")";

			// find any number of key=value pairs
			formTagRegex += "(?: \\w+=(?:\"[^\"]*\"|[^\\s>]+))*?";

			// include all the text within the form tag in a group
			formTagRegex += ">(.*?)</form>";
			Console.WriteLine("Form RegEx: " + formTagRegex);

			Regex formRegEx = new Regex(formTagRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			Match formMatch = formRegEx.Match(buff.ToString());

			ArrayList dateList = new ArrayList();

			if ((formMatch != null) && (formMatch.Groups.Count > 1))
			{
				String text = formMatch.Groups[1].Value;

				String dateFormatRegRx = config.getOption("/XMLTVGrabber_Config/BaseUrl/DateFormateRegEx");
				Console.WriteLine("Date RegEx: " + dateFormatRegRx);

				String dateFormat = config.getOption("/XMLTVGrabber_Config/BaseUrl/DateFormat");
				Console.WriteLine("Date Format: " + dateFormat);

				Regex exp = new Regex(dateFormatRegRx, RegexOptions.IgnoreCase | RegexOptions.Singleline);
				MatchCollection matchList = exp.Matches(text);

				for (int x = 0; x < matchList.Count; x++)
				{
					Match match = matchList[x];

					//Console.WriteLine("MATCH = " + match.Value);
					//Console.WriteLine("GROUP COUNT = " + match.Groups.Count);

					if (match.Groups.Count == 3)
					{
						DateTime date = DateTime.ParseExact(match.Groups[2].Value, dateFormat, CultureInfo.InvariantCulture);
						hashes.Add(date.ToString("yyyy-MM-dd"), match.Groups[1].Value);
					}
				}
			}

			return hashes;
		}

	}
}
