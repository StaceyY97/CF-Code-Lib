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
public class TUTL_053 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
        Programmer Name: Qian Yang/13Jan2024
        * Custom Function: TUTL_053
        * Edit Check(s): TUTL_053
        * Descriptions: Derived in PDIAM field for all visit except screening:
        *Sum of LTD/TSTMD or SP1LTD/SP1TSTMD + SP2LTD/SP2TSTMD + SP3LTD/SP3TSTMD
        **************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dp = afp.ActionDataPoint;
        Subject subject = dp.Record.Subject;
        DataPage pag = dp.Record.DataPage;
        Instance ins = pag.Instance;

        double total = 0;
        System.Collections.Generic.List<string> SumOIDs = new System.Collections.Generic.List<string>
        {
            "SP1LTD", "SP1TSTMD", "LTD", "TSTMD", "SP2LTD", "SP2TSTMD", "SP3LTD", "SP3TSTMD"
        }
        ;
        System.Collections.Generic.List<double> SumValue = new System.Collections.Generic.List<double>();

        DataPoint dpInput = null;
        if (string.Compare(ins.Folder.OID, "SCR", true) == 0)
        {
            dpInput = GetDataPoint(subject, "", ins.Folder.OID, pag.Form.OID, "PDIAM");
            if (dpInput == null) return null;
            DataPoints temDPs = CustomFunction.FetchAllDataPointsForOIDPath("LTD", pag.Form.OID, ins.Folder.OID, subject);
            if (temDPs == null) return null;
            foreach (DataPoint temDP in temDPs)
            {
                if (temDP == null && !temDP.Active) continue;
                if (temDP.IsVisible != true && temDP.Data != string.Empty) continue;
                double value = ValueTrans(temDP.Data);
                SumValue.Add(value);
            }
        }
        else
        {
            if (ins.Folder.OID == "UNS")
            {
                dpInput = dp.Record.DataPage.MasterRecord.DataPoints.FindByFieldOID("PDIAM");
            }
            else
            {
                string parentIns = ins.ParentInstance == null ? "" : ins.ParentInstance.Folder.OID;
                dpInput = GetDataPoint(subject, parentIns, ins.Folder.OID, pag.Form.OID, "PDIAM");
            }
            if (dpInput == null) return null;
            DataPoints allRCDP = pag.GetAllDataPoints();
            if (allRCDP == null) return null;
            foreach (DataPoint tempDP_all in allRCDP)
            {
                if (tempDP_all == null || !tempDP_all.Active || tempDP_all.Record.RecordPosition == 0) continue;
                if (SumOIDs.Contains(tempDP_all.Field.OID))
                {
                    if (tempDP_all.IsVisible == true && tempDP_all.Data != string.Empty)
                    {
                        double value = ValueTrans(tempDP_all.Data);
                        SumValue.Add(value);
                    }
                }
            }
        }

        if (SumValue.Count < 0) return null;
 if (SumValue.Count==0&&dpInput!=null) {EnterData(dpInput, string.Empty); return null;}

        total = SumList(SumValue);
        if (dpInput != null)
        {
            EnterData(dpInput, Math.Round(total, 1).ToString());
        }
        return null;
    }

    void EnterData(DataPoint dpt, string Val)
    {
        if (dpt != null && dpt.LockStatus != LockStatusEnum.Locked)
        {
            dpt.Enter(Val, "", 0);
        }
    }

    double ValueTrans(string inputString)
    {
        double valueOut = 0;
        if (string.IsNullOrEmpty(inputString)) return valueOut;
        if (Number.IsValidNumber(inputString))
        {
            try
            {
                valueOut = double.Parse(inputString);
            }
            catch
            {
            }
        }
        return valueOut;
    }

    DataPoint GetDataPoint(Subject currentSubj, string parentFolderOID, string folderOID, string formOID, string feildOID)
    {
        DataPoint dt = null;
        Instance Folder;
        if (currentSubj != null && currentSubj.Active)
        {
            if (parentFolderOID != "")
            {
                Instance tempIns = currentSubj.Instances.FindByFolderOID(parentFolderOID);
                if (tempIns == null) return dt;
                Folder = tempIns.Instances.FindByFolderOID(folderOID);
                if (Folder == null) return dt;
            }
            else
            {
                Folder = currentSubj.Instances.FindByFolderOID(folderOID);
                if (Folder == null) return dt;
            }
            DataPage tempPage = Folder.DataPages.FindByFormOID(formOID);
            if (tempPage == null) return dt;
            DataPoint tempDP = tempPage.MasterRecord.DataPoints.FindByFieldOID(feildOID);
            if (tempDP != null) return tempDP;
        }
        return dt;
    }

    double SumList(System.Collections.Generic.List<double> inputList)
    {
        double sum = 0;
        if (inputList.Count <= 0) return 0;
        foreach (double input in inputList)
        {
            sum += input;
        }
        return sum;
}
}
}
