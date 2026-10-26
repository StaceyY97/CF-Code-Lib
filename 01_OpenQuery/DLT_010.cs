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
public class DLT_010 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        Programmer Name: MXZ/26Jan2024
        * Custom Function: DLT_010
        * Edit Check(s): DLT_010, DLT_010_1
        * Descriptions:Dose Limiting Toxicity? Must be Yes for the same AE entered in DLTAEID. When
        <DLT.DLTAEID is not null & AE.AEDLT=No for the same AE>
        **************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint Dpt_Action = afp.ActionDataPoint;

        Subject current_subject = Dpt_Action.Record.Subject;
        // Declaration Start
        string querytext = "DLT AE ID is provided; however, 'Dose Limiting Toxicity?' is answered \"No\" for the same AE. Please verify & update.";
        bool queryvalue = false;
        string FolderOID_CM = "AE";
        string FormOID_ST = "DLT";

        string FieldOID_STAENO = "DLTAEID";
        string FieldOID_AEMED = "AEDLT", FormOID = "AE", FolderOID = "AE";
        string Yes = "Y", AE = "4", data1 = string.Empty, recordposition = string.Empty;
        DataPoints dpsSTAENO = new DataPoints();

        if (Dpt_Action.Field.OID == FieldOID_STAENO) dpsSTAENO.Add(Dpt_Action);

        dpsSTAENO = CustomFunction.FetchAllDataPointsForOIDPath(FieldOID_STAENO, FormOID_ST, FolderOID_CM, current_subject);


        for (int i = 0; i < dpsSTAENO.Count; i++)
        {
            queryvalue = false;
            data1 = string.Empty;
            recordposition = string.Empty;

            if (dpsSTAENO[i].Active && dpsSTAENO[i].Record.RecordPosition != 0)
            {

                if (dpsSTAENO[i].Data != string.Empty)
                {

                    if (dpsSTAENO[i].Data.Contains("_"))
                    {
                        string[] sss = dpsSTAENO[i].Data.Split('_');
                        recordposition = sss[0].Trim();
                        if (recordposition != string.Empty)
                        {
                            queryvalue = IsPresent(Convert.ToInt32(recordposition), Yes, FieldOID_AEMED, FormOID, FolderOID, current_subject);

                        }
                    }
                }
                CustomFunction.PerformQueryAction(querytext, 1, false, false, dpsSTAENO[i], queryvalue, afp.CheckID, afp.CheckHash);
            }

        }
        return null;
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
                    if (dpt != null && dpt.Data != string.Empty && dpt.Data != Yes) return true;
                }
            }
        }

        return false;
}
}
}
