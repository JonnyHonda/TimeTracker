
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using TimeTracker.kimai.tsgapis.com;
using static TimeTracker.KimaiDatadase;

namespace TimeTracker
{
    [Activity(Label = "Settings", MainLauncher = true)]
    public class Settings : Activity
    {
        public bool debug = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Settings);
            Context mContext = Android.App.Application.Context;
            AppPreferences ap = new AppPreferences(mContext);
            TextView txtField = FindViewById<TextView>(Resource.Id.edit_url);
            txtField.Text = ap.getAccessKey("URL");

            txtField = FindViewById<TextView>(Resource.Id.edit_username);
            txtField.Text = ap.getAccessKey("USERNAME");

            txtField = FindViewById<TextView>(Resource.Id.edit_password);
            txtField.Text = ap.getAccessKey("PASSWORD");


            // Get our button from the layout resource,
            // and attach an event to it
            // Save the Settings
            Button save_button = FindViewById<Button>(Resource.Id.btn_settings);

            string dbPath = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "localkimaidata.db3");
            var db = new SQLiteConnection(dbPath);

            string strApiUrl = ap.getAccessKey("URL");
            string strApiUserName = ap.getAccessKey("USERNAME");
            string strApiPassword = ap.getAccessKey("PASSWORD");
            string strApiKey = ap.getAccessKey("APIKEY");

            db.CreateTable<Customer>();
            db.DeleteAll<Customer>();
            //ThreadPool.QueueUserWorkItem(o => populateCustomerTable(strApiUrl, strApiKey, db));
                    populateCustomerTable(strApiUrl, strApiKey, db);

            db.CreateTable<Project>();
            db.DeleteAll<Project>();
   //         ThreadPool.QueueUserWorkItem(o => populateProjectTable(strApiUrl, strApiKey, db));
                    populateProjectTable(strApiUrl, strApiKey, db);;

            db.CreateTable<ProjectActivity>();
            db.DeleteAll<ProjectActivity>();
           // ThreadPool.QueueUserWorkItem(o => populateActivityTable(strApiUrl, strApiKey, db));
                   populateActivityTable(strApiUrl, strApiKey, db);

            save_button.Click += delegate
            {
                // Save App Prefs

                try
                {
                    char[] charsToTrim = { '*', ' ', '\'' };
                    TextView url = FindViewById<TextView>(Resource.Id.edit_url);
                    ap.saveAccessKey("URL", url.Text.Trim(charsToTrim),true);

                    TextView username = FindViewById<TextView>(Resource.Id.edit_username);
                    ap.saveAccessKey("USERNAME", username.Text.Trim(charsToTrim),true);


                    TextView password = FindViewById<TextView>(Resource.Id.edit_password);
                    ap.saveAccessKey("PASSWORD", password.Text.Trim(charsToTrim),true);

                     strApiUrl = ap.getAccessKey("URL");
                     strApiUserName = ap.getAccessKey("USERNAME");
                     strApiPassword = ap.getAccessKey("PASSWORD");
                    strApiKey = ap.getAccessKey("APIKEY");
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
                    // StartActivity(new Intent(Application.Context, typeof(MainActivity)));


                }
                catch (Exception)
                {
                    Toast.MakeText(ApplicationContext, "All fields are required", ToastLength.Long).Show();
                    
                }
            };


            Button sql_button = FindViewById<Button>(Resource.Id.btn_view_data);
            sql_button.Click += delegate
            {
                StartActivity(typeof(SqliteActivity));
            };

            Button cancel_button = FindViewById<Button>(Resource.Id.btn_cancel);
            cancel_button.Click += delegate
            {
                StartActivity(typeof(MainActivity));
            };
        }

        /// <summary>
        /// Populates the customer table.
        /// </summary>
        /// <param name="strApiUrl">String API URL.</param>
        /// <param name="strApiKey">String API key.</param>
        /// <param name="db">Db.</param>
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
                                newCustomer.CustomerID = Convert.ToInt16(n["value"].InnerText);
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


        /// <summary>
        /// Populates the project table.
        /// </summary>
        /// <param name="strApiUrl">String API URL.</param>
        /// <param name="strApiKey">String API key.</param>
        /// <param name="db">Db.</param>
        private static void populateProjectTable(string strApiUrl, string strApiKey, SQLiteConnection db)
        {
            try
            {
                Kimai_Remote_ApiService Service = new Kimai_Remote_ApiService(strApiUrl + "/core/soap.php");
                Service.AllowAutoRedirect = true;

                //Get details of the active recording
                object responseObject = Service.getProjects(strApiKey, true);

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
                                newProject.ProjectID = Convert.ToInt16(n["value"].InnerText);
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
        /// <summary>
        /// Populates the activity table.
        /// </summary>
        /// <param name="strApiUrl">String API URL.</param>
        /// <param name="strApiKey">String API key.</param>
        /// <param name="db">Db.</param>
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
                    object responseObject = Service.getTasks(strApiKey, project.ID);

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
                                    newActivity.ActivityID = Convert.ToInt16(n["value"].InnerText);
                                    newActivity.ProjectID = project.ID;
                                    break;
                            }
                        }
                        db.Insert(newActivity);

                    }
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }


    }
}
