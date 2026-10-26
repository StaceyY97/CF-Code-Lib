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
            * Description:Standard DSL
            **************************************************************************/

            //Variable Declaration Start -------
            string oid_folder_tartget = "CMF";
            string oid_form_tartget = "AE";
            string oid_term = "AETERM", oid_dat = "AESTDAT";
            //Variable Declaration End -------

            IDynamicSearchParams DSP = (DynamicSearchParams)ThisObject;
            ISubject subject = DSP.DataPoint.Record.Subject;

            KeyValueCollection KVC = new KeyValueCollection();
            try
            {
                IReadOnlyList<IDataPoint> dps_terms = subject.GetDataPointsByOidPath(oid_term, oid_form_tartget, oid_folder_tartget);
                if (dps_terms != null && dps_terms.Count > 0)
                {
                    for (int i = 0; i < dps_terms.Count; i++)
                    {
                        if (dps_terms[i].Record.RecordPosition == 0 || !dps_terms[i].Active) continue;

                        IDataPoint dp_term = dps_terms[i];
                        IDataPoint dp_dat = dp_term.Record.AllDataPoints.FindByFieldOid(oid_dat);

                        if (dp_term.ChangeCount > 0 && dp_term != null && dp_dat != null)
                        {
                            string str_rcPos = dp_term.Record.RecordPosition.ToString();
                            string str_term = dp_term.Data.Trim().Length >= 60 ? dp_term.Data.Trim().Substring(0, 60) : dp_term.Data.Trim();
                            string str_dat = dp_dat.UserValue != null ? dp_dat.UserValue.ToString() : "";
                            //string str_dat = dp_dat.StandardValue is DateTime ? dp_dat.StandardValue.ToString() : "";

                            string ds = $"{str_rcPos}_{str_term}_{str_dat}";
                            if (string.Compare(ds.Trim(), string.Empty) != 0)
                            {
                                KVC.Add(new KeyValue(ds, ds));
                            }
                        }
                    }
                }

            }
            catch { }
            return KVC;
        }
    }
}
