using System;
using System.Threading.Tasks;
using System.Xml;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using SQLite;
using TimeTracker.kimai.tsgapis.com;
using static TimeTracker.KimaiDatadase;

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

        public TextView TimerViewer;
        public TextView Tv2;
        public bool RunUpdateLoopState = true;
        public UInt32 DurationCount = 1;

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



            Console.WriteLine("Creating database, if it doesn't already exist");
            string dbPath = System.IO.Path.Combine(
                 System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                 "localkimaidata.db3");
            var db = new SQLiteConnection(dbPath);

            /********************************************
             * 
             * Store Customers in DB and populate Spinner
             * 
             *////////////////////////////////////////////
            db.CreateTable<Customer>();
            // Tempary Delete all while testing
            db.DeleteAll<Customer>();
            if (db.Table<Customer>().Count() == 0)
            {
                populateCustomerTable(strApiUrl, strApiKey, db);
            }

            Spinner Customers = FindViewById<Spinner>(Resource.Id.spinnerCustomers);
            var customers = db.Table<Customer>();
            var customeradapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
            foreach (var customer in customers)
            {
                customeradapter.Add(customer.Name);
            }
            Customers.Adapter = customeradapter;

            /********************************************
             * 
             * Store Projects in DB and populate Spinner
             * 
             *////////////////////////////////////////////
            db.CreateTable<Project>();
            // Tempary Delete all while testing
            db.DeleteAll<Project>();
            if (db.Table<Project>().Count() == 0)
            {
                populateProjectTable(strApiUrl, strApiKey, db);
            }
            Spinner Projects = FindViewById<Spinner>(Resource.Id.spinnerProjects);
            var projects = db.Table<Project>();
            var projectadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
            foreach (var project in projects)
            {
                projectadapter.Add(project.Name);
            }
            Projects.Adapter = projectadapter;

            /********************************************
             * 
             * Store Activities in DB and populate Spinner
             * 
             *////////////////////////////////////////////

            db.CreateTable<ProjectActivity>();
            // Tempary Delete all while testing
            db.DeleteAll<ProjectActivity>();
            if (db.Table<ProjectActivity>().Count() == 0)
            {
                       populateActivityTable(strApiUrl, strApiKey, db);
            }
            Spinner Activities = FindViewById<Spinner>(Resource.Id.spinnerActivities);
            var activities = db.Table<ProjectActivity>();
            var activityadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
            foreach (var activity in activities)
            {
                activityadapter.Add(activity.Name);
            }
            Activities.Adapter = activityadapter;



            // Do we have a apikey to make a call, if not login fetch a key and store it in the local prefs
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
                    Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();
                }
                else
                {
                    startbutton.Enabled = false; RunUpdateLoopState = true;
                    stopbutton.Enabled = true;
                    Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();
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
                    startbutton.Enabled = false; RunUpdateLoopState = true;
                    stopbutton.Enabled = true;
                    Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();
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
                    startbutton.Enabled = true; RunUpdateLoopState = false;
                    stopbutton.Enabled = false;
                    Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();

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
        private static void populateCustomerTable(string strApiUrl, string strApiKey, SQLiteConnection db)
        {
            try
            {
                Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                Service.AllowAutoRedirect = true;

                //Get details of the active recording
                object responseObject = Service.getCustomers(strApiKey);

                XmlNode[] responseXml = (System.Xml.XmlNode[])responseObject;

                XmlNodeList customerNodeXml;

                customerNodeXml = responseXml[2].SelectNodes("value/item");
                var newCustomer = new Customer();
                foreach (XmlNode node in customerNodeXml)
                {
                    //Fetch the Node and Attribute values.
                    XmlNodeList lns;
                    lns = node.SelectNodes("item");
                    foreach (XmlNode n in lns)
                    {
                        switch (n["key"].InnerText)
                        {
                            case "name":
                                newCustomer.Name = n["value"].InnerText;
                                break;
                            case "customerID":
                                newCustomer.ID = Convert.ToInt16(n["value"].InnerText);
                                break;
                        }
                    }
                    db.Insert(newCustomer);

                }
            }
            catch
            {

            }
        }
        private static void populateActivityTable(string strApiUrl, string strApiKey, SQLiteConnection db)
        {
            try
            {
                Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                Service.AllowAutoRedirect = true;


                // Get a list of project id's from the database

                var projects = db.Table<Project>();

                foreach (var project in projects)
                {
                //Get details of the active recording
                    object responseObject = Service.getTasks(strApiKey,project.ID);

                XmlNode[] responseXml = (System.Xml.XmlNode[])responseObject;

                XmlNodeList activityNodeXml;

                activityNodeXml = responseXml[2].SelectNodes("value/item");
                var newActivity = new ProjectActivity();
                    foreach (XmlNode node in activityNodeXml)
                    {
                        //Fetch the Node and Attribute values.
                        XmlNodeList lns;
                        lns = node.SelectNodes("item");
                        foreach (XmlNode n in lns)
                        {
                            switch (n["key"].InnerText)
                            {
                                case "name":
                                    newActivity.Name = n["value"].InnerText;
                                    break;
                                case "activityID":
                                    newActivity.ID = Convert.ToInt16(n["value"].InnerText);
                                    newActivity.ProjectID= project.ID;
                                    break;
                                                            }
                        }
                        db.Insert(newActivity);

                    }
                }
            }
            catch
            {

            }
        }

        private static void populateProjectTable(string strApiUrl, string strApiKey, SQLiteConnection db)
        {
            try
            {
                Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                Service.AllowAutoRedirect = true;

                //Get details of the active recording
                object responseObject = Service.getProjects(strApiKey, false);

                XmlNode[] responseXml = (System.Xml.XmlNode[])responseObject;

                XmlNodeList projectNodeXml;

                projectNodeXml = responseXml[2].SelectNodes("value/item");
                var newProject = new Project();
                foreach (XmlNode node in projectNodeXml)
                {
                    //Fetch the Node and Attribute values.
                    XmlNodeList lns;
                    lns = node.SelectNodes("item");
                    foreach (XmlNode n in lns)
                    {
                        switch (n["key"].InnerText)
                        {
                            case "name":
                                newProject.Name = n["value"].InnerText;
                                break;
                            case "projectID":
                                newProject.ID = Convert.ToInt16(n["value"].InnerText);
                                break;
                            case "customerID":
                                newProject.CustomerID = Convert.ToInt16(n["value"].InnerText);
                                break;
                        }
                    }
                    db.Insert(newProject);

                }
            }
            catch
            {

            }
        }

        private async void RunUpdateLoop()
        {

            while (true)
            {
                await Task.Delay(1000);
                TimeSpan time = TimeSpan.FromSeconds(DurationCount++);
                if (RunUpdateLoopState)
                {
                    try
                    {
                        string str = time.ToString(@"hh\:mm\:ss");
                        TimerViewer.Text = str;
                        Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = "Running";
                    }
                    catch (Exception ex)
                    {
                        TimerViewer.Text = "00:00:00";
                    }
                }
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
                //TimerViewer.Text = "00:00:00";
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
                            UInt32 StartTimeInUnixTime = Convert.ToUInt32(node["value"].InnerText);
                            UInt32 TimeNowInUnixTime = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                            try
                            {
                                DurationCount = TimeNowInUnixTime - StartTimeInUnixTime;
                            }
                            catch (Exception ex)
                            {
                                Toast mesg = Toast.MakeText(this, ex.Message, ToastLength.Long);
                                mesg.Show();
                                DurationCount = 0;
                            }
                            break;
                        case "end":
                            //    projectText.Append((node["value"].InnerText));
                            break;
                        case "duration":
                            //projectText.Append((node["value"].InnerText));
                            //projectText.Append(System.Environment.NewLine);
                            // DurationCount = Convert.ToInt16(node["value"].InnerText);
                            break;
                        case "servertime":
                            //   projectText.Append((node["value"].InnerText));
                            break;
                        case "customerID":
                            //    projectText.Append((node["value"].InnerText));
                            break;
                        case "customerName":
                            projectText.Append("Customer: ");
                            projectText.Append((node["value"].InnerText));
                            projectText.Append(System.Environment.NewLine);
                            break;
                        case "projectName":
                            projectText.Append("Project: ");
                            projectText.Append((node["value"].InnerText));
                            projectText.Append(System.Environment.NewLine);
                            break;
                        case "activityName":
                            projectText.Append("Activity:");
                            projectText.Append((node["value"].InnerText));
                            break;

                    }
                }
            }
            else
            {
                projectText.Text = "No Active Recording.";
                TimerViewer.Text = "00:00:00";

            }
        }
    }
}

