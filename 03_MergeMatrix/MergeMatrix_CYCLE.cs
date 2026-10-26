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
public class MergeMatrix_CYCLE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
      * CF Name: MergeMatrix_CYCLE
      * Programmer Name:YueXu (14 Nov 2024)
      * Last Modified: Not Applicable
      * Reason Modified: Not Applicable
      * Description: add cycles **/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAct = afp.ActionDataPoint;
    Subject cSUB = dpAct.Record.Subject;
    Instance curIns = dpAct.Record.DataPage.Instance;
    System.Collections.Generic.List<string> lsFOLDEROID = new System.Collections.Generic.List<string>()
{
    "C07D01", "C08D01", "C09D01", "C10D01", "C11D01", "C12D01", "C13D01", "C14D01", "C15D01", "C16D01",
    "C17D01", "C18D01", "C19D01", "C20D01", "C21D01", "C22D01", "C23D01", "C24D01", "C25D01", "C26D01", "C27D01", "C28D01", "C29D01"
}
    ;
    if (curIns != null && lsFOLDEROID.Contains(curIns.Folder.OID))
    {
        string str_cycle = curIns.Folder.OID.Substring(1, 2);
        int int_cycle = int.Parse(str_cycle) + 1;
        string cycle_oid = "CYCLE" + int_cycle.ToString();
        Matrix targetMatrix = Matrix.FetchByOID(cycle_oid, cSUB.CRFVersion.ID);


        if (dpAct.Data == "Y")
        {
            cSUB.MergeMatrix(targetMatrix);
            string ins1_oid = "C" + int_cycle.ToString().PadLeft(2, '0') + "D01";
            Instance ins1 = cSUB.Instances.FindByFolderOID(ins1_oid);
            ins1.SetInstanceName("");
        }
        else cSUB.UnMergeMatrix(targetMatrix);
    }
    return null;
}
}
}
