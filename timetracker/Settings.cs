using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JsonKimaiMaps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using static TimeTracker.KimaiDatadase;

namespace TimeTracker
{
    [Activity(Label = "Settings")]
    public class Settings : Activity
    {
        public SQLiteConnection db;
        protected override void OnCreate(Bundle savedInstanceState)
        { 
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);
            string dbPath = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "localkimaidata.db3");
            
            db = new SQLiteConnection(dbPath);
            // Create your application here
            SetContentView(Resource.Layout.Settings);
            Context mContext = Application.Context;
            AppPreferences ap = new AppPreferences(mContext);
            TextView txtField = FindViewById<TextView>(Resource.Id.edit_url);
            txtField.Text = ap.getAccessKey("URL");

            txtField = FindViewById<TextView>(Resource.Id.edit_username);
            txtField.Text = ap.getAccessKey("USERNAME");

            txtField = FindViewById<TextView>(Resource.Id.edit_password);
            txtField.Text = ap.getAccessKey("PASSWORD");

            Button sql_lite = FindViewById<Button>(Resource.Id.btn_view_data);
            sql_lite.Click += delegate
            {
                StartActivity(typeof(SqliteActivity));
            };


            Button save_button = FindViewById<Button>(Resource.Id.btn_save);
            save_button.Click += delegate
            {
                char[] charsToTrim = { '*', ' ', '\'' };
                TextView url = FindViewById<TextView>(Resource.Id.edit_url);
                string strApiUrl = url.Text.Trim(charsToTrim);

                TextView username = FindViewById<TextView>(Resource.Id.edit_username);
                string strApiUserName = username.Text.Trim(charsToTrim);

                TextView password = FindViewById<TextView>(Resource.Id.edit_password);
                string strApiPassword = password.Text.Trim(charsToTrim);

                TextView statusMessage = FindViewById<TextView>(Resource.Id.status_message);

                ap.saveAccessKey("URL", strApiUrl, true);
                ap.saveAccessKey("USERNAME", strApiUserName, true);
                ap.saveAccessKey("PASSWORD", strApiPassword, true);

                List<object> Parameters = new List<object>();
                Parameters.Add(strApiUserName);
                Parameters.Add(strApiPassword);
                KimaiServer MyKimai = new KimaiServer(strApiUrl + "/core/json.php");
                System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("authenticate", Parameters));
                taskA.Wait();

                AuthenticateMap AuthObject = new AuthenticateMap();
                AuthObject = JsonConvert.DeserializeObject<AuthenticateMap>(MyKimai.JsonResultString);
                bool Success = AuthObject.Result.Success;
                if (Success)
                {
                    string apikey = AuthObject.Result.Items[0].apiKey;
                    ap.saveAccessKey("APIKEY", AuthObject.Result.Items[0].apiKey, true);
                    Parameters.Clear();
                    Parameters.Add(apikey);
                    System.Threading.Tasks.Task CustomersTask = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("getCustomers", Parameters));
                    CustomersTask.Wait();
                    CustomerMap CustomerObject = new CustomerMap();
                    CustomerObject = JsonConvert.DeserializeObject<CustomerMap>(MyKimai.JsonResultString);
                    Success = CustomerObject.Result.Success;
                    if (Success)
                    {
                        db.CreateTable<KimaiDatadase.Customer>();
                        db.DeleteAll<KimaiDatadase.Customer>();
                        var newCustomer = new KimaiDatadase.Customer();
                        foreach (JsonKimaiMaps.Customer item in CustomerObject.Result.Items)
                        {
                            newCustomer.CustomerID = item.customerID;
                            newCustomer.Name = item.name;
                                
                            db.Insert(newCustomer);
                        }
                    }

                    Parameters.Clear();
                    Parameters.Add(apikey);
                    Parameters.Add("1"); // nest in the activities
                    System.Threading.Tasks.Task ProjectsTask = System.Threading.Tasks.Task.Factory.StartNew(() => MyKimai.ConnectAsync("getProjects", Parameters));
                    ProjectsTask.Wait();
                    ProjectMap ProjectObject = new ProjectMap();
                    ProjectObject = JsonConvert.DeserializeObject<ProjectMap>(MyKimai.JsonResultString);
                    Success = ProjectObject.Result.Success;
                    if(Success){
                        db.CreateTable<KimaiDatadase.Project>();
                        db.DeleteAll<KimaiDatadase.Project>();
                        db.CreateTable<ProjectActivity>();
                        db.DeleteAll<ProjectActivity>();
                        var newProject = new KimaiDatadase.Project();
                        foreach (JsonKimaiMaps.Project item in ProjectObject.Result.Items)
                        {
                            newProject.CustomerID = item.customerID;
                            newProject.ProjectID = item.projectID;
                            newProject.Name = item.name;
                            var newActivity = new ProjectActivity();
                            foreach (JsonKimaiMaps.Task i in item.Tasks){
                                newActivity.ActivityID = i.activityID;
                                newActivity.ProjectID = item.projectID;
                                newActivity.Name = i.name;
                                db.Insert(newActivity);
                            }
                            db.Insert(newProject);
                        }
                        StartActivity(typeof(MainActivity));
                    }
                }else{
                    Toast mesg = Toast.MakeText(this, "Login Failed", ToastLength.Short);
                    mesg.Show();
                }
            };

            Button cancel_button = FindViewById<Button>(Resource.Id.btn_cancel);
            cancel_button.Click += delegate
            {
                StartActivity(typeof(MainActivity));
            };
        }
    }
}
