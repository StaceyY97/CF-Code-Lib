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
public class EC_Contra_RSPTLRESP_TLDIAMNE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
string QUERY_TEXT = "Evaluation of target lesion on Overall response-RECIST 1.1 form is NE (Not Evaluable), however, the special circumstances \"Lesion not evaluated/evaluable\" " +
                "is not checked for at least one of the lesions on Tumor Assessment-Target Lesion form. Please verify and update, else clarify.";
            const int MARKING_GROUP_ID = 1;
            const bool ANSWER_ON_CHANGE = false;
            const bool CLOSE_ON_CHANGE = false;
            bool openQuery = false;

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject currentSubject = dpAction.Record.Subject;

            Instance currentIns = dpAction.Record.Instance;

            if (dpAction == null || !dpAction.Active || currentSubject == null || currentIns == null) return null;

            string currInsOID = currentIns.Folder.OID;

            CFField queryField = new CFField() { OID = "RSPTLRESP", FormOID = "RSP", FolderOID = currInsOID,TriggerValue = "NE" };
            queryField.GetDP(currentSubject);

            CFField aidField_1 = new CFField() { OID = "TLDIAMNE", FormOID = "TL", FolderOID = currInsOID, TriggerValue = "1", IsLog = true, Conversely = true };
            aidField_1.GetDP(currentSubject);

            if (queryField.IsValid())
            {
                if (queryField.IsMeet()) openQuery = !aidField_1.IsValid()||aidField_1.IsMeet();
                CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, queryField.DP, openQuery, afp.CheckID, afp.CheckHash);
            }
            return null;
        }


        /// <summary>
        /// OID and FormOID are required parameters for CFField class, while FolderOID, ParentFolderOID, IsLog and triggerValue are optional parameters with default values.
        /// FolderOID default value is null.
        /// ParentFolderOID default value is string.Empty.
        /// IsLog default value is false.
        /// triggerValue default value is null.
        /// conversely default value is false.
        /// DP used to store the single DataPoint when IsLog is false, DPs used to store the collection of DataPoints when IsLog is true.
        /// For non-log fields, IsMeet return if DP VALUE equals to triggerValue when conversely is false, return if DP VALUE does not equal to triggerValue and not Empty when conversely is true.
        /// For log fields, IsMeet return if at least one VALUE in DPs equals to triggerValue when conversely is false, return if all VALUEs in DPs not equal to triggerValue when conversely is true.
        /// </summary>
        class CFField
        {
            public string OID { set; get; }
            public string FormOID { set; get; }
            public string FolderOID { set; get; } = null;
            public string ParentFolderOID { set; get; } = string.Empty;
            public bool IsLog { set; get; } = false;
            public string TriggerValue { set; get; } = null;
            public bool Conversely { set; get; } = false;
            public DataPoint DP { private set; get; } = null;
            public DataPoints DPs { private set; get; } = null;
            public bool IsValid() => IsLog ? DPs != null && DPs.Count > 0 : IsValidDp(DP, false);

            public void GetDP(Subject subj)
            {
                if (OID != null)
                {
                    if (!IsLog)
                    {
                        if (FormOID != null && FolderOID != null)
                        {
                            if (ParentFolderOID == string.Empty) DP = GetSimpleDP(OID, FormOID, FolderOID, subj);
                            else DP = GetSimpleDP(OID, FormOID, FolderOID, ParentFolderOID, subj);
                        }
                    }
                    else DPs = CustomFunction.FetchAllDataPointsForOIDPath(OID, FormOID == string.Empty ? null : FormOID, FolderOID == string.Empty ? null : FolderOID, subj);
                }
            }

            public bool IsMeet()
            {
                if (!IsLog) return IsValueMeet();
                else return AtLeaseOneDPMeet();
            }

            bool IsValidDp(DataPoint dp, bool checkEmpty) => dp != null && dp.Active && dp.IsVisible && (checkEmpty ? dp.Data != string.Empty : true);

            bool IsValueMeet()
            {
                if (DP == null || !DP.Active || !DP.IsVisible) return false;
                return Conversely ? DP.Data != TriggerValue : DP.Data == TriggerValue;
            }
            bool AtLeaseOneDPMeet()
            {
                if (DPs == null || DPs.Count == 0) return false;
                foreach (DataPoint dp in DPs)
                {
                    if (IsValidDp(dp, true))
                    {
                        if (Conversely)
                        {
                            if (dp.Data == TriggerValue) return false;
                        }
                        else
                        {
                            if (dp.Data == TriggerValue) return true;
                        }
                    }
                }
                return Conversely;
            }
            DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID, Subject subj) =>
subj.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
            DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID, string parentFolderOID, Subject subj) =>
subj.Instances.FindByFolderOID(parentFolderOID)?.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
}
}
}
