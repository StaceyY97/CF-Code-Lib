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
public class PK_TIMEPOINTS2 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
       * Created By : MXZ/18Jan2024
       * Custom Function : PK_TIMEPOINTS2
       * Edit Checks : PK_TIMEPOINTS2_1, PK_TIMEPOINTS2_2, PK_TIMEPOINTS2_3
       * Description : 
      
       *********************************************************************************************************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;
            string formOid = "PK";
            string fieldOid_TimePoint = "PKTP";
           
            string[] strFolder =
           {
            "SFU", "EOT", "UNS"
        }
           ;

            string strTPT = string.Empty;

            ArrayList arFolders = new ArrayList(strFolder);

            Instance inst = dpAction.Record.Instance;

            if (inst == null || (inst != null && !arFolders.Contains(inst.Folder.OID))) return null;


            string Yes = "Y", check = "1";
            bool isAdd = false;

            if (inst != null && arFolders.Contains(inst.Folder.OID))
            {
                if (dpAction.Field.OID == "H_NOW")
                {
                    AddTimepoints(dpAction.Record.DataPage, fieldOid_TimePoint, strTPT);
                }

                else
                {
                    if (string.Compare(dpAction.Data.ToUpper(), Yes, true) == 0 || string.Compare(dpAction.Data.ToUpper(), check, true) == 0) isAdd = true;
                    AddForms(inst, formOid, isAdd, subject);

                    int index = arFolders.IndexOf(inst.Folder.OID);
                    if (index > -1)
                    {
                        if (newDpage != null && newDpage.Active)
                        {
                            AddTimepoints(newDpage, fieldOid_TimePoint, strTPT);
                        }
                    }
                }
            }
            return null;

        }
        DataPage newDpage = null;
        void AddTimepoints(DataPage dpg, string fieldOid_TimePoint, string strTPT)
        {
            if (dpg != null && dpg.Active)
            {
                Records rcds = dpg.Records;
                for (int i = 0; i < rcds.Count; i++)
                {
                    if (!rcds[i].Active || rcds[i].RecordPosition == 0) continue;

                    DataPoint dpTimePoint = rcds[i].DataPoints.FindByFieldOID(fieldOid_TimePoint);
                    EnterData(dpTimePoint, strTPT);

                }
            }
        }
        void EnterData(DataPoint dp, string val)
        {
            if (dp != null && dp.Active && dp.LockStatus != LockStatusEnum.Locked)
            {
                if (dp.ChangeCount == 0)
                {
                    dp.Enter(val, string.Empty, 0);
                    dp.Freeze();
                }

                else if (string.Compare(dp.Data.Trim(), val, true) != 0)
                {
                    dp.Enter(val, string.Empty, 0);
                    dp.Freeze();
                }
            }
        }
        void AddForms(Instance ins, string formOID, bool doAdd, Subject subject)
        {
            string fieldOid_Req = "PKYN";
            newDpage = ins.DataPages.FindByFormOID(formOID);
            if (ins != null && ins.Active)
            {
                if (doAdd)
                {
                    if (newDpage == null)
                    {
                        //Form newForm = Form.FetchByOID(formOID, ins.Subject.CRFVersion.ID);
                        //newDpage = new DataPage(ins, newForm, ins.SubjectMatrixID);
                        //ins.DataPages.Add(newDpage);
                    }
                    else
                    {
                        newDpage.Active = true;
                    }
                }
                else if (newDpage != null)
                {

                    DataPoint dpReq = newDpage.MasterRecord.DataPoints.FindByFieldOID(fieldOid_Req);
                    if (dpReq != null && dpReq.ChangeCount == 0)
                    {
                        //newDpage.Active = false;
                        
                    }
                }
            }
}
}
}
