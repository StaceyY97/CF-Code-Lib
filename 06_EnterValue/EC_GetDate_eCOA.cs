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
public class EC_GetDate_eCOA : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
        * Created By :Qian Yang/27Dec2024
        * Custom Function :EC_GetDate_eCOA
        * Description: get most recent screening or re-screening date and enter to eCOA
        * Edit Checks :Derive_ECOADER1, Derive_ECOADER1_2,     Merge_D1
        *********************************************************************************************************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;
            Instance currentFolder = dpAction.Record.Instance;
            string folderOID = currentFolder.Folder.OID;

            if (folderOID != "SCREEN_1" && folderOID != "RESCREEN_1"&&folderOID != "SCREEN_2" && folderOID != "RESCREEN_2") return null;

            DataPoint dp_enter = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "D1", "ECOA", "ECOADER1" });
            DataPoint dp_match_S1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "SCREEN_1", "PDIARY", "SCRDOV" });
            DataPoint dp_match_RS1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "RESCREEN_1", "PDIARY", "RSCRDOV" });
            string enterValue;
            if (dp_match_S1 != null && dp_match_RS1 != null && dp_match_S1.Active && dp_match_RS1.Active&& dp_match_S1.IsVisible && dp_match_RS1.IsVisible)
            {
                if (dp_match_S1.StandardValue() is DateTime && dp_match_RS1.StandardValue() is DateTime) enterValue = (DateTime)dp_match_S1.StandardValue() > (DateTime)dp_match_RS1.StandardValue() ?
                        dp_match_S1.Data : dp_match_RS1.Data;
                else if (dp_match_S1.StandardValue() is DateTime) enterValue = dp_match_S1.Data;
                else if (dp_match_RS1.StandardValue() is DateTime) enterValue = dp_match_RS1.Data;
                else enterValue = "";
            }
            else if (dp_match_S1 == null && dp_match_RS1 == null)
            {
                enterValue = "";
            }
            else
            {
                enterValue = dp_match_S1 == null|| !dp_match_S1.IsVisible ? (dp_match_RS1!=null&&dp_match_RS1.IsVisible ? dp_match_RS1.Data : "") : dp_match_S1.Data;
            }

            EnterData(dp_enter, enterValue);


            return null;
        }

        public void EnterData(DataPoint dp, string val)
        {
            if (dp != null && dp.Active && !dp.IsDataPointLocked && string.Compare(dp.Data, val, true) != 0)
            {
                dp.UnFreeze();
                dp.Enter(val, string.Empty, 0);
                dp.Freeze();
            }
}
}
}
