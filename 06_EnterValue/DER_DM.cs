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
public class DER_DM : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dp = afp.ActionDataPoint;
    if (dp == null) return null;
    Subject subj = dp.Record.Subject;
    Instance curIns = dp.Record.DataPage.Instance;
    DataPage curPage = dp.Record.DataPage;

    string[] dataFields = new string[]
    {
        "DMRACE1",
        "DMRACE2",
        "DMRACE3",
        "DMRACE4",
        "DMRACE5",
        "DMRACE6",
        "DMRACE7",
        "DMRACESP",
        "DMETHNIC"
    };
    string formOID = "DM", folderOID_source = "SCREEN_1", folderOID_target = "RESCREEN_1";

    DataPage sourcePage = subj.Instances.FindByFolderOID(folderOID_source)?.DataPages.FindByFormOID(formOID);
    DataPage targetPage = subj.Instances.FindByFolderOID(folderOID_target)?.DataPages.FindByFormOID(formOID);
    if (sourcePage == null || targetPage == null) return null;
    foreach (string df in dataFields)
    {
        DataPoint sourceDP = sourcePage.MasterRecord.DataPoints.FindByFieldOID(df);
        DataPoint targetDP = targetPage.MasterRecord.DataPoints.FindByFieldOID(df);
        if (sourceDP != null && targetDP != null)
        {
            EnterData(targetDP, sourceDP.Data);
        }
    }
    return null;
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
