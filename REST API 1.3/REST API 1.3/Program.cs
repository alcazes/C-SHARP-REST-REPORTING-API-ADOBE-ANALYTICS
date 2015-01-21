using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Web.Script.Serialization;


namespace REST_API_1._3
{
    class Program
    {
        string USERNAME = "[WEB SERVICES USER NAME]";
        string SECRET = "[WEB SERVICES PASSWORD]";
        /*Make sure to point to the right data center 
	    * Sanjose : api.omniture.com
	    * Dallas : api2.omniture.com
	    * London : api3.omniture.com
	    * Singapore : api4.omniture.com
	    * Portland : api5.omniture.com
	    * */

        /*
        private static String ENDPOINT = "https://[INPUT THE RIGHTDATA CENTER SEE ABOVE]/admin/1.3/rest/";
        */

        //Change the ENDPOINT IF NEEDED
        private static String ENDPOINT = "https://api.omniture.com/admin/1.3/rest/";

        static void Main(string[] args)
        {
            string response = "";
            Console.WriteLine("Adobe Analytics Rest API 1.3");

            /*1.Build the json to send with the call to Report.QueueRanked. Remember to modify the settings in requestJsonBuilder()*/
            Program prog = new Program();
            string json = prog.requestJsonBuilder();
            /*2.Send the request to create the Queue Ranekd report. A Report ID should be returned*/
            response = prog.callMethod("Report.QueueRanked", json);
            Console.WriteLine("Status of the queued request : " + response);
            var jss = new JavaScriptSerializer();
            var requestDetails = jss.Deserialize<Dictionary<string, string>>(response);
            /*3.If the report has bee queued successfully - continue*/
            if (requestDetails["status"] == "queued")
            {
                var reportIdQueued = requestDetails["reportID"];
                Console.WriteLine("Your report has been queued successfully. The report ID is : " + reportIdQueued);
                Console.WriteLine("Checking status of the processing of the report");
                response = "";
                /*4.Buid the json to send with the call Report.GetStatus*/
                string id = prog.requestJsonBuilderStatus(reportIdQueued);
                bool done = false;
                bool error = false;
                int it = 1;
                //While the report if not finished processing or processing error continue checking status
                while (!done && !error)
                {
                    try
                    {
                        Console.WriteLine("Start checking status. Iteration : " + it);
                        /*5.Send the request to check the processing status of the report ID.*/
                        response = prog.callMethod("Report.GetStatus", id);
                        var requestDetailsStatus = jss.Deserialize<Dictionary<string, string>>(response);
                        requestDetailsStatus = jss.Deserialize<Dictionary<string, string>>(response);
                        //If processing completed successfully stop checking process
                        if (requestDetailsStatus["status"] == "done")
                        {
                            done = true;
                            Console.WriteLine("Checking status done");
                        }
                        //If processing error, stop checking
                        else if (requestDetailsStatus["status"] == "failed" || requestDetailsStatus["status"] == "error")
                        {
                            error = true;
                            Console.WriteLine("Checking status failed or error");
                        }
                      
                    }
                    catch (Exception ex) { error = true; throw new Exception(ex.Message); }
                    it ++;
                }

                //The report has not been processed successfully
                if (error)
                {
                    Console.WriteLine("The processing of the report failed");
                }
                /*6.Processing of the report is successful. Send the request to return the report data*/
                else
                {
                    Console.WriteLine("The report is ready. Requesting the processed report");
                    try
                    {

                        response = prog.callMethod("Report.GetReport", id);

                        
                        Console.WriteLine("Please find your data below");
                        Console.WriteLine(response);

                    }
                    catch (Exception ex) { throw new Exception(ex.Message); }
                }
                // Keep the console window open in debug mode.
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();

            }
        }

        /*Build the json for the method Report.QueueRanked*/
        string requestJsonBuilder()
        {
            //Build the list of metrics to send with the request
            //i.e: listMetrics.Add(new Metrics() { id = "pageviews" });
            var listMetrics = new List<Metrics>();
            listMetrics.Add(new Metrics() { id = "[METRIC]" });
            //Build the list of elements to send with the request
            //i.e : listElements.Add(new Elements() { id = "page", top = "25"});
            var listElements = new List<Elements>();
            listElements.Add(new Elements() { id = "[ELEMENT]", top = "[NUMBER]"});


            var serializer2 = new JavaScriptSerializer();
            Dictionary<string, RankedRequest> dic = new Dictionary<string, RankedRequest>();
            dic.Add("reportDescription", new RankedRequest()
            {
                reportSuiteID = "[REPORT SUITE ID]",
                dateFrom = "[YYYY-MM-DD]",
                dateTo = "[YYYY-MM-DD]",
                metrics = listMetrics,
                elements = listElements
            });
            var serializedResult2 = serializer2.Serialize(dic);
            return serializedResult2;
            
        }

        /*Build the json for the methods Report.GetStatus and Report.GetReport*/
        string requestJsonBuilderStatus(string id)
        {
            ReportID json = new ReportID() { reportID = id }; 
            
            var serializer = new JavaScriptSerializer();
            var serializedResult = serializer.Serialize(json);
            return serializedResult;
        }

        /*Build the rest call to the Adobe Analytics REST APII 1.3*/
        public String callMethod(String method, String data)
        {
            Program prog = new Program();
            HttpWebResponse statusResponse = null;
            string responseXml = "";
            StringBuilder sbUrl = new StringBuilder(ENDPOINT + "?method=" + method);
            HttpWebRequest omniRequest = (HttpWebRequest)WebRequest.Create(sbUrl.ToString());
            string timecreated = generateTimestamp();
            string nonce = generateNonce();
            string digest = getBase64Digest(nonce + timecreated + SECRET);
            nonce = base64Encode(nonce);
            omniRequest.Headers.Add("X-WSSE: UsernameToken Username=\"" + USERNAME + "\", PasswordDigest=\"" + digest + "\", Nonce=\"" + nonce + "\", Created=\"" + timecreated + "\"");
            omniRequest.Method = "POST";
            omniRequest.ContentType = "text/json";
            //Right the json details to the request
            using (var streamWriter = new StreamWriter(omniRequest.GetRequestStream()))
            {
                string json = data;
                Console.WriteLine("Json request :" + json);
                streamWriter.Write(json);
            }
            //Get the response of the request
            try
            {

                statusResponse = (HttpWebResponse)omniRequest.GetResponse();
                using (Stream receiveStream = statusResponse.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        responseXml = readStream.ReadToEnd();
                        return responseXml;
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        //string restRequest()
        //{
        //    string USERNAME = "obellot:Cardgage Corp";
        //    string SECRET = "4c66d0ba06757b5c1791db69699922d2";
        //    string ENDPOINT = "https://api.omniture.com/admin/1.3/rest/";
        //    HttpWebResponse statusResponse = null;
        //    string responseXml = "";
        //    StringBuilder sbUrl = new StringBuilder(ENDPOINT + "?method=Company.GetTokenCount");
        //    HttpWebRequest omniRequest = (HttpWebRequest)WebRequest.Create(sbUrl.ToString());
        //    string timecreated = generateTimestamp();
        //    string nonce = generateNonce();
        //    string digest = getBase64Digest(nonce + timecreated + SECRET);
        //    nonce = base64Encode(nonce);
        //    omniRequest.Headers.Add("X-WSSE: UsernameToken Username=\"" + USERNAME + "\", PasswordDigest=\"" + digest + "\", Nonce=\"" + nonce + "\", Created=\"" + timecreated + "\"");
        //    omniRequest.Method = "POST";
        //    omniRequest.ContentType = "application/x-www-form-urlencoded";
        //    try
        //    {
        //        statusResponse = (HttpWebResponse)omniRequest.GetResponse();
        //        using (Stream receiveStream = statusResponse.GetResponseStream())
        //        {
        //            using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
        //            {
        //                responseXml = readStream.ReadToEnd();
        //            }
        //        }
        //    }
        //    catch (Exception ex) { throw new Exception(ex.Message); }
        //    return responseXml;
        //}


        /*** Here are the private functions ***/
        // Encrypting passwords with SHA1 in .NET and Java 
        // http://authors.aspalliance.com/thycotic/articles/view.aspx?id=2 
        private string getBase64Digest(string input)
        {
            SHA1 sha = new SHA1Managed();
            ASCIIEncoding ae = new ASCIIEncoding();
            byte[] data = ae.GetBytes(input);
            byte[] digest = sha.ComputeHash(data);
            return Convert.ToBase64String(digest);
        }
        // generate random nonce 
        private string generateNonce()
        {
            Random random = new Random();
            int len = 24;
            string chars = "0123456789abcdef";
            string nonce = "";
            for (int i = 0; i < len; i++)
            {
                nonce += chars.Substring(Convert.ToInt32(Math.Floor(random.NextDouble() * chars.Length)), 1);
            }
            return nonce;
        }
        // Time stamp in UTC string 
        private string generateTimestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        // C#-Base64 Encoding 
        // http://www.vbforums.com/showthread.php?t=287324 
        public string base64Encode(string data)
        {
            byte[] encData_byte = new byte[data.Length];
            encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
            string encodedData = Convert.ToBase64String(encData_byte);
            return encodedData;
        }


    }


    //Classes tp help build the json elements
    //These classes will need to amended depending of the json structure that you want to send. For a full json
    //default request go to https://marketing.adobe.com/developer/api-explorer
    public class Metrics
    {
        public string id { get; set; }
    }

    public class Elements
    {
        public string id { get; set; }
        public string top { get; set; }
    }

    public class RankedRequest
    {
        public string reportSuiteID { get; set; }
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public List<Metrics> metrics { get; set; }
        public List<Elements> elements { get; set; }

    }

    public class ReportID
    {
        public string reportID { get; set; }


    }

}
