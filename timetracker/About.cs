
using System;

using Android.App;
using Android.OS;
using Android.Widget;

namespace TimeTracker.Resources
{
    [Activity(Label = "About", MainLauncher = true)]
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
            String author;
            versionCode = Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionCode;
            versionName = Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName;
                
            TextView textViewVersionInfo = FindViewById<TextView>(Resource.Id.versionInfo);
            textViewVersionInfo.Append(String.Format("\nVersion number: {0}.{1}", versionName, versionCode));
        }
    }

}
