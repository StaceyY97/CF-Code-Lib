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
            * Description: If Year is Unknown, the Month & Day Should be unknown. If Month is unknown then Day should be unknown.
           **************************************************************************/

            const int MARKING_GROUP_ID = 1;
            const bool ANSWER_ON_CHANGE = false;
            const bool CLOSE_ON_CHANGE = false;
            string querytext_Year = "If Year is Unknown, the Month & Day Should be unknown. Please review & update.";
            string querytext_Month = "Month is unknown but Day is known. Please verify & update.";

            IActionFunctionParams afp = (IActionFunctionParams)ThisObject;
            IDataPoint dp_action = Rave.Context.ActionDataPoint;

            if (dp_action == null) return null;

            bool doQuery_Year = false, doQuery_Month = false;

            if (DoCheck(dp_action, true))
            {
                List<bool> UNKStatus = GetUNKStatus(dp_action.Data);
                if (UNKStatus != null)
                {
                    bool isUNK_year = UNKStatus[0];
                    bool isUNK_month = UNKStatus[1];
                    bool isUNK_date = UNKStatus[2];
                    if (isUNK_year && (!isUNK_month || !isUNK_date))
                    {
                        doQuery_Year = true;
                    }
                    else if (!isUNK_year && isUNK_month && !isUNK_date)
                    {
                        doQuery_Month = true;
                    }
                }
            }

            Rave.Marking.PerformQueryAction(dp_action,querytext_Year, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, doQuery_Year, afp.CheckRef, afp.CheckHash);
            Rave.Marking.PerformQueryAction(dp_action,querytext_Month, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, doQuery_Month, afp.CheckRef , afp.CheckHash);

            return null;
        }

        bool DoCheck(IDataPoint dp, bool IsDate, bool checkEmpty = true) => IsDate ? IsValidDp(dp, checkEmpty) && IsValidDate(dp) : IsValidDp(dp, checkEmpty);
        bool IsValidDp(IDataPoint dp, bool checkEmpty) => dp != null && dp.Active && dp.IsVisibleRaw && (checkEmpty ? dp.Data != string.Empty : true);
        bool IsValidDate(IDataPoint dp) => dp != null && dp.Active && dp.IsVisibleRaw && (dp.StandardValue is DateTime || dp.StandardValue is TimeSpan);

        List<bool> GetUNKStatus(string str_data)
        {
            string[] devided_data = str_data.Split(' ');
            bool isUNK_date = false, isUNK_year = false, isUNK_month = false;
            if (devided_data.Length == 3)
            {
                isUNK_date = devided_data[0] == "UN";
                isUNK_month = devided_data[1] == "UNK";
                isUNK_year = devided_data[2] == "UNKN";
                return new List<bool>() { isUNK_year, isUNK_month, isUNK_date };
            }
            else return null;
        }
    }
}
