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
public class AE_030 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
        /** HEADER INFORMATION:**************************
        * Programmer Name:Stacey Yang
        * Description: Check if corresponding record exists
        * **/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint Dpt_Action = afp.ActionDataPoint;

        Subject current_subject = Dpt_Action.Record.Subject;
        // Declaration Start
        string querytext = "Dose Limiting Toxicity? equals Yes; however, DLT form is missing. Please complete.";
        bool queryvalue = false;
        string FolderOID_CM = "AE";
        string FormOID_ST = "DLT";

        string FieldOID_STAENO = "DLTAEID";
        string FieldOID_AEMED = "AEDLT", FormOID = "AE", FolderOID = "AE";
        string Yes = "Y", AE = "4", data1 = string.Empty, recordposition = string.Empty;
        DataPoints dpsAEMED = new DataPoints();

        if (Dpt_Action.Field.OID == FieldOID_AEMED) dpsAEMED.Add(Dpt_Action);
        dpsAEMED = CustomFunction.FetchAllDataPointsForOIDPath(FieldOID_AEMED, FormOID, FolderOID, current_subject);

        for (int i = 0; i < dpsAEMED.Count; i++)
        {
            queryvalue = false;
            data1 = string.Empty;
            recordposition = string.Empty;

            if (dpsAEMED[i].Active && dpsAEMED[i].Record.RecordPosition != 0)
            {
                if (dpsAEMED[i].Data == Yes)
                {
                    queryvalue = IsPresent(dpsAEMED[i].Record.RecordPosition, FieldOID_STAENO, FormOID_ST, FolderOID_CM, current_subject);
                }
                CustomFunction.PerformQueryAction(querytext, 1, false, false, dpsAEMED[i], queryvalue, afp.CheckID, afp.CheckHash);
            }
        }
        return null;
    }
    bool IsPresent(int pos, string FieldOID_AEMED, string FormOID, string FolderOID, Subject curr_subject)
    {
        DataPoints dptsDLT = CustomFunction.FetchAllDataPointsForOIDPath(FieldOID_AEMED, FormOID, FolderOID, curr_subject);
        string recordposition = string.Empty;
        for (int i = 0; i < dptsDLT.Count; i++)
        {
            if (!dptsDLT[i].Active || dptsDLT[i].Record.RecordPosition == 0) continue;
            recordposition = string.Empty;
            if (dptsDLT[i].Data != string.Empty)
            {
                if (dptsDLT[i].Data.Contains("_"))
                {
                    string[] sss = dptsDLT[i].Data.Split('_');
                    recordposition = sss[0].Trim();
                    if (recordposition != string.Empty)
                    {
                        if (Convert.ToInt32(recordposition) == pos) return false;
                    }
                }
            }
        }
        return true;
    }
}
}
