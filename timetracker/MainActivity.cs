using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using SQLite;
using static Kimai.KimaiDatadase;
using JsonKimaiMaps;
using Newtonsoft.Json;

namespace Kimai
{
    [Activity(Label = "Kimai", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : Activity
    {
        public bool debug = false;
        public string strEntryId;
        public string strCustomerID;
        public string strProjectID;
        public string strActivityID;
        public bool startButtonState;
        public bool stopButtonState;
        public int CurrentCustomerInTimer;
        public int CurrentProjectInTimer;
        public int CurrentActivityInTimer;

        //   public TextView TimerViewer;
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
        int count = 1;

        public string ActionMessage;
        public string KimaiMessage;
        AppPreferences ap;
        string strApiKey;
        KimaiServer MyKimai = new KimaiServer();


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            string dbPath = System.IO.Path.Combine(
System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
"localkimaidata.db3");

            db = new SQLiteConnection(dbPath);
            // Fetch App Prefs
            Context mContext = Application.Context;
            ap = new AppPreferences(mContext);
            SetContentView(Resource.Layout.Main);
            if (string.IsNullOrEmpty(ap.getAccessKey("URL")))
            {
                StartActivity(typeof(Settings));
            }
            else
            {
                MyKimai.url = ap.getAccessKey("URL") + "/core/json.php";

            }

            RunUpdateLoop();

            Button update_button = FindViewById<Button>(Resource.Id.update);
            update_button.Click += delegate
            {
                GetActiveRecord();
            };
       //     ToggleButton togglebutton = FindViewById<ToggleButton>(Resource.Id.toggleButton1);

            strApiKey = ap.getAccessKey("APIKEY");
            // Do we haave an api key?
            if (string.IsNullOrEmpty(strApiKey))
            {
                // No, Let's log in
                LoginToKimai();
            }
            else
            {
                //GetActiveRecord();

                //
                ToggleButton togglebutton = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
                // Let's get the data for any current active recording and update the start/stop button states
              
                if(do_refresh()){
                    togglebutton.Checked = true;
                        RunUpdateLoopState = true;

                        Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();
                }else{
                    togglebutton.Checked = false;
                    RunUpdateLoopState = false;
                    Tv2 = FindViewById<TextView>(Resource.Id.textView2); Tv2.Text = RunUpdateLoopState.ToString();
                }

                togglebutton.Click += (o, e) =>
                {

                    // Perform action on clicks
                    if (togglebutton.Checked)
                    {
                        if (!GetActiveRecord())
                        {
                            List<string> Parameters = new List<string>
                        {
                            strApiKey
                        };
                            System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("startRecord", Parameters));
                            taskA.Wait();
                            RunUpdateLoopState = true;
                            DurationCount = 0;
                        }else{
                            // We did not start a new recording we just switvhed to the active one.
                            //Todo: this has big repercussions, as many devices could start mnay recordings with out stopping any exiting ones
                            // If this proves to be a problem the best approach may be just to exit and warn the user multiple timers are running.
                            Toast.MakeText(this, "There appears to be an active recording", ToastLength.Long).Show();
                            do_refresh();
                        }
                    }
                    else
                    {
                            List<string> Parameters = new List<string>
                        {
                            strApiKey
                        };
                           System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("stopRecord", Parameters));
                            taskA.Wait();
                            RunUpdateLoopState = false;
                            DurationCount = 0;
 
                           do_refresh();
                    }
                };
            }
        }

        private bool do_refresh()
        {         
            PopulateCustomersSpinner();
            return GetActiveRecord();
        }

        /// <summary>
        /// Ons the create options menu.
        /// </summary>
        /// <returns><c>true</c>, if create options menu was oned, <c>false</c> otherwise.</returns>
        /// <param name="menu">Menu.</param>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.option_menu, menu);
            return true;
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
                    //GetActiveRecord();
                    do_refresh();
                    PopulateCustomersSpinner();
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
        /// Logins to kimai.
        /// </summary>
        private void LoginToKimai()
        {
            List<string> Parameters = new List<string>
            {
                "john.burrin",
                "dragon32"
            };
          //  MyKimai.ConnectAsync("authenticate", Parameters);
            System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("authenticate", Parameters));
            taskA.Wait();
            ActionMessage = "Login Complete";
            KimaiMessage = "LoginToKimai";

        }

        /// <summary>
        /// Gets the active record.
        /// </summary>
        private bool GetActiveRecord()
        {
            TextView projectText = FindViewById<TextView>(Resource.Id.textView1);
            TextView TimerViewer = FindViewById<TextView>(Resource.Id.TimerView);
            ToggleButton togglebutton = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
            bool activeEvent = false;
            List<string> Parameters = new List<string>
            {
                strApiKey
            };
            try
            {
               // MyKimai.ConnectAsync("getActiveRecording", Parameters);
                System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("getActiveRecording", Parameters));
                taskA.Wait();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
            ActiveRecordingMap ActiveRecordingObject = new ActiveRecordingMap();
            try
            {
            ActiveRecordingObject = JsonConvert.DeserializeObject<ActiveRecordingMap>(MyKimai.JsonResultString);
                bool Success = ActiveRecordingObject.Result.Success;
                if (Success)
                {
                    projectText.Text = "Customer: ";
                    projectText.Append(ActiveRecordingObject.Result.Items[0].customerName);
                    projectText.Append(System.Environment.NewLine);
                    projectText.Append("Project: ");
                    projectText.Append(ActiveRecordingObject.Result.Items[0].projectName);
                    projectText.Append(System.Environment.NewLine);
                    projectText.Append("Activity:");
                    projectText.Append(ActiveRecordingObject.Result.Items[0].activityName);
                    UInt32 StartTimeInUnixTime = ActiveRecordingObject.Result.Items[0].start;
                    UInt32 TimeNowInUnixTime = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    RunUpdateLoopState = true;
                    togglebutton.Checked = true;
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
                    activeEvent = true;
                }
                else
                {
                    RunUpdateLoopState = false;
                    togglebutton.Checked = false;
                    projectText.Text = "No Active Recording.";
                    TimerViewer.Text = "00:00:00";
                    activeEvent = false;
                }
                return activeEvent;
              }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
           }
            return activeEvent;
        }

        /// <summary>
        /// Runs the update loop. This loop runs forever, and updates the clock every second provided that the RunUpdateLoopState is true
        /// </summary>
        private async void RunUpdateLoop()
        {
            while (true)
            {
                await System.Threading.Tasks.Task.Delay(1000);
                TimeSpan time = TimeSpan.FromSeconds(DurationCount++);
                TextView TimerViewer = FindViewById<TextView>(Resource.Id.TimerView);
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
        /// Customers the spinner item selected.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void CustomerSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            PopulateProjectsSpinner(CustomerLookupList[e.Position]);
            CurrentCustomerInTimer = e.Position;
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
                // Clear the customer lookup table
                CustomerLookupList.Clear();
                var customers = db.Query<KimaiDatadase.Customer>("SELECT * FROM Customer");
                int count = customers.Count;
                if (count > 0)
                {
                    var customeradapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
                    int index = 0;
                    foreach (var customer in customers)
                    {
                        // Add item to the data adapter
                        customeradapter.Add(customer.Name);
                        // Add items to the lookup list by adding an index and the kimai customerID
                        CustomerLookupList.Add(index++, customer.CustomerID);
                    }
                    // Apply the data adapter to the spinner
                    CustomersSpinner.Adapter = customeradapter;
                }

                string c = KeyByValue(CustomerLookupList, Convert.ToUInt16(strCustomerID));
                CustomersSpinner.SetSelection(Convert.ToInt16(c));

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
                var projects = db.Query<KimaiDatadase.Project>("SELECT * FROM Project WHERE CustomerID = ?", customerID);
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

                string p = KeyByValue(ProjectLookupList, Convert.ToUInt16(strProjectID));
                ProjectsSpinner.SetSelection(Convert.ToInt16(p));
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

                string a = KeyByValue(ActivitiesLookupList, Convert.ToUInt16(strActivityID));
                ActivitiesSpinner.SetSelection(Convert.ToInt16(a));
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

    }

}

