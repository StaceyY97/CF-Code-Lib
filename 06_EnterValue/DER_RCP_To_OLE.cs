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
public class DER_RCP_To_OLE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint enterDP = afp.ActionDataPoint;
    sj = enterDP.Record.Subject;

    deriveFields = (Dictionary<string, List<string>>)CustomFunction.PerformCustomFunction("CF_RCP_To_OLE_MapList", enterDP.Field.CRFVersion.ID, new object[] { null });

    if (enterDP?.Field.OID == "VISDAT") mapList = mapList_All;
    else mapList = new string[] { enterDP?.Record.DataPage.Form.OID };

    DataPoint VISDAT = enterDP?.Field.OID == "VISDAT" ? enterDP : GetSimpleDP("VISDAT", "VIS", "OLED1");
    foreach (string map in mapList)
    {
        Form_match = new Dictionary<string, Dictionary<string, string>>();
        Main(map, VISDAT?.StandardValue() is DateTime && VISDAT?.Data != string.Empty);
    }
    return null;
}

Subject sj;
string commenFolder_OID = "LOGS";
string[] mapList_All = new string[] { "AE", "CM", "NP" };
string[] mapList;
string triger_OID = "ONGO", matchValue = "Y", match_OID = "PID", num_OID;
string[] nonLOGFields = new string[] { "AE2YN", "NP2YN", "CM2YN", "H_NOW" };
Dictionary<string, List<string>> deriveFields = new Dictionary<string, List<string>>() { };
Dictionary<string, Dictionary<string, string>> Form_match = new Dictionary<string, Dictionary<string, string>>();


void Main(string formOID, bool doGetSource)
{
    if (doGetSource)
    {
        //get source
        DataPage source = sj.Instances.FindByFolderOID(commenFolder_OID)?.DataPages.FindByFormOID(formOID);
        if (source != null) GetSource(source, formOID);
    }

    // do enter
    string targetForm_OID = formOID + "2";
    if (deriveFields.ContainsKey(targetForm_OID))
    {
        num_OID = targetForm_OID.StartsWith("AE") ? targetForm_OID + "_" + match_OID : targetForm_OID + match_OID;
        DataPage enter = sj.Instances.FindByFolderOID(commenFolder_OID)?.DataPages.FindByFormOID(targetForm_OID);
        if (enter != null)
        {
            //add sufficient logs first
            AddLogLine(enter, targetForm_OID);

            List<string> KeyAvoid = new List<string>();
            DataPoints updateRC = new DataPoints();
            DataPoints emptyRC = new DataPoints();

            //Get records that need to be updated or is all empty
            foreach (Record rc in enter.Records)
            {
                if (!IsValidRC(rc)) continue;

                DataPoint num = rc.DataPoints.FindByFieldOID(num_OID);
                if (num?.Data == string.Empty)
                {
                    if (!AtLeastOneValue(rc))
                    {
                        emptyRC.Add(num);
                    }
                }
                else
                {
                    if (Form_match.ContainsKey(num?.Data)) updateRC.Add(num);
                    else rc.Active = false;
                }
            }

            //Do update
            foreach (DataPoint num in updateRC)
            {
                if (Form_match.ContainsKey(num.Data)) { DoEnter(num); KeyAvoid.Add(num.Data); }
            }

            //enter to empty records if any
                int rcIndex = 0;
            foreach (string key in Form_match.Keys)
            {
                if (!KeyAvoid.Contains(key))
                {
                    if (rcIndex <= emptyRC.Count - 1)
                    {
                        DoEnter(emptyRC[rcIndex], key);
                        rcIndex++;
                    }
                }
            }
        }
    }
}

bool IsValidRC(Record r) => r != null && r.RecordPosition != 0;
bool isValidDP(DataPoint checkDP) => checkDP != null && checkDP.Active;
DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
sj.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
string ChangeOID(string OID) => OID.Replace(OID.Substring(0, 2), OID.Substring(0, 2) + "2");

void GetSource(DataPage target, string formOID)
{
    //get source data
    Records allPage_RC = target.Records;
    foreach (Record r in allPage_RC)
    {
        if (IsValidRC(r))
        {
            DataPoint ongo = r.DataPoints.FindByFieldOID(formOID + triger_OID);
            if (isValidDP(ongo) && ongo.Data == matchValue)
            {
                string recordPosition = r.RecordPosition.ToString();
                Form_match.Add(recordPosition, new Dictionary<string, string> { });

                foreach (DataPoint dpToMap in r.DataPoints)
                {
                    if (isValidDP(dpToMap)) Form_match[recordPosition].Add(dpToMap.Field.OID, dpToMap.Data);
                }
            }

        }
    }

}

int GetActiveRC(Records allPageRecord, string formOID)
{
    int acitve = allPageRecord.Count - 1;
    foreach (Record record in allPageRecord)
    {
        if (!IsValidRC(record)) continue;

        DataPoint num = record?.DataPoints.FindByFieldOID(num_OID);
        if (num == null) continue;
        if (num?.Data != string.Empty)
        {
            if (!num.Active && !Form_match.ContainsKey(num?.Data)) acitve--;
        }
        else
        {
            if (AtLeastOneValue(record)) acitve--;
        }

    }
    return acitve;
}

void AddLogLine(DataPage enter, string targetForm_OID)
{
    int active = GetActiveRC(enter.Records, targetForm_OID);
    if (active < Form_match.Count)
    {
        for (int j = active; j < Form_match.Count; j++) enter.AddLogRecord();
    }
}

bool AtLeastOneValue(Record rc)
{
    if (rc == null) return true;
    foreach (DataPoint dp in rc?.DataPoints)
    {
        if (isValidDP(dp) && !nonLOGFields.Contains(dp.Field.OID) && (dp?.Field.ControlType == "CheckBox" ? dp?.Data == "1" : dp?.Data != string.Empty))
        {
            return true;
        }
    }
    return false;
}

bool IsDeriveFields(string OID) => deriveFields.ContainsKey(ChangeOID(OID).Substring(0, 3)) && deriveFields[ChangeOID(OID).Substring(0, 3)].Contains(ChangeOID(OID));
void DoEnter(DataPoint num)
{
    num.Record.Active = true;
    foreach (string OID in Form_match[num.Data].Keys)
    {
        if (IsDeriveFields(OID))
        {
            DataPoint enterDP = num.Record.DataPoints.FindByFieldOID(ChangeOID(OID));
            EnterData(enterDP, Form_match[num.Data][OID]);
        }
    }
}
void DoEnter(DataPoint num, string position)
{
    num.Record.Active = true;
    num.UnFreeze();
    num.Enter(position, string.Empty, 0);
    num.Freeze();
    foreach (string OID in Form_match[position].Keys)
    {
        if (IsDeriveFields(OID))
        {
            DataPoint enterDP = num.Record.DataPoints.FindByFieldOID(ChangeOID(OID));
            EnterData(enterDP, Form_match[position][OID]);
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
