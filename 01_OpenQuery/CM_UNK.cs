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
public class CM_UNK : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        Programmer Name: Rananay Singh/19Apr2022
        * Custom Function: CM_021
        * Edit Check(s):CM_021, CM_021_1
        * Descriptions:If Year is Unknown (UNKN) then the Month (UNK) & Day (UN) should be Unknown.
        **************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;

        bool fireQuery = false;
        string querytext = "If Year is Unknown, the Month & Day Should be unknown. Please review & update.";

        if (dpAction.StandardValue() is DateTime)
        {
            if (dpAction.Data.ToUpper().Contains("UNKN"))
            {
                string[] str = dpAction.Data.ToUpper().Split(' ');
                if (str.Length >= 3)
                {
                    string day = str[0];
                    string month = str[1];
                    fireQuery = (day != "UN" || month != "UNK");
                }
            }
        }
        CustomFunction.PerformQueryAction(querytext, 1, false, false, dpAction, fireQuery, afp.CheckID, afp.CheckHash);

        return null;
}
}
}
