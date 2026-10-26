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
public class CF_Aid_CleanExtraCycle : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
    Subject curSub = (Subject)inputParameters[0];
    System.Collections.Generic.Dictionary<string, int[]> checkCycles = (System.Collections.Generic.Dictionary<string, int[]>)inputParameters[1];
    Commen = (string[])inputParameters[2];
                        if (curSub.Name == "C501-QY01") Message.SendEmail("qian.yang@edetek.com", "test_qy_2@mdsol.com", "test_qy", $"test_enter");

    System.Collections.Generic.List<string> manageFolders = new System.Collections.Generic.List<string> { };

    foreach (string key in checkCycles.Keys)
    {
        int[] cycle = checkCycles[key];
        int lowRange = key == "D15" ? cycle[0] : cycle[0] + 1;
        int upperRange = key == "D15" ? cycle[1] : cycle[1] + 1;
        for (int cycleNumber = lowRange; cycleNumber < upperRange + 1; cycleNumber++)
        {
            string folderOID = "C" + cycleNumber.ToString() + key;
            manageFolders.Add(folderOID);
        }
    }


    Instance parentIns = curSub.Instances.FindByFolderOID("TRT");

    if (parentIns != null)
    {
        foreach (Instance CheckIns in parentIns.Instances)
        {
            if (CheckIns == null || !CheckIns.Active) continue;
            string checkFolder = CheckIns.Folder.OID;
            if (checkFolder == "C3D22") continue;
            int index_C = checkFolder.IndexOf("C");
            int index_D = checkFolder.IndexOf("D");
            if (int.TryParse(checkFolder.Substring(index_C + 1, index_D - index_C - 1), out int cycleNumber))
            {
                if (cycleNumber < 4) continue;
                if (!manageFolders.Contains(checkFolder)) InActiveIns(CheckIns);
            }
        }
    }
    return null;
}
string[] Commen = new string[] { };

private void InActiveIns(Instance inst)
{
    foreach (string formOID in Commen)
    {
        DataPage dpg = inst.DataPages.FindByFormOID(formOID);
        if (dpg != null && dpg.Active && !dpg.IsBitSet(Status.IsTouched))
        {
            dpg.Active = false;
        }
    }
}
}
}
