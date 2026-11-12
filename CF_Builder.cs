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

    public class DSL_CMAETERM : CustomFunctionBase
    {
        public object Eval(object ThisObject)
        {
            /**************************HEADER INFORMATION******************************
            * Programmer Name: [Your Name]
            * Description: Dynamic Search List for CMAETERM (form CM, subject-wide).
            *              Options are built from AE log lines (subject-wide) in the
            *              format <RecordPosition>-<AETERM>-<AESTDAT>.
            *              AETERM is validated with DoCheck(dp, false) (non-null,
            *              active, visible, non-empty); invalid lines and the master
            *              record are skipped. AETERM is truncated to 60 characters.
            *              AESTDAT is validated with DoCheck(dp, true) (valid + full
            *              date); an invalid date keeps the line with an empty date
            *              part (pos-term-).
            **************************************************************************/

            //Variable Declaration Start -------
            string oid_folder_target = null;
            string oid_form_target = "AE";
            string oid_term = "AETERM", oid_dat = "AESTDAT";
            //Variable Declaration End -------

            KeyValueCollection KVC = new KeyValueCollection();
            try
            {
                IDynamicSearchParams DSP = (IDynamicSearchParams)ThisObject;
                if (DSP == null || DSP.DataPoint == null) return KVC;

                ISubject subject = DSP.DataPoint.Record.Subject;
                if (subject == null) return KVC;

                IReadOnlyList<IDataPoint> dps_terms = subject.GetDataPointsByOidPath(oid_term, oid_form_target, oid_folder_target);
                if (dps_terms != null && dps_terms.Count > 0)
                {
                    for (int i = 0; i < dps_terms.Count; i++)
                    {
                        IDataPoint dp_term = dps_terms[i];

                        // Skip invalid term (null/inactive/hidden/empty) and master record
                        if (!DoCheck(dp_term, false) || dp_term.Record.RecordPosition == 0) continue;

                        IDataPoint dp_dat = dp_term.Record.AllDataPoints.FindByFieldOid(oid_dat);

                        string str_rcPos = dp_term.Record.RecordPosition.ToString();
                        // Truncate long terms to 60 characters
                        string str_term = dp_term.Data.Trim().Length >= 60 ? dp_term.Data.Trim().Substring(0, 60) : dp_term.Data.Trim();
                        // Invalid or partial start date -> empty date part
                        string str_dat = DoCheck(dp_dat, true) ? dp_dat.UserValue.ToString().Trim() : "";

                        string ds = $"{str_rcPos}-{str_term}-{str_dat}";
                        KVC.Add(new KeyValue(ds, ds));
                    }
                }
            }
            catch {}
            return KVC;
        }

        bool DoCheck(IDataPoint dp, bool IsDate, bool checkEmpty = true) => IsDate ? IsValidDp(dp, checkEmpty) && IsValidDate(dp) : IsValidDp(dp, checkEmpty);
        bool IsValidDp(IDataPoint dp, bool checkEmpty) => dp != null && dp.Active && dp.IsVisibleRaw && (checkEmpty ? dp.Data != string.Empty : true);
        bool IsValidDate(IDataPoint dp) => dp != null && dp.Active && dp.IsVisibleRaw && (dp.StandardValue is DateTime || dp.StandardValue is TimeSpan);
    }
}
