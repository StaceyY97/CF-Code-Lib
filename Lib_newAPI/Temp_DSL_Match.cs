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
            * Get and compare target AE by DSL value
           **************************************************************************/

            //Variable Declaration Start -------

            string oid_folder_tartget = "AE";
            string oid_form_tartget = "AE";
            string oid_compare = "AEMED";
            string str_trigger = "N";

            string oid_form_query = "CM";
            string oid_folder_query = "";
            string oid_query_head = "CMAENO";
            int lowLim = 1, upLim = 5;

            char char_divider = '-';

            const bool closeOnChange = true;
            const bool answerOnChange = true;
            const int markingGroup = 1;
            string queryMessage = "Reason for Use is Adverse Event & AE ID has been provided; however, \"Any medications used to treat this event?\" is No for the same AE. Please review and update.";

            //Variable Declaration End -------

            IActionFunctionParams afp = (IActionFunctionParams)ThisObject;
            IDataPoint dp_action = Rave.Context.ActionDataPoint;

            if (dp_action == null) return null;
            ISubject subj = dp_action.Record.Subject;
            string str_enterOID = dp_action.Field.OID;

            IReadOnlyList<IDataPoint> dps_query = str_enterOID.StartsWith(oid_query_head) ? new List<IDataPoint>() { dp_action } : GatherAllQueryDps(subj, lowLim, upLim, oid_query_head, oid_form_query);
            IReadOnlyList<IDataPoint> dps_aecmyn = subj.GetDataPointsByOidPath(oid_compare, oid_form_tartget, oid_folder_tartget);

            foreach (IDataPoint dp_query in dps_query)
            {
                if (dp_query == null || !dp_query.Active) continue;

                bool doQuery = false;
                if (dp_query.Data != string.Empty)
                {
                    string[] devided = dp_query.Data.Split(char_divider);
                    if (devided.Length >= 3 && int.TryParse(devided[0], out int recordPos) && CheckContain(recordPos,dps_aecmyn, str_trigger)) doQuery =true;
                }
                Rave.Marking.PerformQueryAction(dp_query,queryMessage, markingGroup, answerOnChange, closeOnChange, doQuery, afp.CheckRef, afp.CheckHash);
            }
            return null;
        }
        IReadOnlyList<IDataPoint> GatherAllQueryDps(ISubject subj, int lowLim, int upLim, string head, string formOID)
        {
            List<IDataPoint> allDps = new List<IDataPoint>();

            if (subj == null || lowLim > upLim) return allDps;

            for (int i = lowLim; i <= upLim; i++)
            {
                IReadOnlyList<IDataPoint> dps_temp = subj.GetDataPointsByOidPath($"{head}{i.ToString()}", formOID, null);
                foreach (IDataPoint dp in dps_temp)
                {
                    if (dp != null && dp.Record.RecordPosition != 0) allDps.Add(dp);
                }
            }
            return allDps;
        }

        bool CheckContain(int int_pos,IReadOnlyList<IDataPoint> dps,string matchValue)
        {
            foreach (IDataPoint dp in dps)
            {
                if (dp != null && dp.Record.RecordPosition == int_pos && dp.Data == matchValue) return true;
            }
            return false;
        }
    }
}
