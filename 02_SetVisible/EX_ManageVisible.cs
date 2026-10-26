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
public class EX_ManageVisible : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
* Created By :Qian Yang/06Nov2024
* Custom Function :EX_ManageVisible
* change visible status of field in multi visits
* Edit Checks :All related ECs    test     
*********************************************************************************************************/
           ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
           DataPoint dpAction = afp.ActionDataPoint;
           Subject subject = dpAction.Record.Subject;
           bool doVisible = false;

           DataPoint version = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "SCR", "DSCONS", "PRTVR" });
           if (version != null && version.Active)
           {
               doVisible = version.Data == "V2";

               GetAllManageDPs(subject);

               if (allManageDPs.Count > 0)
               {                   
                   foreach (DataPoint dp in allManageDPs)
                   {
                       if (dp == null) continue;                      
                       dp.IsVisible = doVisible;
                   }
               }
           }
           return null;
       }
       DataPoints allManageDPs = new DataPoints();
       string form_OID = "EX";
       string[] folder_OID_Arr = { "C1D1", "C2D1", "C3D1", "C4D1", "C5D1", "C6D1", "C7D1", "C8D1", "C9D1", "C10D1", "C11D1", "C12D1", "C13D1", "C14D1", "C15D1", "C16D1", "C17D1", "C18D1", "C19D1", "C20D1", "C21D1", "C22D1", "C23D1", "C24D1", "C25D1", "C26D1", "C27D1", "C28D1", "C29D1", "C30D1", "C31D1", "C32D1", "C33D1", "C34D1", "C35D1" };


       void GetAllManageDPs(Subject subject)
       {
           for (int i = 0; i < folder_OID_Arr.Length; i++)
           {
               string folder_OID = folder_OID_Arr[i];
               DataPoint tempDPs_1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", subject.CRFVersion.ID, new object[] { subject, "TRT", folder_OID, form_OID, "EXCDOSAD" });
               DataPoint tempDPs_2 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", subject.CRFVersion.ID, new object[] { subject, "TRT", folder_OID, form_OID, "EXCDOSU" });
               allManageDPs.Add(tempDPs_1);
               allManageDPs.Add(tempDPs_2);
           }
}
}
}
