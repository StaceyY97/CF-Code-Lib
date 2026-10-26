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
public class GENERIC_GetMatchedValue : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*************************************************************
        *Name: GENERIC_GetMatchedValue
        //Author: CF GLIB
        //Purpose: Finds a matching value for given OID path
        //Modification history: None
        * expected params:
        * param[0]: string fieldOID
        * param[1]: string formOID
        * param[2]: string folderOID
        * param[3]: Subject afpSubject
        * param[4]: string matchVAL
        * returns: true or false
        *************************************************************/

        object[] param = (object[]) ThisObject;

        if (param.Length != 5)
        return null;

        string strfieldOID = param[0] as string;
        string strformOID = param[1] as string;
        string strfolderOID = param[2] as string;
        Subject subject = (Subject) param[3];
        string strmatchVAL = param[4] as string;


        DataPoints dpsOIDPath = CustomFunction.FetchAllDataPointsForOIDPath(strfieldOID, strformOID, strfolderOID, subject);
        for (int i = 0; i < dpsOIDPath.Count; i++)
        {
            if (dpsOIDPath[i].Active && dpsOIDPath[i].ChangeCount > 0 && (strmatchVAL != null ? dpsOIDPath[i].Data.Trim() == strmatchVAL : true))
            return true;
        }
        return false;
}
}
}
