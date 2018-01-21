using System;
using Android.Content;
using Android.Preferences;

namespace TimeTracker
{
    public class AppPreferences
    {
        private ISharedPreferences mSharedPrefs;
        private ISharedPreferencesEditor mPrefsEditor;
        private Context mContext;

        //private static String PREFERENCE_ACCESS_KEY = "PREFERENCE_ACCESS_KEY";

        public AppPreferences(Context context)
        {
            this.mContext = context;
            mSharedPrefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            mPrefsEditor = mSharedPrefs.Edit();
        }

/**
 * Preference Manager
 * */
        public void saveAccessKey(String preferenceAccessKey,string value, bool is_mandatory = false)
        {
            if (string.IsNullOrEmpty(value) && is_mandatory == true)
            {
                throw new ArgumentException(preferenceAccessKey + " cannot be null or empty string", nameof(value));
            }
            else
            {
                mPrefsEditor.PutString(preferenceAccessKey, value);
                mPrefsEditor.Commit();

            }

        }

        public string getAccessKey(String preferenceAccessKey)
        {
            return mSharedPrefs.GetString(preferenceAccessKey, "");
        }

        /*
         * clear the preferences
         */
        public void clearPrefs(){
            mPrefsEditor.Clear().Commit(); 
        }

    }

}

