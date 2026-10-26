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
public class EC_DynamicSearchList : CustomFunctionBase
{
public object Eval(object ThisObject)
{
DataPoint dp = ((DynamicSearchParams)ThisObject).DataPoint;
 Subject subj = dp.Record.Subject;


 KeyValueCollection kvcList = new KeyValueCollection();

 string entetField = dp.Field.OID;

 Dictionary<string, List<string>> DSLKey = (Dictionary<string, List<string>>)CustomFunction.PerformCustomFunction("CF_Aid_GetDSL_Key", dp.Field.CRFVersion.ID, new object[] { entetField });

 string GetTermOID(string formOID) => formOID.StartsWith("CM") ? formOID + "TRT" : formOID + "TERM";
 string GetDateOID(string formOID) => formOID + "STDAT";


 if (DSLKey != null)
 {
     string[] doubleDSL = new string[] { "AE", "CM", "NP" };
     foreach (string key in DSLKey.Keys)
     {
         if (doubleDSL.Contains(key))
         {
             KeyValueCollection TempKVL_1 = (KeyValueCollection)CustomFunction.PerformCustomFunction("CF_Aid_GetDSL", dp.Field.CRFVersion.ID, new object[] { null, key, GetTermOID(key), GetDateOID(key), subj });
             KeyValueCollection TempKVL_2 = (KeyValueCollection)CustomFunction.PerformCustomFunction("CF_Aid_GetDSL", dp.Field.CRFVersion.ID, new object[] { null, key + "2", GetTermOID(key+"2"), GetDateOID(key + "2"), subj });
             kvcList.Join(TempKVL_1);
             kvcList.Join(TempKVL_2);
         }
         else
         {
             kvcList = (KeyValueCollection)CustomFunction.PerformCustomFunction("CF_Aid_GetDSL", dp.Field.CRFVersion.ID, new object[] { null, key, GetTermOID(key), GetDateOID(key), subj });
         }
         break;
     }
 }
 return kvcList;
}
}
}
