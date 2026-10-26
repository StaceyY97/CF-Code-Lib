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
public class VS3_TIMEPOINTS1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
* Created By :MXZ/18Jan2024
* Custom Function :VS3_TIMEPOINTS1
* Edit Checks :VS3_TIMEPOINTS1
VS3_TIMEPOINTS1_1

*********************************************************************************************************/
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpAction = afp.ActionDataPoint;
Subject subject = dpAction.Record.Subject;
string formOid = "VS3";
string fieldOid_TimePoint = "VSTPT2";

 string[] strFolder =
 {
 "C3D1", "C4D1", "C5D1", "C6D1", "C7D1", "C8D1", "C9D1", "C10D1", "C11D1", "C12D1", "C13D1", "C14D1", "C15D1",
 "C16D1", "C17D1", "C18D1", "C19D1", "C20D1", "C21D1", "C22D1", "C23D1", "C24D1", "C25D1", "C26D1", "C27D1",
 "C28D1", "C29D1", "C30D1", "C31D1", "C32D1", "C33D1", "C34D1", "C35D1", "C36D1", "C37D1", "C38D1", "C39D1", "C40D1",
 "C41D1","C42D1","C43D1","C44D1","C45D1","C46D1","C47D1","C48D1","C49D1", "C50D1", "C51D1", "C52D1", "C53D1", "C1D15","C2D15","C3D15","C4D15","C5D15","C6D15",
 "C7D15","C8D15","C9D15","C10D15","C11D15","C12D15","C13D15","C14D15","C15D15","C16D15","C17D15","C18D15","C19D15",
 "C20D15","C21D15","C22D15","C23D15","C24D15","C25D15","C26D15","C27D15"}
 ;

 string[] strPKTPT =
 {
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN", "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN",
 "PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN","PRE-INFUSION|END OF INFUSION (EOI)|EOI+60MIN"
};

ArrayList arFolders = new ArrayList(strFolder);
ArrayList arTimePoint = new ArrayList(strPKTPT);

Instance instDPAction = dpAction.Record.Instance;

if (instDPAction == null || (instDPAction != null && instDPAction.Folder.OID != "SCR" && !arFolders.Contains(instDPAction.Folder.OID))) return null;

DataPoints dptsVISYN = new DataPoints();

if (dpAction.Field.OID == "VISYN") dptsVISYN.Add(dpAction);

else
 dptsVISYN = CustomFunction.FetchAllDataPointsForOIDPath("VISYN", "VISIT", null, subject);

string Yes = "Y";

for (int i = 0; i < dptsVISYN.Count; i++)
{
 if (dptsVISYN[i] == null || !dptsVISYN[i].Active || dptsVISYN[i].Record.RecordPosition != 0) continue;

 Instance inst = dptsVISYN[i].Record.DataPage.Instance;
 if (inst != null && inst.Active && arFolders.Contains(inst.Folder.OID))
 {
  DataPage Dpg_VS3 = GetDatapage(inst, formOid);
  if (string.Compare(dptsVISYN[i].Data.Trim().ToUpper(), Yes, true) == 0)
  {
   int index = arFolders.IndexOf(inst.Folder.OID);
   if (index != -1)
   {
    string strTimePoint = arTimePoint[index].ToString();
    string[] timePoints = strTimePoint.ToString().Split('|');

    if (Dpg_VS3 != null && Dpg_VS3.Active)
    {
     AddlogLines(Dpg_VS3, fieldOid_TimePoint, timePoints);
    }
   }
  }
 }
}
return null;

}
DataPage GetDatapage(Instance inst, string FormOID)
{
if(inst!=null)
{
 DataPage Dpg = inst.DataPages.FindByFormOID(FormOID);
 if(Dpg!=null)
 {
  return Dpg;
 }
}
return null;
}

DataPage newDpage = null;
void AddlogLines(DataPage dpg, string fieldOid_TimePoint, string[] timePoints)
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
   EnterData(dpTimePoint, timePoints[i]);
  }
 }
}
}
void EnterData(DataPoint dp, string val)
{
if (dp != null && dp.Active && string.Compare(dp.Data.Trim(), val, true) != 0)
{
 if (dp.LockStatus != LockStatusEnum.Locked)
 {
  dp.Enter(val, string.Empty, 0);
  dp.Freeze();
 }}
}
}
}
