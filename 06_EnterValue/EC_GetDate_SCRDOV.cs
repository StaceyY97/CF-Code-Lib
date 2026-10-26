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
public class EC_GetDate_SCRDOV : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
* Created By :Qian Yang/23Dec2024
* Custom Function :EC_GetDate_SCRDOV
* Description: Enter data to Paper Prompt Diary
* Edit Checks :Set_Visiable_SCRDOV, Set_Visiable_RSCRDOV,Derive_VISDAT
*********************************************************************************************************/
     ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
     DataPoint dpAction = afp.ActionDataPoint;
     Subject subject = dpAction.Record.Subject;
     Instance currentFolder = dpAction.Record.Instance;
     string folderOID = currentFolder.Folder.OID;

     if (folderOID != "SCREEN_1" && folderOID != "RESCREEN_1" && folderOID != "D1") return null;

     if (dpAction.Field.OID == "VISDAT")
     {
         string matchField = folderOID.Length > 8 ? "RSCRDOV" : "SCRDOV";
         DataPoint dp_enter = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", folderOID, "PDIARY", matchField });
         if(dp_enter!=null &&dp_enter.Active) {
    if(dp_enter.IsVisible) EnterData(dp_enter, dpAction.Data);
    else EnterData(dp_enter,"");
}
     }
     else
     {
         if (dpAction.Field.OID == "IEYN")
         {
             DataPoint S1VISDAT = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "SCREEN_1", "VIS", "VISDAT" });
             DataPoint RS1VISDAT = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "RESCREEN_1", "VIS", "VISDAT" });
             DataPoint SCRDOV = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "D1", "ENR", "SCRDOV" });
             DataPoint RSCRDOV = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "D1", "ENR", "RSCRDOV" });
             if (S1VISDAT != null && S1VISDAT.Active) EnterData(SCRDOV, S1VISDAT.Data);
             if (RS1VISDAT != null && RS1VISDAT.Active) EnterData(RSCRDOV, RS1VISDAT.Data);
         }
         else
         {
             string matchFolder = dpAction.Field.OID.StartsWith("R") ? "RESCREEN_1" : "SCREEN_1";
             DataPoint VISDAT = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", matchFolder, "VIS", "VISDAT" });
            EnterData(dpAction, VISDAT.Data);
         }
     }

     return null;
 }

 public void EnterData(DataPoint dp, string val)
 {
     if (dp != null && dp.Active && string.Compare(dp.Data, val, true) != 0)
     {
         dp.UnFreeze();
         dp.Enter(val, string.Empty, 0);
         dp.Freeze();
     }
}
}
}
