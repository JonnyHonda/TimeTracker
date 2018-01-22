using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using TimeTracker.kimai.tsgapis.com;
using System.Xml;
using System;
using System.Threading.Tasks;

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

        public TextView TimerViewer { get; private set; }
        public bool RunUpdateLoopState = true;
        public int DurationCount = 1;

        //string apiKey;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            TimerViewer = FindViewById<TextView>(Resource.Id.TimerView);

            RunUpdateLoop();
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


            // Let's get the data for any current active recording and update the start/stop button states
            try
            {
                int countofRecodrings = getActiveRecording(strApiUrl, strApiKey);
                if (countofRecodrings == 0)
                {
                    startbutton.Enabled = true; RunUpdateLoopState = false;
                    stopbutton.Enabled = false;
                }
                else
                {
                    startbutton.Enabled = false;RunUpdateLoopState = true;
                    stopbutton.Enabled = true;
                }
            }
            catch (Exception e)
            {
                Toast mesg = Toast.MakeText(this, e.Message, ToastLength.Long);
                mesg.Show();
            }

            /**
             * Start button onClick event, attempts to start a new recording and update the view
            **/
            startbutton.Click += delegate
            {

                try
                {
                    Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                    Service.AllowAutoRedirect = true;
                    //Get details of the active recording
                    object responseObject = Service.startRecord(strApiKey, Convert.ToInt16(strProjectID), Convert.ToInt16(strActivityID));
                    // toggle button states
                    startbutton.Enabled = false;RunUpdateLoopState = true;
                    stopbutton.Enabled = true;
                    // need to get the new active recording
                    try
                    {
                        int x = getActiveRecording(strApiUrl, strApiKey);
                    }
                    catch (Exception e)
                    {
                        throw (e);
                    }
                }
                catch (Exception e)
                {
                    throw (e);
                }
            };


            /**
             * Stop button onClick event, attempts to stop the current recording and update the text view
            **/
            stopbutton.Click += delegate
            {
                try
                {
                    Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                    Service.AllowAutoRedirect = true;
                    // Toggle button status
                    startbutton.Enabled = true;RunUpdateLoopState = false;
                    stopbutton.Enabled = false;
                    //Get details of the active recording
                    object responseObject = Service.stopRecord(strApiKey, Convert.ToInt16(strEntryId));
                    // need to get the new active recording, incase it did not stop
                    try
                    {
                        int x = getActiveRecording(strApiUrl, strApiKey);
                    }
                    catch (Exception e)
                    {
                        throw (e);
                    }
                }
                catch (Exception e)
                {
                    Toast mesg = Toast.MakeText(this, e.Message, ToastLength.Long);
                    mesg.Show();
                }
            };

            /** 
                OnCLick event to Launch the settings activity
            **/
            Button button = FindViewById<Button>(Resource.Id.btn_main);
            button.Click += delegate
            {
                StartActivity(typeof(Settings));
            };

            /** 
                OnCLick event to clear then Shared Preferences, this function only clears the stored prefs
            **/
            Button clearButton = FindViewById<Button>(Resource.Id.btn_clear);
            clearButton.Click += delegate
            {
                ap.clearPrefs();
            };
        }


        private async void RunUpdateLoop()
        {
           
            while (RunUpdateLoopState)
            {
                await Task.Delay(1000);
                TimerViewer.Text = string.Format("{0} ticks!", DurationCount++);
            }
        }
        /**
         * Gets the currect active recording 
         * 
        */
        int getActiveRecording(string lstrApiUrl, string lstrApiKey)
        {
            try
            {
                Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(lstrApiUrl + "/core/soap.php");
                Service.AllowAutoRedirect = true;

                //Get details of the active recording
                object responseObject = Service.getActiveRecording(lstrApiKey);

                XmlNode[] responseXml = (System.Xml.XmlNode[])responseObject;

                XmlNodeList recordingNodeXml;

                recordingNodeXml = responseXml[2].SelectNodes("value/item/item");
                //Loop through the selected Nodes.
                TextView projectText = FindViewById<TextView>(Resource.Id.textView1);

                // Populate the projectText TextView
                currentProject(recordingNodeXml, projectText);
                return recordingNodeXml.Count;

            }
            catch (Exception e)
            {
                throw (e);
            }
        }


        /**
         * Takes an XML Node List and a TextView and populates it with the node list data
         **/
        void currentProject(XmlNodeList recordingNodeXml, TextView projectText)
        {
            if (recordingNodeXml.Count > 0)
            {
                projectText.Text = "";
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
                            // projectText.Append((node["value"].InnerText));
                            // projectText.Append(System.Environment.NewLine);
                            strActivityID = node["value"].InnerText;
                            break;
                        case "projectID":
                            //projectText.Append((node["value"].InnerText));
                            //projectText.Append(System.Environment.NewLine);
                            strProjectID = node["value"].InnerText;
                            break;
                        case "start":
                            //     projectText.Append((node["value"].InnerText));
                            break;
                        case "end":
                            //    projectText.Append((node["value"].InnerText));
                            break;
                        case "duration":
                            //projectText.Append((node["value"].InnerText));
                            //projectText.Append(System.Environment.NewLine);
                            DurationCount = Convert.ToInt16(node["value"].InnerText);
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
            else
            {
                projectText.Text = "No Active Recording.";
            }
        }
    }
}

