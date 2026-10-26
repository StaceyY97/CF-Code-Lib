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
public class EOTBEV_019 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
      Date:06-NOV-2025
      CF Name: EOTBEV_019
      Edit check name: EOTBEV_019, EOTBEV_019_CROSS
      Description: If TTREAS= Death then at least one AE should be reported with the Outcome= Fatal & 
    CTCAE Severity/Intensity= 5 - Death related to AE. when <  EOTBEV.TTREAS are not null & AE.AEOUT<> Fatal>*/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;
    bool Flag = false;

    string QT = "Reason for treatment discontinuation was reported as Death; however, there is no Grade 5 AE recorded with the Outcome equals Fatal. Please verify & update.";

    string Field_TTREAS = "TTREAS", Form_EOTBEV = "EOTBEV", Folder_EOT = "EOT";
    string Field_AEOUT = "AEOUT", Field_AESEV = "AESEV", Form_AE = "AE", Folder_AE = "AE";

    DataPoint Dp_TTREAS = GetDatapoint(Folder_EOT, Form_EOTBEV, Field_TTREAS, subject);

    bool Openquery = GetCondition(Field_AESEV, Field_AEOUT, Form_AE, Folder_AE, subject);

    if(Dp_TTREAS!=null && Dp_TTREAS.Active && string.Compare(Dp_TTREAS.Data.Trim(), "10", true)==0 &&
        Openquery)
    {
        Flag = true;
    }
    if (Dp_TTREAS != null)
        CustomFunction.PerformQueryAction(QT, 1, false, false, Dp_TTREAS, Flag, afp.CheckID, afp.CheckHash);
    return null;
}

bool GetCondition(string Field_AESEV, string Field_AEOUT, string Form_AE, string Folder_AE, Subject sub)
{
    Records Rds = new Records();
    Record Rd = null;
    DataPoint Dp_AESEV = null, Dp_AEOUT = null;

    Instance Ins = sub.Instances.FindByFolderOID(Folder_AE);
    if(Ins!=null)
    {
        DataPage Dpg = Ins.DataPages.FindByFormOID(Form_AE);
        if(Dpg!=null)
        {
            Rds = Dpg.Records;
            if(Rds.Count>0)
            {
                for(int i=0;i<Rds.Count;i++)
                {
                    Rd = Rds[i];
                    if (Rd == null || !Rd.Active || Rd.RecordPosition == 0) continue;

                    Dp_AESEV = Rd.DataPoints.FindByFieldOID(Field_AESEV);
                    Dp_AEOUT = Rd.DataPoints.FindByFieldOID(Field_AEOUT);

                    if(Dp_AESEV!=null && Dp_AEOUT!=null && string.Compare(Dp_AESEV.Data.Trim(), "5", true)==0 &&
                        string.Compare(Dp_AEOUT.Data.Trim(), "FATAL", true) == 0)
                    {
                        return false;
                    }
                }
            }
        }
    }
    return true;
}

DataPoint GetDatapoint(string Folder, string Form, string Field, Subject sub)
{
    Instance Ins = sub.Instances.FindByFolderOID(Folder);
    if (Ins != null)
    {
        DataPage Dpg = Ins.DataPages.FindByFormOID(Form);
        if (Dpg != null)
        {
            DataPoint Dp = Dpg.MasterRecord.DataPoints.FindByFieldOID(Field);
            if (Dp != null)
            {
                return Dp;
            }
        }
    }
    return null;
}
}
}
