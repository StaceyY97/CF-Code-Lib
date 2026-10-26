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
public class CF_Email_Recipients : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
 string env = (string)inputParameters[0];
 string type = (string)inputParameters[1];

 string recip_part = "0808-12-EDCalert@crinetics.com;17108-EDC-Alerts@Allucent.com;crn12-EDCalerts@resolutioncrs.com;CRN12-EDCalerts@edetek.com";
 string recip_all = "pv@crinetics.com;0808-12-EDCalert@crinetics.com; 17108-EDC-Alerts@Allucent.com;crn12-EDCalerts@resolutioncrs.com;CRN12-EDCalerts@edetek.com";

 string recip_DeBug = "";

//for UAT 
string recip_UAT = "qian.yang@edetek.com;yuanyuan.liu@edetek.com;yuxin.zeng@edetek.com;CRN12-EDCalerts@edetek.com";
//string recip_UAT1 = type == "All" ? recip_all : (type == "PART" ? recip_part : "");
//recip_UAT += recip_UAT1 ;
//for DEV and debug
 string recip_DeBug_DEV = "qian.yang@edetek.com";
//for PROD
string recip_PROD = type == "All" ? recip_all : type == "PART" ? recip_part : recip_DeBug;

 string recip;

 switch (env.ToUpper())
 {
     case "DEV":
         recip = recip_DeBug_DEV;
         break;
     case "UAT":
         recip = recip_UAT;
         break;
     case "PROD":
         recip = recip_PROD; 
         break;
     default:
         recip = recip_DeBug;
         break;
 }

 return recip;
}
}
}
