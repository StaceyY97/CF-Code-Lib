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
public class PK_018 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
       
         Programmer Name: MXZ/18Jan2024
        * Custom Function: PK_018
        * Edit Check(s): PK_010
        * Descriptions:Same Timepoint should not be used twice in the same visit. When
<PK.PKTP= 'Pre-infusion', 'EOI+15min', 'EOI+1hr', 'EOI+6hr'>
        
        **************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;
            Instance InstCurrent = dpAction.Record.Instance;
            bool fireQuery = false;
            string querytext = "Same Timepoint has been recorded. Please verify & update.";
            string FieldOIDTPT = "PKTP";
            System.Collections.Generic.List<string> FOlders = new System.Collections.Generic.List<string> { "C1D1", "C3D1" };
            System.Collections.Generic.List<string> Timepoints = new System.Collections.Generic.List<string> { "PRE-INFUSION", "EOI+15MIN", "EOI+1HR", "EOI+6HR" };
            if (InstCurrent == null || !InstCurrent.Active || !FOlders.Contains(InstCurrent.Folder.OID)) return null;
            Records rcds = dpAction.Record.DataPage.Records;

            for (int i = 0; i < rcds.Count; i++)
            {
                if (!rcds[i].Active || rcds[i].RecordPosition == 0) continue;
                fireQuery = false;
                DataPoint dptcur = rcds[i].DataPoints.FindByFieldOID(FieldOIDTPT);
                if (dptcur != null)
                {
                    if (dptcur.Data != string.Empty && Timepoints.Contains(dptcur.Data))
                    {
                        for (int j = 0; j < rcds.Count; j++)
                        {
                            if (!rcds[j].Active || rcds[j].RecordPosition == 0 || rcds[j].ID == rcds[i].ID) continue;

                            DataPoint dptpre = rcds[j].DataPoints.FindByFieldOID(FieldOIDTPT);
                            if (dptpre != null && dptpre.Data != string.Empty && dptpre.Data == dptcur.Data)
                            {
                                fireQuery = true;
                                break;
                            }
                        }
                    }
                    CustomFunction.PerformQueryAction(querytext, 1, false, false, dptcur, fireQuery, afp.CheckID, afp.CheckHash);
                }
            }

            return null;
}
}
}
