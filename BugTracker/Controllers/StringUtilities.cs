using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Controllers
{
    public class StringUtilities
    {
        public static string Shorten(string s, int maxValue)
        {
            if (s.Length >= maxValue)
            {
                return s.Substring(0, maxValue) + "...";
            }
            else
            {
                return s;
            }
        }
    }
}