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
public class TUTL_007 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
try{
        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dptAction = afp.ActionDataPoint;
        Subject subject = dptAction.Record.Subject;

        string SCROID = "SCR";
        string FORMOID = "TUTL";
        string HIDDEN_FIELDOID = "TLESNO";

        DataPages current_DataPages = new DataPages();

        Instance scr_Instance = subject.Instances.FindByFolderOID(SCROID);
        if (scr_Instance == null || !scr_Instance.Active) return null;

        Records scr_records = GetSourceRecords("TUTL", "SCR", subject);

        for (int i = 0; i < subject.Instances.Count; i++)
        {
            //???folder????screening?folder
            if (subject.Instances[i] == null || !subject.Instances[i].Active || subject.Instances[i].ID == scr_Instance.ID)
                continue;
            //?????screening folder ???page(?folder+?folder)????
            DataPage tempPage = subject.Instances[i]?.DataPages?.FindByFormOID(FORMOID);
            if (tempPage != null && tempPage.Active) current_DataPages.Add(tempPage);
            //???screening?instances????folder
            Instances subins = subject.Instances[i].Instances;
            if (subins.Count > 0)
            {
                for (int j = 0; j < subins.Count; j++)
                {
                    DataPage tempPage2 = subins[j]?.DataPages?.FindByFormOID(FORMOID);
                    if(tempPage2!=null&& tempPage2.Active) current_DataPages.Add(tempPage2);
                }
            }
        }
        if (current_DataPages.Count > 0)
        {
            for (int j = 0; j < current_DataPages.Count; j++)
            {
                if (current_DataPages[j] == null || !current_DataPages[j].Active) continue;
                //???????TLTU?log
                try
                {
                    AddLogRecords(current_DataPages[j], HIDDEN_FIELDOID, scr_records);
                }
                catch { }
            }
        }
        }
        catch{}
        return null;
    }
    string strcc = string.Empty;
    string[] srcFlds =
    {
    "TLESNO", "TULES", "TLESSP", "TLESLOC"
}
    ;

    string[] trgFlds =
    {
    "TLESNO", "TULES", "TLESSP", "TLESLOC"
}
    ;
    Records GetSourceRecords(string sFrm, string sFol, Subject s)
    {
        Records rdsSrc = new Records();
        Instance ins = s.Instances.FindByFolderOID(sFol);
        if (ins == null) return rdsSrc;
        DataPage dPg = ins.DataPages.FindByFormOID(sFrm);
        if (dPg == null) return rdsSrc;
        Records rds = dPg.Records;
        for (int i = 0; i < rds.Count; i++)
        {
            if (rds[i] == null || rds[i].RecordPosition == 0) continue;
            rdsSrc.Add(rds[i]);
        }
        return rdsSrc;
    }
    void AddLogRecords(DataPage tPage, string fldH, Records srcRds)
    {
        Records targetRds = tPage.Records;
        if (srcRds.Count > 0)//sceening ?tummer assessment??????
        {
            //????scr log???
            for (int index = 0; index < srcRds.Count; index++)
            {
                Record tRec = null;
                //??scr recordPosition
                string srcH = srcRds[index].RecordPosition.ToString();

                //???page???
                for (int i = 0; i < targetRds.Count; i++)
                {
                    if (targetRds[i].RecordPosition == 0) continue;//???log ?record
                    //??hidden,?????log
                    DataPoint dpTARR = targetRds[i].DataPoints.FindByFieldOID(fldH);
                    if (dpTARR != null && string.Compare(dpTARR.Record.RecordPosition.ToString(), srcH, true) == 0)
                    {
                        tRec = targetRds[i];
                        break;
                    }
                }
                if (tRec != null)
                {//??????????
                    tRec.Active = srcRds[index].Active;
                    if (tRec.Active) CopyData(srcRds[index], tRec);
                }
                else
                {
                    //????????log,????????
                    tRec = tPage.AddLogRecord();
                    if (tRec != null) CopyData(srcRds[index], tRec);
                }              
            }
        }
    }
    void CopyInitialData(Records srcRds, string fldH, DataPage tPage)
    {
        Records tRds = tPage.Records;
        Record tRd = null;

        for (int index = 0; index < srcRds.Count; index++)
        {
            if (index == 0)
                tRd = tRds[1];
            else
            {
                tRd = tPage.AddLogRecord();
                tRds = tPage.Records;
            }
            if (tRd != null)
            {
                CopyData(srcRds[index], tRd);
                tRd.Active = srcRds[index].Active;
            }
        }
    }
    void CopyData(Record sR, Record tR)
    {
        string srcData = string.Empty;
        DataPoint dp_SRC = null, dp_TAR = null;
        if (sR != null && tR != null)
        {
            for (int i = 0; i < srcFlds.Length; i++)
            {
                dp_SRC = sR.DataPoints.FindByFieldOID(srcFlds[i]);
                dp_TAR = tR.DataPoints.FindByFieldOID(trgFlds[i]);
                if (dp_SRC != null && dp_TAR != null)
                {
                    srcData = dp_SRC.Data;
                    EnterData(dp_TAR, srcData);
                }
            }
        }
    }
    void EnterData(DataPoint dp, string val)
    {
        if (dp != null && dp.LockStatus != LockStatusEnum.Locked)
        {
            dp.UnFreeze();
            dp.Enter(val, string.Empty, 0);
            dp.Freeze();
        }
}
}
}
