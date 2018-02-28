using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using SQLite;
using static TimeTracker.KimaiDatadase;
using JsonKimaiMaps;
using Newtonsoft.Json;
using Android.Graphics;


namespace TimeTracker
{
    [Activity(Label = "TimeTracker", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : Activity
    {
        public bool debug = false;
        //       public string strEntryId;
        public bool startButtonState;
        public bool stopButtonState;

        // These need to be the actual data base index ids
        public int CurrentCustomerInTimer = 0;
        public int CurrentProjectInTimer = 0;
        public int CurrentActivityInTimer = 0;

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

        AppPreferences ap;
        string strApiKey;
        KimaiServer MyKimai = new KimaiServer();
        // public bool IsChangingSpinner = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            string dbPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                    "localkimaidata.db3");

            db = new SQLiteConnection(dbPath);
            // Fetch App Prefs
            Context mContext = Application.Context;
            ap = new AppPreferences(mContext);

            if (string.IsNullOrEmpty(ap.getAccessKey("URL")) ||
                string.IsNullOrEmpty(ap.getAccessKey("APIKEY")) ||
                string.IsNullOrEmpty(ap.getAccessKey("USERNAME")) ||
                string.IsNullOrEmpty(ap.getAccessKey("PASSWORD"))
               )
            {
                StartActivity(typeof(Settings));
            }
            else
            {
                // Set up TimetView Font and Colour
                TextView TimerViewer = FindViewById<TextView>(Resource.Id.TimerView);
                //TODO: Remove any unsued font files for release build
                Typeface tf = Typeface.CreateFromAsset(Application.Context.Assets, "DS-DIGIT.TTF");
                TimerViewer.SetTypeface(tf, TypefaceStyle.Normal);
                //  TimerViewer.SetTextColor(Android.Graphics.Color.Green);

                PopulateCustomersSpinner();
                MyKimai.url = ap.getAccessKey("URL") + "/core/json.php";
                RunUpdateLoop();

                Button update_button = FindViewById<Button>(Resource.Id.update);
                update_button.Click += delegate
                {
                    // Perform an update to the current running timer, so all the server to see if we have one.
                    if (GetActiveRecord())
                    {
                        Spinner ProjectSpinner = FindViewById<Spinner>(Resource.Id.spinnerProjects);
                        Spinner ActivitySpinner = FindViewById<Spinner>(Resource.Id.spinnerActivities);
                        List<object> Parameters = new List<object>();

                        Parameters.Add(strApiKey);

                        UpdateMap updateParameters = new UpdateMap();
                        int Ppos = (int)ProjectSpinner.SelectedItemId;
                        updateParameters.projectID = ProjectLookupList[Ppos];

                        int Apos = (int)ActivitySpinner.SelectedItemId;
                        updateParameters.activityID = ActivitiesLookupList[Apos];

                        TextView ActivityDescriptionText = FindViewById<TextView>(Resource.Id.description);
                        updateParameters.description = ActivityDescriptionText.Text;
                        Parameters.Add(updateParameters);

                        System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("updateActiveRecording", Parameters));
                        taskA.Wait();
                    }
                    else
                    {
                        Toast.MakeText(this, "There is no active timer running", ToastLength.Long).Show();
                    }
                };

                strApiKey = ap.getAccessKey("APIKEY");
                // Do we haave an api key?
                try
                {
                    if (string.IsNullOrEmpty(strApiKey))
                    {
                        // No, Let's log in

                        LoginToKimai();
                    }
                    else
                    {
                        //
                        ToggleButton togglebutton = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
                        // Let's get the data for any current active recording and update the start/stop button states

                        if (GetActiveRecord())
                        {
                            togglebutton.Checked = true;
                            RunUpdateLoopState = true;
                            Spinner CustomersSpinner = FindViewById<Spinner>(Resource.Id.spinnerCustomers);
                            CustomersSpinner.SetSelection(
                                GetDictionaryKeyFromValue(CustomerLookupList, CurrentCustomerInTimer)
                            );
                            Tv2 = FindViewById<TextView>(Resource.Id.textView2);
                            Tv2.Text = RunUpdateLoopState.ToString();
                        }
                        else
                        {
                            togglebutton.Checked = false;
                            RunUpdateLoopState = false;
                            Tv2 = FindViewById<TextView>(Resource.Id.textView2);
                            Tv2.Text = RunUpdateLoopState.ToString();
                        }

                        togglebutton.Click += (o, e) =>
                        {

                            // Perform action on clicks
                            if (togglebutton.Checked)
                            {
                                if (!GetActiveRecord())
                                {
                                    Spinner ProjectSpinner = FindViewById<Spinner>(Resource.Id.spinnerProjects);
                                    Spinner ActivitySpinner = FindViewById<Spinner>(Resource.Id.spinnerActivities);
                                    List<object> Parameters = new List<object>();

                                    int pos = (int)ProjectSpinner.SelectedItemId;
                                    Parameters.Add(strApiKey);
                                    Parameters.Add(
                                      ProjectLookupList[pos].ToString()

                                    );
                                    Parameters.Add(
                                        // ActivitiesLookupList[CurrentActivityInTimer].ToString()
                                        ActivitiesLookupList[
                                            (int)ActivitySpinner.SelectedItemId
                                            ].ToString()
                                    );

                                    System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("startRecord", Parameters));
                                    taskA.Wait();
                                    RunUpdateLoopState = true;
                                    DurationCount = 0;
                                }
                                else
                                {
                                    // We did not start a new recording we just switvhed to the active one.
                                    //Todo: this has big repercussions, as many devices could start mnay recordings with out stopping any exiting ones
                                    // If this proves to be a problem the best approach may be just to exit and warn the user multiple timers are running.
                                    Toast.MakeText(this, "There appears to be an active recording", ToastLength.Long).Show();
                                    RunUpdateLoopState = true;
                                    GetActiveRecord();
                                }
                            }
                            else
                            {
                                List<object> Parameters = new List<object>
                                {
                            strApiKey
                                };
                                System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("stopRecord", Parameters));
                                taskA.Wait();
                                RunUpdateLoopState = false;
                                DurationCount = 0;

                                GetActiveRecord();
                            }
                        };
                    }
                }
                catch (AggregateException ex)
                {
                    Toast mesg = Toast.MakeText(this, ex.Message, ToastLength.Long);
                    mesg.Show();
                }
            }
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
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
                    GetActiveRecord();
                    break;
                case Resource.Id.menu_about:
                    StartActivity(typeof(About));
                    break;
                case Resource.Id.menu_settings:
                    StartActivity(typeof(Settings));
                    break;
                case Resource.Id.menu_exit:
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Logins to kimai.
        /// </summary>
        private void LoginToKimai()
        {
            List<object> Parameters = new List<object>
            {
                ap.getAccessKey("USERNAME"),
                ap.getAccessKey("PASSWORD")
            };
            try
            {
                System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("authenticate", Parameters));
                taskA.Wait();
            }
            catch (AggregateException ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// Gets the active record.
        /// </summary>
        private bool GetActiveRecord()
        {
            TextView projectText = FindViewById<TextView>(Resource.Id.textView1);
            ToggleButton togglebutton = FindViewById<ToggleButton>(Resource.Id.toggleButton1);

            bool activeEvent = false;
            List<object> Parameters = new List<object>
            {
                strApiKey
            };
            try
            {
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
                    // Tepoary text for debugging buttons and spinners
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
                    TextView ActivityDescriptionText = FindViewById<TextView>(Resource.Id.description);
                    ActivityDescriptionText.Text = ActiveRecordingObject.Result.Items[0].description;
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

                    // We have a lookup value so we need an index to use
                    CurrentCustomerInTimer = ActiveRecordingObject.Result.Items[0].customerID;
                    CurrentProjectInTimer = ActiveRecordingObject.Result.Items[0].projectID;
                    CurrentActivityInTimer = ActiveRecordingObject.Result.Items[0].activityID;
                    PopulateCustomersSpinner();

                }
                else
                {
                    projectText.Text = "No Active Recording.";
                    //   TimerViewer.Text = "00:00:00";
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
        /// Populates the customers spinner.
        /// </summary>
        private void PopulateCustomersSpinner()
        {
            Spinner CustomersSpinner = FindViewById<Spinner>(Resource.Id.spinnerCustomers);
            CustomersSpinner.SetSelection(0);
            try
            {
                // Clear the customer lookup table
                CustomerLookupList.Clear();
                var customers = db.Query<KimaiDatadase.Customer>("SELECT * FROM Customer");
                int count = customers.Count;
                if (count > 0)
                {
                    var customeradapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item);
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

                CustomersSpinner.ItemSelected += CustomerSpinnerItemSelected;
                CustomersSpinner.SetSelection(GetDictionaryKeyFromValue(CustomerLookupList, CurrentCustomerInTimer));

            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
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

            // Now populate the Project spinner based on the selected customerID
            PopulateProjectsSpinner(CustomerLookupList[e.Position]);
            //CurrentCustomerInTimer = e.Position;
        }

        /// <summary>
        /// Populates the projects spinner.
        /// </summary>
        /// <param name="customerID">Customer identifier.</param>
        private void PopulateProjectsSpinner(int customerID)
        {
            Spinner ProjectsSpinner = FindViewById<Spinner>(Resource.Id.spinnerProjects);
            ProjectsSpinner.SetSelection(0);
            try
            {
                ProjectLookupList.Clear();
                var projects = db.Query<KimaiDatadase.Project>("SELECT * FROM Project WHERE CustomerID = ?", customerID);
                int index = 0;
                int count = projects.Count;
                if (count > 0)
                {
                    var projectadapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item);
                    foreach (var project in projects)
                    {
                        projectadapter.Add(project.Name);
                        ProjectLookupList.Add(index++, project.ProjectID);
                    }
                    ProjectsSpinner.Adapter = projectadapter;
                }
                ProjectsSpinner.ItemSelected += ProjectSpinnerItemSelected;
                ProjectsSpinner.SetSelection(
                        GetDictionaryKeyFromValue(ProjectLookupList, CurrentProjectInTimer)
                );
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
            // Now populate the activities spinner bases on the Projectid selected
            PopulateActivitiesSpinner(ProjectLookupList[e.Position]);
        }


        /// <summary>
        /// Populates the activities spinner.
        /// </summary>
        /// <param name="projectID">Project identifier.</param>
        private void PopulateActivitiesSpinner(int projectID)
        {
            Spinner ActivitiesSpinner = FindViewById<Spinner>(Resource.Id.spinnerActivities);
            ActivitiesSpinner.SetSelection(0);
            try
            {

                ActivitiesLookupList.Clear();
                var activities = db.Query<ProjectActivity>("SELECT * FROM ProjectActivity WHERE ProjectID = ?", projectID);
                int index = 0;
                int count = activities.Count;
                if (count > 0)
                {
                    var activityadapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item);
                    foreach (var activity in activities)
                    {
                        activityadapter.Add(activity.Name);
                        ActivitiesLookupList.Add(index++, activity.ActivityID);
                    }
                    ActivitiesSpinner.Adapter = activityadapter;

                }

                ActivitiesSpinner.SetSelection(
                    GetDictionaryKeyFromValue(ActivitiesLookupList, CurrentActivityInTimer)
                );
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }

        private void ActivitySpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // CurrentActivityInTimer = e.Position;
        }

        /// <summary>
        /// Rerurns the dictionary key (index) from a given value
        /// </summary>
        /// <returns>The by value.</returns>
        /// <param name="dict">Dict.</param>
        /// <param name="val">Value.</param>
        public static int GetDictionaryKeyFromValue(Dictionary<int, int> dict, int val)
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
            return key;
        }

    }

}

