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
public class DLT_009 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        Programmer Name: MXZ/26Jan2024
        * Custom Function: DLT_009
        * Edit Check(s): DLT_009, DLT_009_1
        * Descriptions:Dose Limiting Toxicity? (AEDLT) equals Yes for the AE provided in DLTAEID ; however, none of the DLT Criteria equals Yes on DLT form.
        When <DLT.DLT1, DLT.DLT2, DLT.DLT3, DLT.DLT4, DLT.DLT5, DLT.DLT6 equals No>
        **************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint Dpt_Action = afp.ActionDataPoint;

        Subject current_subject = Dpt_Action.Record.Subject;
        // Declaration Start
        string querytext = "Dose Limiting Toxicity? equals Yes for the same AE; however, none of the DLT Criteria equals Yes. Please verify & update.";
        bool queryvalue = false;
        string FolderOID_CM = "AE";
        string FormOID_ST = "DLT";

        string FieldOID_STAENO = "DLTAEID";
        string FieldOID_AEMED = "AEDLT", FormOID = "AE", FolderOID = "AE";
        string Yes = "Y", No = "N", data1 = string.Empty, recordposition = string.Empty;
        DataPoints dpsDLTAEID = new DataPoints();

        if (Dpt_Action.Field.OID == FieldOID_STAENO) dpsDLTAEID.Add(Dpt_Action);

        dpsDLTAEID = CustomFunction.FetchAllDataPointsForOIDPath(FieldOID_STAENO, FormOID_ST, FolderOID_CM, current_subject);

        string[] fields =
        {
            "DLT1", "DLT2", "DLT3", "DLT4", "DLT5", "DLT6"
        }
        ;

        for (int i = 0; i < dpsDLTAEID.Count; i++)
        {
            queryvalue = false;
            data1 = string.Empty;
            recordposition = string.Empty;

            if (dpsDLTAEID[i].Active && dpsDLTAEID[i].Record.RecordPosition != 0)
            {

                if (dpsDLTAEID[i].Data != string.Empty)
                {

                    if (dpsDLTAEID[i].Data.Contains("_"))
                    {
                        string[] sss = dpsDLTAEID[i].Data.Split('_');
                        recordposition = sss[0].Trim();
                        if (recordposition != string.Empty)
                        {
                            queryvalue = IsPresent1(dpsDLTAEID[i].Record, fields, No) && IsPresent(Convert.ToInt32(recordposition), Yes, FieldOID_AEMED, FormOID, FolderOID, current_subject);

                        }
                    }
                }
                CustomFunction.PerformQueryAction(querytext, 1, false, false, dpsDLTAEID[i], queryvalue, afp.CheckID, afp.CheckHash);
            }

        }
        return null;
    }

    bool IsPresent1(Record rcd, string[] FieldOIDs, string No)
    {
        int counts = 0;
        for (int i = 0; i < FieldOIDs.Length; i++)
        {
            DataPoint dpt = rcd.DataPoints.FindByFieldOID(FieldOIDs[i]);
            if (dpt != null && dpt.Data == No) counts++;
        }

        return (counts== FieldOIDs.Length);
    }
    bool IsPresent(int pos, string Yes, string FieldOID_AEMED, string FormOID, string FolderOID, Subject curr_subject)
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
                    if (dpt != null && dpt.Data == Yes) return true;
                }
            }
        }
        return false;
}
}
}
