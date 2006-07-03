using System;
using System.Collections;

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
			
			String[] dates = getDates();

			String[] baseUrlList = config.getOptionList("/XMLTVGrabber_Config/BaseUrl/URL");

			ArrayList constructedURLS = new ArrayList();

			for(int x = 0; x < baseUrlList.Length; x++)
			{
				String baseURL01 = baseUrlList[x];
				baseURL01 = baseURL01.Replace("(LOCATION)", loc);

				for(int y = 0; y < dates.Length; y++)
				{
					String baseURL02 = baseURL01;
					baseURL02 = baseURL02.Replace("(DATESTRING)", dates[y]);

					BaseUrlContainer urlContainer = new BaseUrlContainer();
					urlContainer.setURL(baseURL02);
					urlContainer.setDate(dates[y]);
					constructedURLS.Add(urlContainer);
				}
			}

			return (BaseUrlContainer[])constructedURLS.ToArray(typeof(BaseUrlContainer));
		}

		private String[] getDates()
		{

			String numDays = config.getOption("/XMLTVGrabber_Config/BaseUrl/NumberOFDays");
			Console.WriteLine("Getting (" + numDays + ") of data");
			int number = int.Parse(numDays);

			String daysOffset = config.getOption("/XMLTVGrabber_Config/BaseUrl/DaysOffset");
			Console.WriteLine("Offsetting data grab by (" + daysOffset + ") days");
			int offset = int.Parse(daysOffset);

			String dateFormat = config.getOption("/XMLTVGrabber_Config/BaseUrl/DateFormate");
			Console.WriteLine("Using base URL Date Format: " + dateFormat);

			DateTime now = DateTime.Now;
			now = now.AddDays(offset);

			ArrayList dateList = new ArrayList();

			for(int x = 0; x < number; x++)
			{
				dateList.Add(now.ToString(dateFormat));
				now = now.AddDays(1);
			}

			return (String[])dateList.ToArray(typeof(String));
		}
	}
}
