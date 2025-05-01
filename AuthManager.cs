using System;
using System.Windows;

namespace JobNest
{
    public static class AuthManager
    {
        public static User CurrentUser { get; private set; }

        public static bool IsAuthenticated => CurrentUser != null;

        public static void Login(User user)
        {
            CurrentUser = user;
            if (Application.Current != null)
            {
                Application.Current.Properties["UserId"] = user.UserId;
            }
        }

        public static void Logout()
        {
            CurrentUser = null;
            if (Application.Current != null)
            {
                Application.Current.Properties.Remove("UserId");
            }
        }
    }
}