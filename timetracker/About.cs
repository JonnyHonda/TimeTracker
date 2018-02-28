
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
    [Activity(Label = "About")]
    public class About : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.About);
            getVersionInfo();

        }


        //get the current version number and name
        private void getVersionInfo()
        {
            String versionName = "";
            int versionCode = -1;
            versionCode = Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionCode;
            versionName = Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName;

            TextView textViewVersionInfo = FindViewById<TextView>(Resource.Id.versionInfo);
            textViewVersionInfo.Append(String.Format("\nVersion number: {0}.{1}", versionName, versionCode));
        }
    }
}
