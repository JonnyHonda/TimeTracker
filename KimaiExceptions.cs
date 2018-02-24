using System;
namespace Kimai
{
    [Serializable]
    class LoginFailedException : Exception
    {
        public LoginFailedException()
            : base(String.Format("Log in Failed using user name "))
        {

        }

    }

    class ToggleTimerException : Exception
    {
        public ToggleTimerException()
            : base(String.Format("Could not toggle timer"))
        {

        }

    }
}
