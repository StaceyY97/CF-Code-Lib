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
public class EC_CLEAN_FOLDERNAME : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: CF_AddSubSequentFolder
        * Programmer Name:Gopi Namballa(11 JAN 2022)
        * Last Modified: Not Applicable
        * Reason Modified: Not Applicable
        * Description: Remove text (1) text matrix folders**/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dp = afp.ActionDataPoint;
        Subject currSubj = dp.Record.Subject;

        renameFolder(currSubj);

        return "";
    }
    void renameFolder(Subject currSubj)
    {
        Instances ins = currSubj.Instances;
        if (ins.Count > 0)
        {
            for (int i = 0; i < ins.Count; i++)
            {

                if (ins[i].Folder.OID != "UNA" && ins[i].Folder.OID != "CX" && ins[i].Folder.OID != "SURFUX" && ins[i].Folder.OID != "FUX" && ins[i].Folder.OID != "TUX")
                {

                    ins[i].SetInstanceName("");
                    Instances subins = ins[i].Instances;
                    if (subins.Count > 0)
                    {
                        for (int j = 0; j < subins.Count; j++)
                        {
                            if (subins[j].Folder.OID != "UNA" && subins[j].Folder.OID != "CX" && subins[j].Folder.OID != "SURFUX" && subins[j].Folder.OID != "FUX" && subins[j].Folder.OID != "TUX")
                            {
                                subins[j].SetInstanceName("");
                            }
                        }

                    }
                }
            }
        }
}
}
}
