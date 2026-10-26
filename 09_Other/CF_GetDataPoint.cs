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
public class CF_GetDataPoint : CustomFunctionBase
{
public object Eval(object ThisObject)
{
//Quick tool to get a simple form datapoint
//Related CFs: 

object[] inputParameters = (object[])ThisObject;
Subject currentSubj = (Subject)inputParameters[0];
string parentFolderOID = (string)inputParameters[1];
string folderOID = (string)inputParameters[2];
string formOID = (string)inputParameters[3];
string feildOID = (string)inputParameters[4];

DataPoint dt = null;
Instance Folder = null;
if (currentSubj != null && currentSubj.Active)
{
    if (parentFolderOID != "")
    {
        Instance tempIns = currentSubj.Instances.FindByFolderOID(parentFolderOID);
        if (tempIns == null) return dt;
        Folder = tempIns.Instances.FindByFolderOID(folderOID);
        int j = 0;
        while (j < 100 && Folder == null)
        {
            Folder = tempIns.Instances.FindByFolderOID(folderOID);
            j++;
        }
        if (Folder == null) return dt;
    }
    else
    {
        Folder = currentSubj.Instances.FindByFolderOID(folderOID);
        if (Folder == null) return dt;
    }
    DataPage tempPage = Folder.DataPages.FindByFormOID(formOID);
    int i = 0;
    while (i < 100 && tempPage==null)
    {
        tempPage = Folder.DataPages.FindByFormOID(formOID);
        i++;
    }
    if (tempPage == null) return dt;
    DataPoint tempDP = tempPage.MasterRecord.DataPoints.FindByFieldOID(feildOID);
    return tempDP;
}
return dt;
}
}
}
