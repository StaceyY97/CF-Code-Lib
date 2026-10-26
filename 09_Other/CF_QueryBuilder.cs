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
public class CF_QueryBuilder : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
 Dictionary<int, List<object>> triggerDPs = (Dictionary<int, List<object>>)inputParameters[0];
 Dictionary<int, string> relation = (Dictionary<int, string>)inputParameters[1];
 string querytext = (string)inputParameters[2];
 DataPoint enterDP= (DataPoint)inputParameters[3];
//if (enterDP.Record.Subject.Name == "QY005" && enterDP.Field.OID == "STOOL_P1") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", "enter CF");

 Dictionary<string, string> FUNC_TYPE = new Dictionary<string, string>
 {
     {"IsEqualTo", " is not recorded as "},
     {"IsNotEqualTo", " is recorded as "},
     {"IsNotEmpty", " is empty"},
     {"IsGreaterThan", " is less than or equal to " },
     {"IsLessThan", " is greater than or equal to "},
     {"IsGreaterThanOrEqualTo", " is less than "},
     {"IsLessThanOrEqualTo", " is greater than "},
     {"Contains", " is not contains "}
 };
 Dictionary<string, string> REl_TYPE = new Dictionary<string, string>
 {
     {"And", " or "},
     { "Or", " and "}
 };
 Dictionary<string, Func<string, string, bool>> Comparers = new Dictionary<string, Func<string, string, bool>>()
 {
     { "IsEqualTo", (a, b) => a.ToUpper() == b.ToUpper() },
     { "IsNotEqualTo", (a, b) => a.ToUpper() != b.ToUpper() },
     { "IsNotEmpty", (a, _) => !string.IsNullOrEmpty(a) },
     { "IsGreaterThan", (a, b) => double.TryParse(a, out var numA) && double.TryParse(b, out var numB) && numA > numB },
     { "IsLessThan", (a, b) => double.TryParse(a, out var numA) && double.TryParse(b, out var numB) && numA < numB },
     { "IsGreaterThanOrEqualTo", (a, b) => double.TryParse(a, out var numA) && double.TryParse(b, out var numB) && numA >= numB },
     { "IsLessThanOrEqualTo", (a, b) => double.TryParse(a, out var numA) && double.TryParse(b, out var numB) && numA <= numB },
     { "Contains", (a, b) => a.ToUpper().Contains(b.ToUpper()) }

 };

 string GetUV(DataPoint targetDP, string codedValue) => GetCKBUV(targetDP, codedValue) == "" ?
 targetDP.Field.Variable.DataDictionary?.Entries.FirstOrDefault(e => e.Key.Equals(codedValue))?.Value ?? codedValue
 : GetCKBUV(targetDP, codedValue);
 string GetCKBUV(DataPoint targetDP, string codedValue) => string.Compare(targetDP.Field.ControlType, "CHECKBOX", true) == 0 ?
 codedValue == "1" ? "Checked" : codedValue == "0" ? "Unchecked" : "": "";

 Dictionary<string, List<string>> queryParameters = new Dictionary<string, List<string>>();
 if (triggerDPs == null) return querytext;
 //if (enterDP.Record.Subject.Name == "QY005" && enterDP.Field.OID == "STOOL_P1") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", "enter CF_2");


 foreach (var kvp in triggerDPs)
 {
     if (kvp.Value?.Count > 2 && kvp.Value[0] is DataPoint)
     {
         DataPoint triggerDP = (DataPoint)kvp.Value[0];
         string compareValue = kvp.Value[1]?.ToString() ?? "";
         int index = Convert.ToInt32(kvp.Key);
         if (triggerDP != null)
         {
             string fieldText = triggerDP.Field?.PreText ?? "field",
             CNT_Main = FUNC_TYPE.ContainsKey((string)kvp.Value[2]) ? FUNC_TYPE[(string)kvp.Value[2]] : "",
             connection = relation.ContainsKey(index) && REl_TYPE.ContainsKey(relation[index]) ? ", " + REl_TYPE[relation[index]] : "",
             compare = GetUV(triggerDP, compareValue);

             if (queryParameters.ContainsKey($"\"{fieldText}\"")) queryParameters[$"\"{fieldText}\""].Add($"{CNT_Main}{compare}{connection}");
             else queryParameters.Add($"\"{fieldText}\"", new List<string> { $"{CNT_Main}{compare}{connection}" });

             //querytext = string.IsNullOrEmpty(querytext) ? $"\"{fieldText}\"{CNT_Main}{compareValue}" : $"{querytext}, {connection}\"{fieldText}\"{CNT_Main}{compareValue}";
         }
     }
 }
 //if (enterDP.Record.Subject.Name == "QY005" && enterDP.Field.OID == "STOOL_P1") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", "enter CF_3");

 foreach (string key in queryParameters.Keys)
 {
     string tempQR = key;
     foreach (string PARAMETER in queryParameters[key])
     {
         tempQR += PARAMETER;
     }
     querytext += tempQR;
 }
// if (enterDP.Record.Subject.Name == "QY005" && enterDP.Field.OID == "STOOL_P1") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", "enter CF_4");

 return querytext;
}
}
}
