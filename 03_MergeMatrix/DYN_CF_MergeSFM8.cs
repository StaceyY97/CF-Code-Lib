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
public class DYN_CF_MergeSFM8 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
IDataPoint dpAct = Rave.Context.ActionDataPoint;
     if (dpAct == null) return null;

     ISubject cSUB = dpAct.Record.Subject;
     field[] checkFields = new field[]
     {
new field{OID = "PRTVR",FormOID = "DSCONS",FolderOID = "SCR"},
new field{OID = "REPROT",FormOID = "RECONS",FolderOID = "CONSENT"}
     };
     string str_matrix_OID = "SFM8";

     IMatrix targetMatrix = Rave.Architect.FetchMatrixByOid(str_matrix_OID, cSUB.CrfVersionRef);

IDataPoint dp_trigger = cSUB.Instances.FindByFolderOid("SCR")?.DataPages.FindByFormOid("VISNX")?.MasterRecord.DataPoints.FindByFieldOid("DSCONT");
     bool needMerge = dp_trigger?.Data=="Y"&& IsTriggerMeet(checkFields, cSUB);

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
             if (dp != null && dp.Active && int.TryParse(dp.Data.Replace("V", "").Trim(), out int versionNum) && versionNum >= 7) return true;                   
         }
     }
     return false;
 }

 class field
 {
     public string OID;
     public string FormOID;
     public string FolderOID;
}
}
}
