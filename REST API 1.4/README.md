Please follow the following steps :

1.Input your web services user name and password: 
string USERNAME = "[WEB SERVICES USER NAME]";
string SECRET = "[WEB SERVICES PASSWORD]";

2.Input the right ENDPOINT:
private static String ENDPOINT = "https://api.omniture.com/admin/1.4/rest/";
		/*Make sure to point to the right data center 
		* Sanjose : api.omniture.com
		* Dallas : api2.omniture.com
		* London : api3.omniture.com
		* Singapore : api4.omniture.com
		* Portland : api5.omniture.com
		* */

		/*
		private static String ENDPOINT = "https://[INPUT THE RIGHTDATA CENTER SEE ABOVE]/admin/1.4/rest/";
		*/
		
3.modify the details of the method : requestJsonBuilder() :

		//Build the list of metrics to send with the request
		//i.e: listMetrics.Add(new Metrics() { id = "pageviews" });
		
		//Build the list of elements to send with the request
		//i.e : listElements.Add(new Elements() { id = "page", top = "25"});
		
		reportSuiteID = "[REPORT SUITE ID]",
		dateFrom = "[YYYY-MM-DD]",
		dateTo = "[YYYY-MM-DD]",
		metrics = listMetrics,
		elements = listElements