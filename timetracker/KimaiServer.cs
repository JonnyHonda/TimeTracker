using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Org.Json;

namespace TimeTracker
{
    public class JsonClass
    {
        // {"jsonrpc":"2.0","method":"getActiveRecording","params":["49945438e826988f2e31d8972"],"id":"1"}
        public string jsonrpc { get; set; }
        public string method { get; set; }
        public List<string> @params { get; set; }
        public string id { get; set; }

    }


    public class KimaiServer
    {
        public string url { get; set; }
        private JsonClass Connection = new JsonClass();
        public string ContentType { get; set; }
        public bool RunningTimer { get; set; }
        public string JsonResultString { get; set; }
        public string ErrorString { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TimeTracker.KimaiServer"/> class.
        /// </summary>

        public KimaiServer()
        {
            Connection.jsonrpc = "2.0";
            Connection.id = "1";
            ContentType = "application/json";
        }
        public KimaiServer(string u)
        {
            url = u;
            Connection.jsonrpc = "2.0";
            Connection.id = "1";
            ContentType = "application/json";
        }

        /// <summary>
        /// Connect the specified method and parameters.
        /// </summary>
        /// <returns>The connect.</returns>
        /// <param name="method">Method.</param>
        /// <param name="parameters">Parameters.</param>
        public void ConnectAsync(string method, List<string> parameters)
        {
            Connection.method = method;
            Connection.@params = parameters;
            HttpClient oHttpClient = new HttpClient();
            oHttpClient.Timeout = new TimeSpan(0, 0, 15);
            HttpResponseMessage oHttpResponseMessage = new HttpResponseMessage();
            string JsonString = JsonConvert.SerializeObject(Connection);
            try
            {
                oHttpResponseMessage = oHttpClient.PostAsync(url, new StringContent(JsonString, Encoding.UTF8, ContentType)).Result;
                oHttpResponseMessage.EnsureSuccessStatusCode();
                string contents = oHttpResponseMessage.Content.ReadAsStringAsync().Result;
                JsonResultString = contents;
            }
            catch (ArgumentNullException ex)
            {
                ErrorString = ex.Message;
                throw (ex);
            }
        }
    }
}