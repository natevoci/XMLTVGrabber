The IE Form info for the project comes from:

http://www.codeproject.com/csharp/webbrowser.asp

"C:\Program Files\Microsoft Visual Studio .NET 2003\SDK\v1.1\Bin\aximp" c:\windows\system32\shdocvw.dll
"C:\Program Files\Microsoft Visual Studio .NET 2003\SDK\v1.1\Bin\tlbimp" mshtml.tlb

and was implemented by nate.



History
-------
Version (first release)

- added a download working dir default to pages
- added the WS reload action, edit the config.xml to enable, you may have to change the WS URL also
- added some better error and time out checking, if a page failes for Num retries then exit
- added if an option could not be loaded from config exit.
- added auto create of output and working dir is they do not exist

Version (1.0.0.18551)

- fixed a bug with the rating tag
- simplified version, yes this is the latest version

Version (1.0.0.23168)

- HTML decode the text data before writing the XML file, stops thing like & showing up in grabbed data.

Version (1.1.0.17770)

- Use Nate's approach to the IE wrapping, better, faster, stronger 

Version (1.2.0.27512)

- Fix for new date obfuscation

Version (1.2.0.24421)

- Added page caching. If no data is found in the downloaded page it will use a cached version. (nate)
- Re-implemented NumberOfDays and DaysOffset. (nate)
- Added regex to search for dates only in the correct FORM of the page. (nate)
- Fix so that config.xml is read from the same directory as the .exe rather than the current path (nate)

Version (1.2.1.3046)

- Made cached files get used if the downloaded page doesn't contain any programs
- Changed getDates to return all the dates in the range (offset to offset+numberOfDays).
  If no hash is found for a date then the url is left blank. This allows cached pages to be used if they exist.
- Added OutputFile option to config.xml
- Added try catch block around everything to make sure all exceptions get written to the console.

Version (1.2.2.2900)

- Added option to config.xml to specify the default search page.
- Added the ability to use (LOCATION) in the default search page and the referer.
- Fixed a bug of not closing a file handles.
