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
public class CF_Aid_AEFieldsLevel : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[]) ThisObject;

    Dictionary<string, List<string>> AEFieldsLevel = new Dictionary<string, List<string>>()
    {
        {"AESER", new List<string> { "AETERM", "AESTDAT", "AEONGO", "AEENDAT", "AEDLT", "AETOXGR", "AEOVER", "AESCR", "AEREL","AEACN2", "AEACN3", "AEACN1", "AEACN5", "AEACN4","AEACN6","AECONTRT","AEACNOTH", "AEOUT", "AEDIS","AESI"}},
{"AEREL_FU", new List<string> {"AEREL_FU","AEACN6", "AEACN7", "AEACN8", "AEACN9", "AEACN10"}},
{"AEREL_PA", new List<string> {"AEREL_PA","AEACN11", "AEACN12", "AEACN13", "AEACN14", "AEACN15"}},
        {"AEPD", new List<string> { "AEPD" }},
        {"AECMTSE1", new List<string> { "AECMTSE1", "AECMTSE2", "AECMTSE3", "AECMTSE4", "AECMTSE5" }},
        {"AEACNSP", new List<string> { "AEACNSP" }},
        {"AESEQ", new List<string> { "AESEQ" }},
        {"AEAXMP", new List<string> { "AEAXMP", "SAEDDAT","AESDTH","AESLIFE","AESHOSP","AESDISAB","AESCONG","AESMIE"}},
{"SAE3DAT", new List<string> { "SAE3DAT", "SAE4DAT","SAECOM","SAECOM1","SAECOM2","AEABATE","REOCCUR" }},
{"SAE3DAT_FU", new List<string> { "SAE3DAT_FU","SAE4DAT_FU" }},
{"SAE3DAT_PA", new List<string> { "SAE3DAT_PA","SAE4DAT_PA" }},
        {"AEDTHDAT", new List<string> { "AEDTHDAT" }},
        {"HADMDAT", new List<string> { "HADMDAT", "HDCDAT" }},
    };
        return AEFieldsLevel ;
}
}
}
