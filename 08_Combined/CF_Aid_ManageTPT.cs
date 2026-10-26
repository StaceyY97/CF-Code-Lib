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
public class CF_Aid_ManageTPT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
Subject subject = (Subject)inputParameters[0];
Instance curIns = (Instance)inputParameters[1];
Dictionary<List<string>, string[]> folderMap = (Dictionary<List<string>, string[]>)inputParameters[2];
DataPoint PART = (DataPoint)inputParameters[3];
string formOid = (string)inputParameters[4];
string fieldOid_TimePoint = (string)inputParameters[5];

string CodeLog = "CF entered_" + folderMap.Keys.Count.ToString() + "\n";

DataPage newDpage = null;
DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) => subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
bool isValuedDate(DataPoint checkDT) => checkDT != null && checkDT.Active && checkDT.StandardValue() is DateTime && checkDT.Data != string.Empty;

DoAdd();

void DoAdd()
{
    DataPoint VISDATE = GetSimpleDP("VISDAT", "VISIT", curIns.Folder.OID);

    bool VISValid = isValuedDate(VISDATE);

    foreach (List<string> key in folderMap.Keys)
    {
        bool doAdd = false;
        string[] RNATPT = new string[] { };

        CodeLog += "Checking key: " + string.Join(",", key) + "\n";
        doAdd = key.Contains(PART?.Data)&& VISValid;
        if (doAdd) RNATPT = folderMap[key];
        CodeLog += " doAdd=" + doAdd.ToString() + "TPT=" + string.Join(",", RNATPT) + "\n"; 
        AddForms(curIns, formOid, doAdd);
        if (RNATPT.Count() > 0)
        {
            AddlogLines(newDpage, RNATPT);
            newDpage = null;
        }
        if (doAdd) break;
    }
}
void AddlogLines(DataPage dpg, string[] timePoints)
{
    if (dpg != null && dpg.Active)
    {
        for (int i = 0; i < timePoints.Length; i++)
        {
            Record rd = null;
            rd = dpg.Records.FindByRecordPosition(i + 1);

            if (rd == null)
                rd = dpg.AddLogRecord();

            if (rd != null)
            {
                DataPoint dpTimePoint = rd.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                if (fieldOid_TimePoint == "EGTPT") dpTimePoint = rd.DataPage.MasterRecord.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                EnterData(dpTimePoint, timePoints[i], true);
            }
        }
        if (dpg.Records.Count - 1 > timePoints.Length)
        {
            for (int i = timePoints.Length + 1; i <= dpg.Records.Count - 1; i++)
            {
                Record rd = null;
                rd = dpg.Records.FindByRecordPosition(i);
                DataPoint dpTimePoint = rd.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                if (fieldOid_TimePoint == "EGTPT") dpTimePoint = rd.DataPage.MasterRecord.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                EnterData(dpTimePoint, "", false);
            }
        }
    }
}
void EnterData(DataPoint dp, string val, bool doFreeze)
{
    if (dp != null && dp.Active && string.Compare(dp.Data.Trim(), val, true) != 0&& val!=string.Empty)
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
void AddForms(Instance ins, string formOID, bool doAdd)
{
    newDpage = ins.DataPages.FindByFormOID(formOID);
    if (ins != null && ins.Active)
    {
        if (doAdd)
        {
            if (newDpage == null)
            {
                Form newForm = Form.FetchByOID(formOID, subject.CRFVersion.ID);
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
return CodeLog;
}
}
}
