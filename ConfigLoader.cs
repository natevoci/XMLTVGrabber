using System;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Specialized;
using System.Collections;
using System.Configuration;

namespace XMLTVGrabber
{
	public class ConfigLoader
	{
		XmlDocument doc = null;

		public ConfigLoader(String name)
		{
			try
			{
				doc = new XmlDocument();
				doc.Load(name);
			}
			catch(Exception e)
			{
				Console.WriteLine("Config File Load Error: \n\n" + e.ToString());
				System.Environment.Exit(-1);
			}
		}

		public String[] getOptionList(String option)
		{
			ArrayList optionList = new ArrayList();

			try
			{
				XPathNavigator nav = doc.CreateNavigator();

				XPathExpression expr; 
				expr = nav.Compile(option);
				XPathNodeIterator iterator = nav.Select(expr);

				while (iterator.MoveNext())
				{
					XPathNavigator nav2 = iterator.Current.Clone();
					optionList.Add(nav2.Value);
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("Config Option not found: \n\n" + e.ToString());
				System.Environment.Exit(-1);
			}

			String[] options = (String[])optionList.ToArray(typeof(String));

			if(options.Length == 0)
			{
				Console.WriteLine("Config Option Group (" + option + ") not found!");
				System.Environment.Exit(-1);	
			}

			return options;
		}

        public String getOption(String option)
        {
            return getOption(option, false);
        }

		public String getOption(String option, bool returnNullForNotExists)
		{
			String optionData = "";

			try
			{
				XPathNavigator nav = doc.CreateNavigator();

				XPathExpression expr = nav.Compile(option);
				XPathNodeIterator iterator = nav.Select(expr);


				bool hasnext = iterator.MoveNext();
				if(!hasnext)
				{
                    if (returnNullForNotExists)
                        return null;
					Console.WriteLine("Config Option (" + option + ") not found!");
					System.Environment.Exit(-1);					
				}

				optionData = iterator.Current.Value;

				if(optionData == null)
					optionData = "";
			}
			catch(Exception e)
			{
                if (returnNullForNotExists)
                    return null;
                Console.WriteLine("Config Option error: \n\n" + e.ToString());
				System.Environment.Exit(-1);
			}

			return optionData;
		}
	}
}
