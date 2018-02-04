using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Toolbar = Android.Support.V7.Widget.Toolbar;

using Android.Views;
using Android.Widget;
using SQLite;
using TimeTracker.kimai.tsgapis.com;
using TimeTracker.Resources;
using static TimeTracker.KimaiDatadase;

namespace TimeTracker
{
    [Activity(Label = "TimeTracker", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {
        public bool debug = false;
        public string strEntryId;
        public string strProjectID;
        public string strActivityID;
        public bool startButtonState;
        public bool stopButtonState;
        public int CurrentCustomerInTimer;
        public int CurrentProjectInTimer;
        public int CurrentActivityInTimer;

        public TextView TimerViewer;
        public TextView Tv2;
        public bool RunUpdateLoopState = true;
        public UInt32 DurationCount = 1;

        /// <summary>
        /// The customer lookup list.
        /// The Look up list hold an two integers <index><customerId>
        /// </summary>
        public Dictionary<int, int> CustomerLookupList = new Dictionary<int, int>();
        /// <summary>
        /// The project lookup list.
        /// The Look up list hold an two integers <index><projectId>
        /// </summary>
        public Dictionary<int, int> ProjectLookupList = new Dictionary<int, int>();
        /// <summary>
        /// The activities lookup list.
        /// The Look up list hold an two integers <index><activityId>
        /// </summary>
        public Dictionary<int, int> ActivitiesLookupList = new Dictionary<int, int>();
        public SQLiteConnection db;
        public string strApiUrl;
        public string strApiUserName;
        public string strApiPassword;
        public string strApiKey;

        AppPreferences ap;
        //string apiKey;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);


            SetSupportActionBar(toolbar);
            SupportActionBar.Title = GetString(Resource.String.app_name);
            //SetActionBar(toolbar);

            string dbPath = System.IO.Path.Combine(
                 System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "localkimaidata.db3");
            db = new SQLiteConnection(dbPath);
            TimerViewer = FindViewById<TextView>(Resource.Id.TimerView);

            RunUpdateLoop();
            // Fetch App Prefs
            Context mContext = Android.App.Application.Context;
            ap = new AppPreferences(mContext);
            strApiUrl = ap.getAccessKey("URL");
            strApiUserName = ap.getAccessKey("USERNAME");
            strApiPassword = ap.getAccessKey("PASSWORD");
            strApiKey = ap.getAccessKey("APIKEY");

            try
            {
                CurrentCustomerInTimer = Convert.ToInt16(ap.getAccessKey("CurrentCustomerInTimer"));
                CurrentProjectInTimer = Convert.ToInt16(ap.getAccessKey("CurrentProjectInTimer"));
                CurrentActivityInTimer = Convert.ToInt16(ap.getAccessKey("CurrentActivityInTimer"));
            }
            catch
            {
                CurrentCustomerInTimer = 0;
                ap.saveAccessKey("CurrentCustomerInTimer", "0");
                CurrentProjectInTimer = 0;
                ap.saveAccessKey("CurrentProjectInTimer", "0");
                CurrentActivityInTimer = 0;
                ap.saveAccessKey("CurrentActivityInTimer", "0");
            }

            // Looks like we don't have any setting stored so we need to go to the Setting Page
            if (string.IsNullOrEmpty(strApiUrl) || debug)
            {
                StartActivity(typeof(Settings));
            }
            else
            {

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

                PopulateCustomersSpinner();
                ToggleButton togglebutton = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
                // Let's get the data for any current active recording and update the start/stop button states
                try
                {
                    int countofRecodrings = getActiveRecording(strApiUrl, strApiKey);
                    if (countofRecodrings == 0)
                    {
                        togglebutton.Checked = false;
                        RunUpdateLoopState = false;

                        Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();
                    }
                    else
                    {
                        togglebutton.Checked = true;
                        RunUpdateLoopState = true;

                        Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();
                    }
                }
                catch (Exception e)
                {
                    Toast mesg = Toast.MakeText(this, e.Message, ToastLength.Long);
                    mesg.Show();
                }



                /**
                 * 
                 * OnClick events for toggle button
                **/
                togglebutton.Click += (o, e) =>
                {
                    // Perform action on clicks
                    if (togglebutton.Checked)
                        StartTimer();
                    else
                    {
                        StopTimer();
                    }
                };
            }


        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        private void StopTimer()
        {
            try
            {
                Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                Service.AllowAutoRedirect = true;
                // Toggle button status
                RunUpdateLoopState = false;
                Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();

                //Get details of the active recording
                object responseObject = Service.stopRecord(strApiKey, Convert.ToInt16(strEntryId));

                // need to get the new active recording, incase it did not stop
                try
                {
                    int x = getActiveRecording(strApiUrl, strApiKey);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
            Toast.MakeText(this, "Timer Stopped", ToastLength.Short).Show();
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        private void StartTimer()
        {
            try
            {
                Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                Service.AllowAutoRedirect = true;
                //Get details of the active recording
                object responseObject;
                responseObject = Service.startRecord(strApiKey, ProjectLookupList[CurrentProjectInTimer], ActivitiesLookupList[CurrentActivityInTimer]);

                // toggle button states
                RunUpdateLoopState = true;
                Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();

                // need to get the new active recording
                try
                {
                    int x = getActiveRecording(strApiUrl, strApiKey);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
            Toast.MakeText(this, "Timer Started", ToastLength.Short).Show();
        }

        /// <summary>
        /// Ons the options item selected.
        /// </summary>
        /// <returns><c>true</c>, if options item selected was oned, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_refresh:
                    getActiveRecording(strApiUrl, strApiKey);
                    break;
                case Resource.Id.menu_about:
                    StartActivity(typeof(About));
                    break;
                case Resource.Id.menu_settings:
                    StartActivity(typeof(Settings));
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Ons the create options menu.
        /// </summary>
        /// <returns><c>true</c>, if create options menu was oned, <c>false</c> otherwise.</returns>
        /// <param name="menu">Menu.</param>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        /// <summary>
        /// Customers the spinner item selected.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void CustomerSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            PopulateProjectsSpinner(CustomerLookupList[e.Position]);
            CurrentCustomerInTimer = e.Position;
            ap.saveAccessKey("CurrentCustomerInTimer", CurrentCustomerInTimer.ToString());

        }


        /// <summary>
        /// Populates the customers spinner.
        /// </summary>
        private void PopulateCustomersSpinner()
        {
            Spinner CustomersSpinner = FindViewById<Spinner>(Resource.Id.spinnerCustomers);
            CustomersSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(CustomerSpinnerItemSelected);

            try
            {
                // Clear the custoer lookup table
                CustomerLookupList.Clear();
                var customers = db.Query<Customer>("SELECT * FROM Customer");
                int count = customers.Count;
                if (count > 0)
                {
                    var customeradapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
                    int index = 0;
                    foreach (var customer in customers)
                    {
                        customeradapter.Add(customer.Name);
                        CustomerLookupList.Add(index++, customer.CustomerID);
                    }
                    CustomersSpinner.Adapter = customeradapter;
                }
                if (Convert.ToUInt16(ap.getAccessKey("CurrentCustomerInTimer")) > count)
                {

                    CustomersSpinner.SetSelection(0);
                    ap.saveAccessKey("CurrentCustomerInTimer", "0");
                    ap.saveAccessKey("CurrentProjectInTimer", "0");
                    ap.saveAccessKey("CurrentActivityInTimer", "0");
                }
                else
                {
                    CustomersSpinner.SetSelection(Convert.ToUInt16(ap.getAccessKey("CurrentCustomerInTimer")));
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }

        }


        /// <summary>
        /// Populates the projects spinner.
        /// </summary>
        /// <param name="customerID">Customer identifier.</param>
        private void PopulateProjectsSpinner(int customerID)
        {
            Spinner ProjectsSpinner = FindViewById<Spinner>(Resource.Id.spinnerProjects);
            ProjectsSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(ProjectSpinnerItemSelected);

            try
            {
                ProjectLookupList.Clear();
                var projects = db.Query<Project>("SELECT * FROM Project WHERE CustomerID = ?", customerID);
                int index = 0;
                int count = projects.Count;
                if (count > 0)
                {
                    var projectadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
                    foreach (var project in projects)
                    {
                        projectadapter.Add(project.Name);
                        ProjectLookupList.Add(index++, project.ProjectID);
                    }
                    ProjectsSpinner.Adapter = projectadapter;
                }

                if (Convert.ToUInt16(ap.getAccessKey("CurrentProjectInTimer")) >= count)
                {
                    ProjectsSpinner.SetSelection(0);
                    ap.saveAccessKey("CurrentProjectInTimer", "0");
                    ap.saveAccessKey("CurrentActivityInTimer", "0");
                }
                else
                {
                    ProjectsSpinner.SetSelection(Convert.ToUInt16(ap.getAccessKey("CurrentProjectInTimer")));

                }
            }
            catch (Exception ex) { Toast.MakeText(this, ex.Message, ToastLength.Long).Show(); }
        }


        /// <summary>
        /// Projects the spinner item selected.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void ProjectSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            PopulateActivitiesSpinner(ProjectLookupList[e.Position]);
            CurrentProjectInTimer = e.Position;
            ap.saveAccessKey("CurrentProjectInTimer", CurrentProjectInTimer.ToString());
        }


        /// <summary>
        /// Populates the activities spinner.
        /// </summary>
        /// <param name="projectID">Project identifier.</param>
        private void PopulateActivitiesSpinner(int projectID)
        {
            Spinner ActivitiesSpinner = FindViewById<Spinner>(Resource.Id.spinnerActivities);
            ActivitiesSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(ActivitySpinnerItemSelected);
            try
            {
                ActivitiesLookupList.Clear();
                var activities = db.Query<ProjectActivity>("SELECT * FROM ProjectActivity WHERE ProjectID = ?", projectID);
                int index = 0;
                int count = activities.Count;
                if (count > 0)
                {
                    var activityadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
                    foreach (var activity in activities)
                    {
                        activityadapter.Add(activity.Name);
                        ActivitiesLookupList.Add(index++, activity.ActivityID);
                    }
                    ActivitiesSpinner.Adapter = activityadapter;
                }

                if (Convert.ToUInt16(ap.getAccessKey("CurrentActivityInTimer")) > count)
                {
                    ActivitiesSpinner.SetSelection(0);
                    ap.saveAccessKey("CurrentActivityInTimer", "0");
                }
                else
                {
                    ActivitiesSpinner.SetSelection(Convert.ToUInt16(ap.getAccessKey("CurrentActivityInTimer")));

                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }

        private void ActivitySpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            CurrentActivityInTimer = e.Position;
            ap.saveAccessKey("CurrentActivityInTimer", CurrentActivityInTimer.ToString());
        }



        /// <summary>
        /// Runs the update loop. This loop runs foreve, and updates the clock ever second provided that the RunUpdateLoopState in true
        /// </summary>
        private async void RunUpdateLoop()
        {
            while (true)
            {
                await Task.Delay(1000);
                TimeSpan time = TimeSpan.FromSeconds(DurationCount++);

                if (RunUpdateLoopState)
                {
                    int hours = time.Hours;
                    if (time.Days > 0)
                    {
                        hours = (time.Days * 24) + hours;
                    }

                    string str = String.Format("{0:00}:{1:00}:{2:00}", hours, time.Minutes, time.Seconds);
                    TimerViewer.Text = str;
                    Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = "Running";
                }
                else
                {
                    TimerViewer.Text = "00:00:00";
                }
            }
        }


        /// <summary>
        /// Gets the active recording.
        /// </summary>
        /// <returns>The active recording.</returns>
        /// <param name="lstrApiUrl">Lstr API URL.</param>
        /// <param name="lstrApiKey">Lstr API key.</param>
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

        public static string KeyByValue(Dictionary<int, int> dict, int val)
        {
            int key = 0;
            foreach (KeyValuePair<int, int> pair in dict)
            {
                if (pair.Value == val)
                {
                    key = pair.Key;
                    break;
                }
            }
            return key.ToString();
        }
        /// <summary>
        /// Currents the project.
        /// </summary>
        /// <param name="recordingNodeXml">Recording node xml.</param>
        /// <param name="projectText">Project text.</param>
        void currentProject(XmlNodeList recordingNodeXml, TextView projectText)
        {
            ToggleButton togglebutton = FindViewById<ToggleButton>(Resource.Id.toggleButton1);

            if (recordingNodeXml.Count > 0)
            {
                //TimerViewer.Text = "00:00:00";
                projectText.Text = "";
                RunUpdateLoopState = true;
                togglebutton.Checked = true;

                foreach (XmlNode node in recordingNodeXml)
                {
                    //Fetch the Node and Attribute values.
                    switch (node["key"].InnerText)
                    {
                        case "timeEntryId":
                            //     projectText.Append((node["value"].InnerText));
                            strEntryId = node["value"].InnerText;
                            break;

                        case "customerID":
                            projectText.Append("(C:");
                            projectText.Append((node["value"].InnerText));
                            projectText.Append(")");

                            //CurrentCustomerInTimer = Convert.ToUInt16((node["value"].InnerText));
                            //CustomersSpinner.SetSelection(Convert.ToUInt16(ap.getAccessKey("CurrentCustomerInTimer")));
                            //CurrentCustomerInTimer = GetCustomerIndexByKimaiId(Convert.ToUInt16(node["value"].InnerText));
                            string c = KeyByValue(CustomerLookupList, Convert.ToUInt16((node["value"].InnerText)));
                            ap.saveAccessKey("CustomerLookupList", c);
                            //CurrentCustomerInTimer = c;
                            //CustomersSpinner.SetSelection(c);
                            break;

                        case "projectID":
                            //projectText.Append((node["value"].InnerText));
                            //projectText.Append(System.Environment.NewLine);
                            strProjectID = node["value"].InnerText;
                            projectText.Append("(P:");
                            projectText.Append((node["value"].InnerText));
                            projectText.Append(")");
                            //   CurrentProjectInTimer = Convert.ToUInt16(strProjectID);
                            string p = KeyByValue(ProjectLookupList, Convert.ToUInt16((node["value"].InnerText)));
                            ap.saveAccessKey("CurrentProjectInTimer", p);
                            //CurrentProjectInTimer = p;
                            //ProjectsSpinner.SetSelection(p);
                            break;

                        case "activityID":
                            // projectText.Append((node["value"].InnerText));
                            // projectText.Append(System.Environment.NewLine);
                            strActivityID = node["value"].InnerText;
                            projectText.Append("(A:");
                            projectText.Append((node["value"].InnerText));
                            projectText.Append(")");
                            //    CurrentActivityInTimer = Convert.ToUInt16(strActivityID);
                            string a = KeyByValue(ActivitiesLookupList, Convert.ToUInt16((node["value"].InnerText)));
                            //CurrentActivityInTimer = a;
                            ap.saveAccessKey("CurrentActivityInTimer", a);
                            //ActivitiesSpinner.SetSelection(a);
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
                RunUpdateLoopState = false;
                togglebutton.Checked = false;
                projectText.Text = "No Active Recording.";
                TimerViewer.Text = "00:00:00";

            }
        }
    }
}

