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
            * Merge Matrix when atleast one data point meet its condition
           **************************************************************************/

            //Variable Declaration Start -------

            field[] checkFields = new field[]
            {
                new field{OID = "PRTVR",FormOID = "DSCONS",FolderOID = "SCR",triggerValue = "Y"},
                new field{OID = "REPROT",FormOID = "RECONS",FolderOID = "CONSENT",triggerValue = "Y"}
            };
            string str_matrix_OID = "SFM8";

            //Variable Declaration End -------

            IDataPoint dpAct = Rave.Context.ActionDataPoint;
            if (dpAct == null) return null;

            ISubject cSUB = dpAct.Record.Subject;
            IMatrix targetMatrix = Rave.Architect.FetchMatrixByOid(str_matrix_OID, cSUB.CrfVersionRef);
            if(targetMatrix == null) return null;

            bool needMerge = IsTriggerMeet(checkFields, cSUB);

            if (needMerge) cSUB.MergeMatrix(targetMatrix);
            else cSUB.UnMergeMatrix(targetMatrix);
            return null;
        }
        bool IsTriggerMeet(field[] checkFields, ISubject cSUB)
        {
            foreach (field f in checkFields)
            {
                IReadOnlyList<IDataPoint> checkDPs = cSUB.GetDataPointsByOidPath(f.OID, f.FormOID, f.FolderOID == "" ? null : f.FolderOID);
                foreach (IDataPoint dp in checkDPs)
                {
                    if (dp != null && dp.Active && dp.Data==f.triggerValue) return true;
                }
            }
            return false;
        }

        class field
        {
            public string OID;
            public string FormOID;
            public string FolderOID;
            public string triggerValue;
        }
    }
}
