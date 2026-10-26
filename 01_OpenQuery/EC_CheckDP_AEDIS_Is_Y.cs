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
public class EC_CheckDP_AEDIS_Is_Y : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const bool closeOnChange = false;
 const bool answerOnChange = false;
 const int markingGroup = 1;
 string query = "Primary Reason for Discontinuation of Study is equal to Adverse Event, but none of the answer \"Did the event cause the subject to be discontinued from the study?\" equals Yes in Adverse Events (RCP) and Adverse Events (OLE) forms. Please verify and update, else clarify.";

 ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
 DataPoint dpAction = afp.ActionDataPoint;
 bool openQuery;

 DataPoint EOSREAS = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", "EOS", "EOS", "EOSREAS" });
 if (EOSREAS != null && EOSREAS.Active)
 {
     if (EOSREAS.Data == "AE")
     {
         DataPoints AEDIS = CustomFunction.FetchAllDataPointsForOIDPath("AEDIS", "AE", "LOGS", dpAction.Record.Subject);
         DataPoints AE2DIS = CustomFunction.FetchAllDataPointsForOIDPath("AE2DIS", "AE2", "LOGS", dpAction.Record.Subject);
         foreach(DataPoint tempDP in AE2DIS)
         {
             if (tempDP != null && tempDP.Active) AEDIS.Add(tempDP);
         }
         if (AEDIS.Count > 0)
         {
             openQuery = true;
             foreach (DataPoint dp in AEDIS)
             {
                 if (dp == null || !dp.Active) continue;
                 if (dp.Data == "Y")
                 {
                     openQuery = false;
                     break;
                 }
             }
         }
         else openQuery = true;
     }
     else openQuery = false;
     CustomFunction.PerformQueryAction(query, markingGroup, false, false, EOSREAS, openQuery, afp.CheckID, afp.CheckHash);
 }
 return null;
}
}
}
