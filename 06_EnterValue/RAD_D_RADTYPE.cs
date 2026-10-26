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
public class RAD_D_RADTYPE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
        * Created By : Xiaorui
        * Custom Function : RAD_D_RADTYPE
        * Edit Checks : RAD_D_RADTYPE
        * Description :

        *********************************************************************************************************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpRADTYP = afp.ActionDataPoint;
        Subject subject = dpRADTYP.Record.Subject;

        string fieldOid1 = "D_RADTYPE", fieldOid2 = "OTHSP", strVal= string.Empty;
        Instance inst = dpRADTYP.Record.Instance;
        if (inst == null || !inst.Active) return null;


        DataPoint dptD_RADTYPE = dpRADTYP.Record.DataPoints.FindByFieldOID(fieldOid1);
        DataPoint dptD_OTHSP = dpRADTYP.Record.DataPoints.FindByFieldOID(fieldOid2);

        if(dpRADTYP.Data !=string.Empty && dpRADTYP.Data != "OTH")
        {
            if (dpRADTYP.UserValue() != null) strVal = dpRADTYP.UserValue().ToString().ToUpper();
        }
        else if(dpRADTYP.Data == "OTH")
        {
            if (dptD_OTHSP!=null) strVal = dptD_OTHSP.Data.ToUpper().Trim();
        }

        EnterData(dptD_RADTYPE, strVal);

        return null;

    }

    void EnterData(DataPoint dp, string val)
    {
        if (dp != null && dp.Active)
        {
            if (dp.LockStatus != LockStatusEnum.Locked)
            {
                dp.Enter(val, string.Empty, 0);

            }
        }
}
}
}
