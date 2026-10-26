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
public class AddForm_PK3_TIMEPOINT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
* CF Name: AddForm_PK3_TIMEPOINT
* Programmer Name:Qian Yang 17Jun2025
* Last Modified: NA
* Reason Modified: NA
* Description: Add form and set default timepoints.
* **/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        Instance curIns = dpAction.Record.DataPage.Instance;
        string formOid = "PK3";
        string fieldOid_TimePoint = "PK3TPT";
        bool doAdd = false;

        List<string> lsFOLDEROID = new List<string>() { "C01D01", "C01D15" };

        if (!lsFOLDEROID.Contains(curIns.Folder.OID) && curIns.Folder.OID != "SCRN")return null;

        try
        {
            if (dpAction.Field.OID == "STPART")
            {
                foreach (string i in lsFOLDEROID)
                {
                    DataPoint dptsVISAT2 = GetSimpleDP("VISDAT", "VISIT", i);
                    if (dptsVISAT2 == null || !dptsVISAT2.Active)continue;
                    doAdd = (dptsVISAT2.StandardValue() is DateTime) && (dpAction.Data == "3" || dpAction.Data == "4");
                    AddForms(dptsVISAT2.Record.Instance, formOid, doAdd, subject);
                    if (doAdd) manageRecord(fieldOid_TimePoint);
                }
            }
            else if (lsFOLDEROID.Contains(curIns?.Folder.OID))
            {
                DataPoint dptsSTPART = GetSimpleDP("STPART", "DSENR", "SCRN");
                doAdd = (dpAction != null && dpAction.Active && dpAction.StandardValue() is DateTime) && (dptsSTPART != null && dptsSTPART.Active && (dptsSTPART.Data == "3" || dptsSTPART.Data == "4"));
                AddForms(curIns, formOid, doAdd, subject);
                if (doAdd) manageRecord(fieldOid_TimePoint);
            }
        }
        catch { }

        DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);

        return null;

    }
    DataPage newDpage = null;
    string[] PK3TPT = { "PREDOSE", "4HR POSTDOSE", "8HR POSTDOSE" };


    void EnterData(DataPoint dp, string val, bool doFreeze)
    {
        if (dp != null && dp.Active && string.Compare(dp.Data.Trim(), val, true) != 0)
        {
            dp.Unlock();
            dp.UnFreeze();
            if (dp.LockDisplayStatus == LockDisplayStatusEnum.Unlocked)
            {
                dp.Enter(val, string.Empty, 0);
                if (doFreeze) dp.Freeze();
            }
        }
    }

    void AddForms(Instance ins, string formOID, bool doAdd, Subject subject)
    {
        newDpage = ins.DataPages.FindByFormOID(formOID);
        if (ins != null && ins.Active)
        {
            if (doAdd)
            {
                if (newDpage == null)
                {
                    Form newForm = Form.FetchByOID(formOID, ins.Subject.CRFVersion.ID);
                    newDpage = new DataPage(ins, newForm, ins.SubjectMatrixID);
                    ins.DataPages.Add(newDpage);
                }
                else
                {
                    newDpage.Active = true;
                }
            }
            else if (newDpage != null)
            {
                newDpage.Active = false;
            }
        }
    }

    void manageRecord(string fieldOid_TimePoint)
    {
        if (newDpage != null)
        {
            Records allPGRC = newDpage?.Records;
            if (allPGRC.Count - 1 < PK3TPT.Count())
            {
                for (int j = (PK3TPT.Count() - (allPGRC.Count - 1)); j > 0; j--) newDpage.AddLogRecord();
            }
            for (int indexOfTPT = 0; indexOfTPT < PK3TPT.Count(); indexOfTPT++)
            {
                DataPoint dp = newDpage.Records[indexOfTPT + 1]?.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                EnterData(dp, PK3TPT[indexOfTPT], true);
            }
        }
}
}
}
