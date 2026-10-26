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
public class DEATH_007 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        Programmer Name: XRZ
        * Custom Function: DEATH_007
        * Edit Check(s): DEATH_007, DEATH_007_1
        * Descriptions:"Primary Cause of death equals 'Adverse Event'; then 'Results in death' must be selected at least for one AE.
        Note: Consider only known date parts.
        **************/
        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dp = afp.ActionDataPoint;
            Subject sj = dp.Record.Subject;
            bool openQuery = false;
            string queryText = "Primary Cause of death equals 'Adverse Event'; then 'Results in death' must be selected at least for one AE. Please verify & update.";

            Instance insDeath = sj.Instances.FindByFolderOID("DTH");
            if (insDeath == null) return null;
            DataPage pageDeath = insDeath.DataPages.FindByFormOID("DEATH");
            if (pageDeath == null) return null;
            DataPoint pointDeath = pageDeath.MasterRecord.DataPoints.FindByFieldOID("DTHCAUS");
            if (pointDeath.Active && pointDeath.Data != string.Empty && pointDeath.Data == "AE")
            {
                DataPoints AESDTHs = CustomFunction.FetchAllDataPointsForOIDPath("AESDTH", "AE", null, sj);
                foreach (DataPoint AESDTH in AESDTHs)
                {
                    openQuery = true;
                    if (AESDTH.Active && AESDTH.Data == "1")
                    {
                        openQuery = false;
                        break;
                    }
                }
            }
            CustomFunction.PerformQueryAction(queryText, 1, false, false, dp, openQuery, afp.CheckID, afp.CheckHash);
            return null;
}
}
}
