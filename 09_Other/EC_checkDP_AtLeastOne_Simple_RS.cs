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
public class EC_checkDP_AtLeastOne_Simple_RS : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
        * Created By :YY/27Dec2024
        * Description:EC_checkDP_AtLeastOne_Simple_/ check fields over simple forms
        * Edit Checks :Merge_RS1
        *********************************************************************************************************/

        List<string> Parent_Folders = new List<string>()
        {
            "", "", ""
        }
        ;
        List<string> Folders = new List<string>()
        {
            "SCREEN_1", "SCREEN_2", "D1"
        }
        ;
        List<string> Forms = new List<string>()
        {
            "RS", "RS", "RS"
        }
        ;
        List<string> Fields = new List<string>()
        {
            "RSYN", "RSYN", "RSYN"
        }
        ;
        List<string> Matched_Code = new List<string>
        {
            "Y"
        }
        ;


        DataPoint dpAction = (DataPoint) ThisObject;


        for (int i = 0; i< Folders.Count; i++)
        {
            DataPoint tempDP = (DataPoint) CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, Parent_Folders[i], Folders[i], Forms[i], Fields[i] } );
            if (tempDP != null && tempDP.Active && Matched_Code.Contains(tempDP.Data)) return true;
        }

        return false;
}
}
}
