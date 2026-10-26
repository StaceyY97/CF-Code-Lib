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
public class DER_TUBASE_TL : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: DER_TUBASE_TL
        * Programmer Name:Qian Yang(17Nov2025)
        * Last Modified: NA.
        * Reason Modified: NA.
        * Description: Populate baseline information to following assessments and do visitble to Logline fields
        **/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dp = afp.ActionDataPoint;
        Subject curSub = dp.Record.Instance.Subject;

        Instance currentIns = dp.Record.Instance;
        string folderOID = currentIns.Folder.OID;
        DataPoints triggerFields = new DataPoints();
        Dictionary<int, List<string>> Baseline = GetBaseline(curSub);

        if (baselineFolder.Contains(folderOID)) triggerFields = CustomFunction.FetchAllDataPointsForOIDPath(triggerField, manageForm, null, curSub);
        else triggerFields.Add(dp.Record.DataPage?.MasterRecord?.DataPoints?.FindByFieldOID(triggerField));
        foreach (DataPoint trigger in triggerFields)
        {
            if (trigger == null || !trigger.Active || baselineFolder.Contains(trigger.Record.Instance.Folder.OID)) continue;
            if (trigger?.Data == triggerMatch)
            {
                DataPage curenPage = trigger.Record.DataPage;
                int currentRC = curenPage.Records.Count;
                if (curenPage.Records.Count < Baseline.Keys.Count + 1)
                {
                    for (int i = Baseline.Keys.Count + 1; i > currentRC; i--)
                    {
                        curenPage.AddLogRecord();
                    }
                }
                foreach (Record rc in curenPage.Records)
                {
                    if (rc == null) continue;
                     if (Baseline.ContainsKey(rc.RecordPosition))
 {
     DoPopulate(rc, Baseline[rc.RecordPosition]);
     foreach (string oid in new string[] { "TLDIAMNE", "TLDIAMRESC", "TLDIAMCOAL", "TLDIAMSPL","TLDIAMCOALID" })
     {
         DataPoint manageDP = rc.DataPoints.FindByFieldOID(oid);
         if (manageDP != null) manageDP.IsVisible = true;
     }
 }
                }
            }
        }
        return false;
    }

    DataPoint GetSimpleDP(string field_OID, string formOID, string folder_OID, Subject curSub) =>
curSub?.Instances.FindByFolderOID(folder_OID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(field_OID);

    ///below should be defined by programmer
    string manageForm = "TL";

    string triggerField = "TLYN";

    string triggerMatch = "Y";

    string[] populateField = new string[] { "TLID", "TLSITE", "TLDESC", "TLDIAMTYP" };


    string[] baselineFolder = new string[] { "RESCREEN_2", "SCREEN_2" };

    ///above should be defined by programmer


    Dictionary<int, List<string>> GetBaseline(Subject curSub)
    {
        Dictionary<int, List<string>> source = new Dictionary<int, List<string>>();

        foreach (string baseFolder in baselineFolder)
        {
            DataPoint triggerDP = GetSimpleDP(triggerField, manageForm, baseFolder, curSub);
            if (triggerDP == null || !triggerDP.Active || triggerDP.Data != triggerMatch) continue;
            Records allSourceRC = triggerDP?.Record.DataPage.Records;
            foreach (Record record in allSourceRC)
            {
                if (record == null || !record.Active || record.RecordPosition == 0) continue;
                List<string> rcValues = new List<string>();
                foreach (string populate in populateField)
                {
                    DataPoint tempDP = record.DataPoints.FindByFieldOID(populate);
                    rcValues.Add(GetDPData(tempDP));
                }
                source.Add(record.RecordPosition, rcValues);
            }
            break;
        }
        return source;
    }

    string GetDPData(DataPoint dp)
{
    if (dp != null && dp.Active)
    {
        if (dp.Field.OID.EndsWith("SITE"))
        {
            return dp.CodedValue()?.ToString() == "OTHER,SPECIFY" ? @"OTHER,SPECIFY\\" + dp.Data : dp?.Data;
        }
        else return dp.Data;
    }
    return string.Empty;
}

void DoPopulate(Record rc, List<string> matchRC)
{
    if (rc != null && rc.Active && rc.RecordPosition != 0 && matchRC.Count == populateField.Length)
    {
        for (int i = 0; i < populateField.Length; i++)
        {
            DataPoint manageDP = rc.DataPoints.FindByFieldOID(populateField[i]);
            EnterData(manageDP, matchRC[i]);
        }
    }
}

void EnterData(DataPoint enterDP, string enterValue)
{
    if (enterDP != null)
    {
        enterDP?.UnFreeze();
        enterDP?.Enter(enterValue, string.Empty, 0);
        enterDP?.Freeze();
    }
}
}
}
