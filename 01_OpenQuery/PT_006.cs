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
public class PT_006 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        * Custom Function: PT_006
        * Edit Check(s): PT_006
        * Descriptions: When PTPDDAT is not empty and PTBR PTENREAS are all not pd, fire a query

        **************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dp_qfield = afp.ActionDataPoint;
        DataPage dpg_PT = dp_qfield.Record.DataPage;
        string tfield_1 = "PTBR", tfield_2 = "PTENREAS";
        string value_1 = "PD", value_2 = "DISEASE PROGRESSION";
        bool fireQuery = false;
        string querytext = "Best Response is not \"Progressive Disease\" and Specify reason agent was stopped is not \"Disease Progression\"; however, Date of relapse/progression is present. Please review & update.";
        if (dp_qfield != null)
        {
            if (dp_qfield.Data != string.Empty) //if dp_qfield is empty,then execute the Query statement, and at this moment, fireQuery is false
            {
                DataPoint dp_tfield_1 = dpg_PT.MasterRecord.DataPoints.FindByFieldOID(tfield_1);
                if (dp_tfield_1 != null && dp_tfield_1.Data == value_1) //if dp_tfield is PD,then execute the Query statement, and at this moment, fireQuery is false
                {
                    goto Query;
                }
                else //else, find tfield_2. If one of tfield_2 is PD, then execute the Query statement, and fireQuery is false, or fireQuery is true
                {
                    fireQuery = true;
                    Records rcds_tfield_2 = dpg_PT.Records;
                    if (rcds_tfield_2 != null)
                    {
                        foreach (Record rcd_tfield_2 in rcds_tfield_2)
                        {
                            DataPoint dpt_tfield_2 = rcd_tfield_2.DataPoints.FindByFieldOID(tfield_2);
                            if (dpt_tfield_2!=null && dpt_tfield_2.Data== value_2)
                            {
                                fireQuery = false;
                                goto Query;
                            }
                        }
                    }
                }
            }
            Query:
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dp_qfield, fireQuery, afp.CheckID, afp.CheckHash);
        }
        return null;
}
}
}
