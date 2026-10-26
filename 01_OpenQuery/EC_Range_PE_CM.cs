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
public class EC_Range_PE_CM : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
        * Created By :Yuxin Zeng/21Jan2025
        * Custom Function :EC_Range_PE_CM
        * Description: IF ( ( "Height Unit" is equal to "cm" )
        AND ( ( ALL.PE.HEIGHT Height is less than "120cm" ) OR ( ALL.PE.HEIGHT Height is less than "210cm" ) ) ) THEN OpenQuery
        * Edit Checks :Range_HEIGHT_CM
        *******************************************/
        const bool closeOnChange = false;
        const bool answerOnChange = false;
        const int markingGroup = 1;
        string query ="Height is out of the range. Please verify and update, else clarify.";

        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        bool openQuery= false;

        if (dpAction.Active)
        {
            if (Number.IsValidNumber(dpAction.Data.Trim()) && !dpAction.IsDataPointNonConformant)
            {
                string unit = GetUnit(dpAction);
                if (unit != string.Empty && unit == "CM")
                {
                    double U = Convert.ToDouble(dpAction.Data.Trim());
                    openQuery  = U < 120|| U > 210;
                }
            }
        }
CustomFunction.PerformQueryAction(query, markingGroup, false, false, dpAction, openQuery, afp.CheckID, afp.CheckHash);
        return null;
    }
    private string GetUnit(DataPoint Dp)
    {

        string UnitsofTemp = string.Empty;
        if (Dp.UnitDictionaryEntry != null)
        UnitsofTemp = Dp.UnitDictionaryEntry.UserUnitString.ToUpper();
        return UnitsofTemp;
}
}
}
