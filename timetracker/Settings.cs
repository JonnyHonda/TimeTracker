
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TimeTracker
{
    [Activity(Label = "Settings", MainLauncher = false)]
    public class Settings : Activity
    {
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
            Button button = FindViewById<Button>(Resource.Id.btn_settings);

            button.Click += delegate
            {
                // Save App Prefs

                try
                {
                    TextView url = FindViewById<TextView>(Resource.Id.edit_url);
                    ap.saveAccessKey("URL", url.Text,true);

                    TextView username = FindViewById<TextView>(Resource.Id.edit_username);
                    ap.saveAccessKey("USERNAME", username.Text,true);


                    TextView password = FindViewById<TextView>(Resource.Id.edit_password);
                    ap.saveAccessKey("PASSWORD", password.Text,true);

                    StartActivity(new Intent(Application.Context, typeof(MainActivity)));

                }
                catch (Exception)
                {
                    Toast.MakeText(ApplicationContext, "All fields are required", ToastLength.Long).Show();
                    
                }
            };
        }
    }
}
