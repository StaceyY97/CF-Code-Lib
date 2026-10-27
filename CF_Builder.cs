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

    public class DateComp_DSDAT_AESTDAT : CustomFunctionBase
    {
        public object Eval(object ThisObject)
        {
            /**************************HEADER INFORMATION******************************
            * Programmer Name: [Your Name]
            * Description: Compare DSDAT (DS form in EOT folder) against maximum AESTDAT (AE log form).
            *              Opens query when DSDAT is earlier than the maximum AESTDAT.
            *              Uses interface types and Basical_Func.cs helper patterns.
            **************************************************************************/

            IActionFunctionParams afp = (IActionFunctionParams)ThisObject;
            ISubject subject = Rave.Context.ActionDataPoint.Record.Subject;

            if (subject == null) return null;

            string QT = "End Date prior to the last AE date, please confirm";
            bool openQuery = false;

            // Fetch all AESTDAT values from AE log form (subject-wide scope)
            IReadOnlyList<IDataPoint> dpsAESTDAT = subject.GetDataPointsByOidPath("AESTDAT", "AE", null);
            
            // Get maximum AESTDAT
            DateTime maxAESTDAT = GetMaxDate(dpsAESTDAT);

            // Get DSDAT from DS form in EOT folder (non-log form)
            IDataPoint dpDSDAT = GetDP("DSDAT", "DS", "EOT", subject);

            // Compare dates
            if (dpDSDAT != null && dpDSDAT.Active && maxAESTDAT != DateTime.MinValue)
            {
                if (dpDSDAT.StandardValue is DateTime && !dpDSDAT.Data.Contains("UN"))
                {
                    DateTime dsdatValue = Convert.ToDateTime(dpDSDAT.StandardValue);
                    if (dsdatValue < maxAESTDAT)
                    {
                        openQuery = true;
                    }
                }
            }

            // Open or close query on DSDAT
            if (dpDSDAT != null)
            {
                Rave.Marking.PerformQueryAction(dpDSDAT, QT, 1, false, false, openQuery, afp.CheckRef, afp.CheckHash);
            }

            return null;
        }

        IDataPoint GetDP(string field_Oid, string form_Oid, string folder_Oid, ISubject subj) 
            => subj.Instances.FindByFolderOid(folder_Oid)?.DataPages.FindByFormOid(form_Oid)?.MasterRecord.AllDataPoints.FindByFieldOid(field_Oid);


        DateTime GetMaxDate(IReadOnlyList<IDataPoint> dps)
        {
            DateTime maxDate = DateTime.MinValue;
            if (dps == null || dps.Count == 0) return maxDate;

            for (int i = 0; i < dps.Count; i++)
            {
                if (dps[i] == null || !dps[i].Active) continue;
                if (!(dps[i].StandardValue is DateTime)) continue;
                if (dps[i].Data.Contains("UN")) continue;

                DateTime currentDate = Convert.ToDateTime(dps[i].StandardValue);
                if (maxDate == DateTime.MinValue || currentDate > maxDate)
                {
                    maxDate = currentDate;
                }
            }
            return maxDate;
        }

    }
}
