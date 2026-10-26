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
public class CF_Aid_AddnextCycle_WithName : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/// <summary>
 ///CF_Aid_AddnextCycle_WithName
 ///

 object[] inputParas = (object[])ThisObject;
 if (inputParas?.Length != 7) return null;

 ISubject curSub = (ISubject)inputParas[0];
 string InsOID_Parent = (string)inputParas[1];
 string targetFolder = (string)inputParas[2];
 bool doAdd = (bool)inputParas[3];
 string[] forms_Common = (string[])inputParas[4];
 IDataPoint enterDP = (IDataPoint)inputParas[5];
 string insOID = (string)inputParas[6];

 SubjectMatrixRef mtxRef = enterDP.Record.SubjectMatrixRef;
 IInstance ins_target = null;
 IInstance ins_Parent = curSub.Instances.FindByFolderOid(InsOID_Parent);
 IReasonCode activativeReason = enterDP.Interaction.Role?.ActivationReasons.FirstOrDefault();
 IReasonCode inactivativeReason = enterDP.Interaction.Role?.InactivationReasons.FirstOrDefault();

 if (ins_Parent != null)
 {
     bool folderExist = false;
     foreach (IInstance ins in ins_Parent.Instances)
     {
         if (ins == null) continue;
         if (ins.Name == targetFolder)
         {
             folderExist = true;
             if (ins.Active != doAdd)
             {
                 if (doAdd) ins.Activate(activativeReason);
                 else ins.InActivate(inactivativeReason);
             }
         }
     }
     if (!folderExist && doAdd)
     {
         IFolder folder_target = curSub.CrfVersion.Folders.FindByOid(insOID);
         if (folder_target != null)
         {
             ins_target = ins_Parent.AddInstance(folder_target, mtxRef);
             if (ins_target != null)
             {
                 ins_target.SetInstanceName(targetFolder.Replace(ins_target.Name,""));
                 foreach (string str_fm_oid in forms_Common)
                 {
                     ins_target.AddDataPage(curSub.CrfVersion.Forms.FindByOid(str_fm_oid), mtxRef);
                 }
             }
         }
     }
 }
 return null;
}
}
}
