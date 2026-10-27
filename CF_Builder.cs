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

    public class ValueComp_CMAETERM_AETERM : CustomFunctionBase
    {
        public object Eval(object ThisObject)
        {
            /**************************HEADER INFORMATION******************************
            * Programmer Name: [Your Name]
            * Description: Check if CMAETERM matches any AETERM value.
            *              Opens query when CMAETERM does not match any AETERM.
            **************************************************************************/

            IActionFunctionParams afp = (IActionFunctionParams)ThisObject;
            IDataPoint dpAction = Rave.Context.ActionDataPoint;

            if (dpAction == null) return null;
            ISubject subject = dpAction.Record.Subject;
            if (subject == null) return null;

            string fieldOID_compare = "AETERM";
            string formOID_compare = "AE";
            string fieldOID_query = "CMAETERM";
            string formOID_query = "CM";

            string queryMessage = "Term's not match with any of the AETERM, please confirm";
            const bool closeOnChange = true;
            const bool answerOnChange = true;
            const int markingGroup = 1;

            // Fetch all AETERM values from AE log form (subject-wide scope)
            IReadOnlyList<IDataPoint> dpsAETERM = subject.GetDataPointsByOidPath(fieldOID_compare, formOID_compare, null);

            // Fetch all CMAETERM values from CM log form (subject-wide scope)
            IReadOnlyList<IDataPoint> dpsCMAETERM = subject.GetDataPointsByOidPath(fieldOID_query, formOID_query, null);

            // If action field is CMAETERM, only check that one; otherwise check all
            IReadOnlyList<IDataPoint> dpsToCheck = dpAction.Field.OID == fieldOID_query 
                ? new List<IDataPoint>() { dpAction } 
                : dpsCMAETERM;

            foreach (IDataPoint dpQuery in dpsToCheck)
            {
                if (dpQuery == null || !dpQuery.Active) continue;

                bool openQuery = false;

                if (dpQuery.Data != string.Empty)
                {
                    // Check if this CMAETERM value matches any AETERM value
                    bool foundMatch = false;
                    foreach (IDataPoint dpCompare in dpsAETERM)
                    {
                        if (dpCompare == null || !dpCompare.Active) continue;
                        if (dpCompare.Data != string.Empty && string.Equals(dpCompare.Data.Trim(), dpQuery.Data.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                    if (!foundMatch) openQuery = true;
                }

                Rave.Marking.PerformQueryAction(dpQuery, queryMessage, markingGroup, answerOnChange, closeOnChange, openQuery, afp.CheckRef, afp.CheckHash);
            }

            return null;
        }
    }
}
