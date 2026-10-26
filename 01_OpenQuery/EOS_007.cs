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
public class EOS_007 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
       
         Programmer Name: MXZ/26Jaan2024
        * Custom Function: EOS_007
        * Edit Check(s): EOS_005, EOS_005_1
        * Descriptions:If DSDECOD='Withdrawal by Subject' then CWD form must be completed & one of the CWD logline should have Type of Consent= 'Withdrawal from study' or 'Withdrawal from follow-up'  
        **************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint Dpt_DSDECOD = afp.ActionDataPoint;

            Subject current_subject = Dpt_DSDECOD.Record.Subject;
            // Declaration Start
            string querytext = "Primary reason for discontinuation was reported as 'Withdrawal by Subject'; however, Consent Withdrawal form is missing or 'Type of Consent' is not \"Withdrawal from study or Withdrawal from follow - up\". Please verify & update.";
            bool queryvalue = false;
            string FieldOID_CWCONS = "CWCONS", FormOID = "CWD";
            string REAS = "WITHDRAWAL BY SUBJECT";
            ArrayList values = new ArrayList() { "WITHDRAWAL FROM TREATMENT", "WITHDRAWAL FROM STUDY" };

            if (Dpt_DSDECOD.Data != string.Empty && Dpt_DSDECOD.Data == REAS)
            {
                queryvalue = IsPresent(values, FieldOID_CWCONS, FormOID, current_subject);
            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, Dpt_DSDECOD, queryvalue, afp.CheckID, afp.CheckHash);

            return null;
        }
        bool IsPresent(ArrayList val, string FieldOID1, string FormOID, Subject curr_subject)
        {
            DataPoints dpts = CustomFunction.FetchAllDataPointsForOIDPath(FieldOID1, FormOID, null, curr_subject);
            for (int i = 0; i < dpts.Count; i++)
            {
                if (!dpts[i].Active) continue;

                if (dpts[i].Data != string.Empty && val.Contains(dpts[i].Data))
                {
                    return false;
                }
            }
            return true;
}
}
}
