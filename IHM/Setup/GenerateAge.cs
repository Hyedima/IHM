using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IHM.Setup
{
    public class GenerateAge
    {
        public static int Age(DateTime BirthDate)
        {
            DateTime Today = DateTime.Now;
            DateTime bDate = BirthDate;

            int Age = Today.Date.Year - bDate.Year;
            return Age;
        }
    }
}