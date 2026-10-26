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
public class EOT_006 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
       
         Programmer Name: MXZ/26Jan2024
        * Custom Function: EOT_006
        * Edit Check(s): EOT_007, EOT_007_1
        * Descriptions:TTREAS= Adverse Event & AEID is provided. Same AE must have "Drug Withdrawn" as  'Action Taken with CX-904'.
        **************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint Dpt_AEID = afp.ActionDataPoint;

            Subject current_subject = Dpt_AEID.Record.Subject;
            // Declaration Start
            string querytext = "Primary reason for treatment discontinuation is 'Adverse Event'; however, 'Action Taken with CX-2051' is not selected as 'Drug Withdrawn' for the same AE. Please verify & update.";
            bool queryvalue = false;
            string FieldOID_INDC = "TTREAS";
            string FieldOID_AEMED = "AEACN4", FormOID = "AE", FolderOID = "AE";
            string Action = "1", AE = "3", data1 = string.Empty, recordposition = string.Empty;
           

            if (Dpt_AEID.Data != string.Empty)
            {
                DataPoint dpt_INDC = Dpt_AEID.Record.DataPoints.FindByFieldOID(FieldOID_INDC);

                if (dpt_INDC != null && dpt_INDC.Data == AE)
                {

                    if (Dpt_AEID.Data.Contains("_"))
                    {
                        string[] sss = Dpt_AEID.Data.Split('_');
                        recordposition = sss[0].Trim();
                        if (recordposition != string.Empty)
                        {
                            queryvalue = IsPresent(Convert.ToInt32(recordposition), Action, FieldOID_AEMED, FormOID, FolderOID, current_subject);

                        }
                    }
                }
            }
                    CustomFunction.PerformQueryAction(querytext, 1, false, false, Dpt_AEID, queryvalue, afp.CheckID, afp.CheckHash);
               
            return null;
        }
        bool IsPresent(int pos, string val, string FieldOID_AEMED, string FormOID, string FolderOID, Subject curr_subject)
        {
            Instance inst = curr_subject.Instances.FindByFolderOID(FolderOID);
            if (inst != null && inst.Active)
            {
                DataPage dpg = inst.DataPages.FindByFormOID(FormOID);
                if (dpg != null && dpg.Active)
                {
                    Record rcd = dpg.Records[pos];
                    if (rcd != null && rcd.Active)
                    {
                        DataPoint dpt = rcd.DataPoints.FindByFieldOID(FieldOID_AEMED);
                        if (dpt != null && dpt.Data == val) return false;
                    }
                }
            }

            return true;
}
}
}
