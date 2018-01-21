using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using TimeTracker.kimai.tsgapis.com;
using System.Xml;
using System;

namespace TimeTracker
{
    [Activity(Label = "TimeTracker", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        public string strEntryId;
        public string strProjectID;
        public string strActivityID;
        public bool startButtonState;
        public bool stopButtonState;

        //string apiKey;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            Button startbutton = FindViewById<Button>(Resource.Id.btn_start);
            Button stopbutton = FindViewById<Button>(Resource.Id.btn_stop);
            // Fetch App Prefs
            Context mContext = Android.App.Application.Context;
            AppPreferences ap = new AppPreferences(mContext);
            string strApiUrl = ap.getAccessKey("URL");
            string strApiUserName = ap.getAccessKey("USERNAME");
            string strApiPassword = ap.getAccessKey("PASSWORD");
            string strApiKey = ap.getAccessKey("APIKEY");

            // Looks like we don't have any setting stored so we need to go to the Setting Page
            if (string.IsNullOrEmpty(strApiUrl))
            {
                StartActivity(typeof(Settings));
            }

            // Do we have a apikey to make a calll, if not login fetch a key and store it in the local prefs
            if (string.IsNullOrEmpty(strApiKey) && !string.IsNullOrEmpty(strApiUrl))
            {
                // No apiKey stored so we'll need to log in
                try
                {
                    // Connect to the Soap Service here for Auth
                    Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                    Service.AllowAutoRedirect = true;
                    // Get the api key by logging in
                    object responseObject = Service.authenticate(strApiUserName, strApiPassword);

                    // Create an XML Node and cast the response object to it.
                    XmlNode[] responseXml;
                    responseXml = (System.Xml.XmlNode[])responseObject;

                    // fetech the abcolute position of the api key
                    XmlNode apiNode = responseXml[2].SelectSingleNode("value/item/item/value");
                    strApiKey = apiNode.InnerXml;
                    ap.saveAccessKey("APIKEY", strApiKey, true);
                }
                catch (Exception e)
                {
                    Toast welcome = Toast.MakeText(this, e.Message, ToastLength.Long);
                    welcome.Show();
                }
            }


            // Let's get the data for any current active recording
            getActiveRecording(startbutton, stopbutton, strApiUrl, strApiKey);

            startbutton.Click += delegate
            {

                try{
                    Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                    Service.AllowAutoRedirect = true;
                    //Get details of the active recording
                    object responseObject = Service.startRecord(strApiKey, Convert.ToInt16(strProjectID), Convert.ToInt16(strActivityID));
                    // toggle button states
                    startbutton.Enabled = false;
                    stopbutton.Enabled = true;
                    // need to get the new active recording

                }catch(Exception e){
                    Toast mesg = Toast.MakeText(this, e.Message, ToastLength.Long);
                    mesg.Show();
                }
               



            };



            stopbutton.Click += delegate
            {
                try
                {
                    Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                    Service.AllowAutoRedirect = true;
                    // Toggle button status
                    startbutton.Enabled = true;
                    stopbutton.Enabled = false;
                    //Get details of the active recording
                    object responseObject = Service.stopRecord(strApiKey, Convert.ToInt16(strEntryId));
                }catch(Exception e){
                    Toast mesg = Toast.MakeText(this, e.Message, ToastLength.Long);
                    mesg.Show();
                }
            };


            Button button = FindViewById<Button>(Resource.Id.btn_main);

            button.Click += delegate
            {
                StartActivity(typeof(Settings));
            };

            Button clearButton = FindViewById<Button>(Resource.Id.btn_clear);

            clearButton.Click += delegate
            {
                ap.clearPrefs();
            };
        }

        private void getActiveRecording(Button startbutton, Button stopbutton, string strApiUrl, string strApiKey)
        {
            try
            {
                Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                Service.AllowAutoRedirect = true;

                //Get details of the active recording
                object responseObject = Service.getActiveRecording(strApiKey);

                XmlNode[] responseXml = (System.Xml.XmlNode[])responseObject;

                XmlNodeList recordingNodeXml;

                recordingNodeXml = responseXml[2].SelectNodes("value/item/item");
                //Loop through the selected Nodes.
                TextView projectText = FindViewById<TextView>(Resource.Id.textView1);
                if (recordingNodeXml.Count == 0)
                {
                    startbutton.Enabled = true;
                    stopbutton.Enabled = false;
                }
                else
                {
                    startbutton.Enabled = false;
                    stopbutton.Enabled = true;
                }

                // Populate the projectText TextView
                currentProject(recordingNodeXml, projectText);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void currentProject(XmlNodeList recordingNodeXml, TextView projectText)
        {
            foreach (XmlNode node in recordingNodeXml)
            {
                //Fetch the Node and Attribute values.
                switch (node["key"].InnerText)
                {
                    case "timeEntryId":
                        //     projectText.Append((node["value"].InnerText));
                        strEntryId = node["value"].InnerText;
                        break;
                    case "activityID":
                        projectText.Append((node["value"].InnerText));
                        projectText.Append(System.Environment.NewLine);
                        strActivityID = node["value"].InnerText;
                        break;
                    case "projectID":
                        projectText.Append((node["value"].InnerText));
                        projectText.Append(System.Environment.NewLine);
                        strProjectID = node["value"].InnerText;
                        break;
                    case "start":
                        //     projectText.Append((node["value"].InnerText));
                        break;
                    case "end":
                        //    projectText.Append((node["value"].InnerText));
                        break;
                    case "duration":
                        projectText.Append((node["value"].InnerText));
                        projectText.Append(System.Environment.NewLine);
                        break;
                    case "servertime":
                        //   projectText.Append((node["value"].InnerText));
                        break;
                    case "customerID":
                        //    projectText.Append((node["value"].InnerText));
                        break;
                    case "customerName":
                        projectText.Append((node["value"].InnerText));
                        projectText.Append(System.Environment.NewLine);
                        break;
                    case "projectName":
                        projectText.Append((node["value"].InnerText));
                        projectText.Append(System.Environment.NewLine);
                        break;
                    case "activityName":
                        projectText.Append((node["value"].InnerText));
                        break;

                }
            }
        }
    }
}

