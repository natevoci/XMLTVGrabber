using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;

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
					String baseURL02 = baseURL01;
					baseURL02 = baseURL02.Replace("(DATESTRING)", dates[y].dateHASH);

					BaseUrlContainer urlContainer = new BaseUrlContainer();
					urlContainer.setURL(baseURL02);
					urlContainer.setDate(dates[y].dateString);
					constructedURLS.Add(urlContainer);
				}
			}

			return (BaseUrlContainer[])constructedURLS.ToArray(typeof(BaseUrlContainer));
		}

		private DateHolder[] getDates()
		{
			IEWrapper ie = new IEWrapper();
			ie.setURL("http://tvguide.ninemsn.com.au/search/default.asp");
			StringBuilder buff = new StringBuilder();
			ie.setTimeOut(120);
			int wsLoadResult = ie.getData(buff);
			//Console.WriteLine(buff.ToString());

			String dateFormatRegRx = config.getOption("/XMLTVGrabber_Config/BaseUrl/DateFormateRegEx");
			Console.WriteLine("Date RegEx: " + dateFormatRegRx);

			ArrayList dateList = new ArrayList();

			Regex exp = new Regex(dateFormatRegRx, RegexOptions.IgnoreCase);
			MatchCollection matchList = exp.Matches(buff.ToString());

			for(int x = 0; x < matchList.Count; x++)
			{
				Match match = matchList[x];
				ProgramInfo info = new ProgramInfo();

				//Console.WriteLine("MATCH = " + match.Value);
				//Console.WriteLine("GROUP COUNT = " + match.Groups.Count);

				if(match.Groups.Count == 3)
				{
					DateHolder holder = new DateHolder();

					Group group = match.Groups[1];
					holder.dateHASH = group.Value;

					group = match.Groups[2];
					holder.dateString = group.Value;

					if(holderContainsDate(dateList, holder) == false)
						dateList.Add(holder);
				}
			}

			return (DateHolder[])dateList.ToArray(typeof(DateHolder));
		}

		bool holderContainsDate(ArrayList dateList, DateHolder holder)
		{
			IEnumerator emun = dateList.GetEnumerator();

			while(emun.MoveNext())
			{
				if(((DateHolder)emun.Current).dateHASH == holder.dateHASH)
					return true;
			}

			return false;
		}
	}
}
