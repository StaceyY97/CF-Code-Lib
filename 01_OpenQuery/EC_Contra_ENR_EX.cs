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
public class EC_Contra_ENR_EX : CustomFunctionBase
{
public object Eval(object ThisObject)
{
string QUERY_TEXT = "Subject has been randomized. Please add “First On-Site Dose” in Study Drug Dosing (RCP) form.";
            const int MARKING_GROUP_ID = 1;
            const bool ANSWER_ON_CHANGE = false;
            const bool CLOSE_ON_CHANGE = false;
            bool openQuery = false;

            CFField queryField = new CFField() { OID = "ELDAT", FormOID = "ENR" };
            CFField aidField_1 = new CFField() { OID = "ELYN", FormOID = "ENR" };
            CFField aidField_2 = new CFField() { OID = "EXYN", FormOID = "EX", FolderOID = "LOGS" };

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject currentSubject = dpAction.Record.Subject;

            if (dpAction == null || !dpAction.Active || currentSubject == null) return null;

            DataPoints queryDPs = CustomFunction.FetchAllDataPointsForOIDPath(queryField.OID, queryField.FormOID, null, currentSubject);
            foreach (DataPoint queryDP in queryDPs)
            {
                if (queryDP == null || !queryDP.Active || queryDP.Record.RecordPosition != 0) continue;
                DataPoint aidDP_1 = GetSimpleDP(aidField_1.OID, queryDP);
                DataPoint aidDP_2 = GetSimpleDP(aidField_2.OID, aidField_2.FormOID, aidField_2.FolderOID, currentSubject);

                openQuery = IsvalidDp(aidDP_1)&& aidDP_1.Data=="Y"&&(!IsvalidDp(aidDP_2)|| aidDP_2.Data=="N");
                CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, queryDP, openQuery, afp.CheckID, afp.CheckHash);
            }

            return null;
        }

        DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID, Subject currentSubject) =>
currentSubject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);

        DataPoint GetSimpleDP(string fieldOID, DataPoint dpOfSamePage) => dpOfSamePage.Record.DataPoints.FindByFieldOID(fieldOID);
        bool IsvalidDp(DataPoint dp) => dp != null && dp.Active && dp.IsVisible && dp.Data != string.Empty;

        class CFField
        {
            public string OID { set; get; }
            public string FormOID { set; get; }
            public string FolderOID { set; get; } = null;
            public string ParentFormOID { set; get; } = string.Empty;
            public bool IsLog { set; get; } = false;
}
}
}
