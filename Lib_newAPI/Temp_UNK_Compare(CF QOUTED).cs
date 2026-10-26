using Medidata.Core.Common;
using Medidata.Core.Common.Utilities;
using Medidata.Core.Objects;
using Medidata.Interfaces;
using Medidata.Interfaces.Clinical;
using Medidata.Interfaces.Refs;
using Medidata.RaveWebServices.Outbound;
using Medidata.Utilities;
using Medidata.Utilities.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CustomFunctions
{
    using Medidata.Core.Objects.DCFs;
    #region System Class - Do Not Modify
    using Medidata.CustomFunctions.Debug;
    using Mono.CSharp;
    using System.Diagnostics;
    using System.Windows.Forms;
    using static log4net.Appender.ColoredConsoleAppender;

    /// <summary>
    /// Runs the CustomFunction Development Utility.
    /// </summary>
    public class _SystemClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="_SystemClass"/> class.
        /// </summary>
        public _SystemClass()
        {
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            System.Windows.Forms.Application.Run(new MainForm());
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            if (!CacheManager.IsThreadAbortException(exception)) System.Windows.Forms.MessageBox.Show(exception.ToString());
        }
    }
    #endregion

    /// <summary>
    /// Sample CustomFunction.
    /// </summary>
    public class CustomFunction1 : CustomFunctionBase
    {
        public object Eval(object ThisObject)
        {
            /**************************HEADER INFORMATION******************************
            * Programmer Name:Stacey Yang
            * Applies when atleast one data field allow UNK date
            * Compares two datetime sequentially by Year, Month, and Day.
            * Only the following comparison types are allowed in format of string: Equal, NotEqual, GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo
           **************************************************************************/

            object[] inputParameters = (object[])ThisObject;
            IDataPoint dp1 = (IDataPoint)inputParameters[0];
            IDataPoint dp2 = (IDataPoint)inputParameters[1];
            string compareType = (string)inputParameters[2];

            bool result = false;
            UNKStatus unks_dp1 = InitialUNKDate(dp1);
            UNKStatus unks_dp2 = InitialUNKDate(dp2);

            if (unks_dp1.isValid && unks_dp2.isValid)
            {
                DateTime dt1 = unks_dp1.date, dt2 = unks_dp2.date;
                if (!unks_dp1.isUNK_year && !unks_dp1.isUNK_year)
                {
                    if (DoCompare(dt1.Year, dt2.Year, compareType)) result = true;
                    else if (dt1.Year == dt2.Year)
                    {
                        if (!unks_dp1.isUNK_month && !unks_dp2.isUNK_month)
                        {
                            if (DoCompare(dt1.Month, dt2.Month, compareType)) result = true;
                            else if (dt1.Month == dt2.Month)
                            {
                                if (!unks_dp1.isUNK_date && !unks_dp2.isUNK_date)
                                {
                                    if (DoCompare(dt1.Day, dt2.Day, compareType)) result = true;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
        bool DoCheck(IDataPoint dp, bool IsDate, bool checkEmpty = true) => IsDate ? IsValidDp(dp, checkEmpty) && IsValidDate(dp) : IsValidDp(dp, checkEmpty);
        bool IsValidDp(IDataPoint dp, bool checkEmpty) => dp != null && dp.Active && dp.IsVisibleRaw && (checkEmpty ? dp.Data != string.Empty : true);
        bool IsValidDate(IDataPoint dp) => dp != null && dp.Active && dp.IsVisibleRaw && (dp.StandardValue is DateTime || dp.StandardValue is TimeSpan);

        UNKStatus InitialUNKDate(IDataPoint dp)
        {
            UNKStatus unks = new UNKStatus();

            if (DoCheck(dp, true))
            {
                unks.date = (DateTime)dp.StandardValue;

                string str_data = dp.Data;
                string[] devided_data = str_data.Split(' ');
                bool isUNK_date = false, isUNK_year = false, isUNK_month = false;
                if (devided_data.Length == 3)
                {
                    unks.isUNK_date = devided_data[0] == "UN";
                    unks.isUNK_month = devided_data[1] == "UNK";
                    unks.isUNK_year = devided_data[2] == "UNKN";

                    unks.isValid = true;
                }
            }

            return unks;
        }
        bool DoCompare(int date1, int date2, string type)
        {
            switch (type)
            {
                case "Equal":
                    return date1 == date2;
                case "NotEqual":
                    return date1 != date2;
                case "GreaterThan":
                    return date1 > date2;
                case "GreaterThanOrEqualTo":
                    return date1 >= date2;
                case "LessThan":
                    return date1 < date2;
                case "LessThanOrEqualTo":
                    return date1 <= date2;
                default:
                    return false;
            }
        }

        class UNKStatus
        {
            public DateTime date { get; set; } = DateTime.MinValue;
            public bool isUNK_year { get; set; } = false;
            public bool isUNK_month { get; set; } = false;
            public bool isUNK_date { get; set; } = false;
            public bool isValid { get; set; } = false;
        }
    }
}
