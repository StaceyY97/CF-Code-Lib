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
public class TUTL_021 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

          Programmer Name: MXZ/17Jan2023
         * Custom Function: TUTL_021
         * Edit Check(s): TUTL_021
         * Descriptions: "TLRADYN", "TLRADSZ", "TSTMD", "SP1LTD", "SP1TSTM", "SP1TSTMD", "SP2LTD", "SP2TSTM", "SP2TSTMD", "SP3LTD", "SP3TSTM", "SP3TSTMD", "COAL1"  Fields should not be visible at Screening on TUTL form.

         **************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;
            Instance InstCurrent = dpAction.Record.Instance;
            System.Collections.Generic.List<string> FOlders = new System.Collections.Generic.List<string> { "SCR" };
            string[] Fields = { "TLRADYN", "TLRADSZ", "TSTMD", "SP1LTD", "SP1TSTM", "SP1TSTMD", "SP2LTD", "SP2TSTM", "SP2TSTMD", "SP3LTD", "SP3TSTM", "SP3TSTMD", "COAL1" };
            string formOID = "TUTL";

            if (InstCurrent == null || !InstCurrent.Active || !FOlders.Contains(InstCurrent.Folder.OID)) return null;

            DataPage dpg = InstCurrent.DataPages.FindByFormOID(formOID);

            if (dpg != null && dpg.Active)
            {
                Records rcds = dpg.Records;
                for (int j = 0; j < rcds.Count; j++)
                {
                    if (!rcds[j].Active || rcds[j].RecordPosition == 0) continue;

                    for (int i = 0; i < Fields.Length; i++)
                    {
                        DataPoint dpt = rcds[j].DataPoints.FindByFieldOID(Fields[i]);

                        if (dpt != null && dpt.IsVisible == true) dpt.IsVisible = false;
                    }
                }
            }
            return null;
}
}
}
