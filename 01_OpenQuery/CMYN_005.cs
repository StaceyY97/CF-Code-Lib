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
public class CMYN_005 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        Programmer Name:Lixia shuai/25Jan2024
        * Custom Function: CMYN_005
        * Edit Check(s): CMYN_005, CMYN_005_A
        * Descriptions:If CMYN= No then 'Prior and Concomitant Medications' form must be blank. 


        **************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpEGPERF = afp.ActionDataPoint;
            Subject subject = dpEGPERF.Record.Subject;
            Instance InstCurrent = dpEGPERF.Record.Instance;
            bool fireQuery = false;
            string querytext = "Answer is No; however, 'Prior and Concomitant Medications' form is entered. Please verify & update.";

            string[] fields =
            {
            "CMTRT", "CMINDC", "CMINDCSP", "CMAENO1", "CMAENO2", "CMAENO3", "CMAENO4", "CMAENO5", "CMSTDAT", "CMENDAT", "CMONGO", "CMDOSE", "CMDOSU", "CMDOSUSP", "CMROUTE", "CMRTSP", "CMDOSFRQ", "CMDFRQSP"
        }
            ;

            Records rcds = GetRecords(dpEGPERF, "CM");

            fireQuery = dpEGPERF.Data == "N" && IsPresent(fields, rcds);


            CustomFunction.PerformQueryAction(querytext, 1, false, false, dpEGPERF, fireQuery, afp.CheckID, afp.CheckHash);

            return null;
        }

        Records GetRecords(DataPoint dpEGPERF, string formOID)
        {
            Records rcds = new Records();
            Instance inst = dpEGPERF.Record.Instance;
            if (inst != null && inst.Active)
            {
                DataPage dpg = inst.DataPages.FindByFormOID(formOID);
                if (dpg != null && dpg.Active)
                {
                    rcds = dpg.Records;
                }

            }

            return rcds;
        }
        bool IsPresent(string[] fields, Records rcds)
        {
            if (rcds.Count > 0)
            {
                for (int j = 0; j < rcds.Count; j++)
                {
                    if (!rcds[j].Active) continue;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        DataPoint dpt = rcds[j].DataPoints.FindByFieldOID(fields[i]);

                        if (dpt == null || !dpt.Active || (dpt.Field.IsLog && dpt.Record.RecordPosition == 0) || (!dpt.Field.IsLog && dpt.Record.RecordPosition != 0)) continue;

                        if (dpt.Field.ControlType.ToUpper() == "CHECKBOX")
                        {
                            if (dpt.Data == "1") return true;
                        }

                        else
                        {
                            if (dpt.Data != string.Empty) return true;
                        }
                    }
                }
            }
            return false;
}
}
}
