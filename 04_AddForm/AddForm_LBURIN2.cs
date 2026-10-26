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
public class AddForm_LBURIN2 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*******************************************************
            * Created By : YueXu 2Jan2025
            * Custom Function : AddForm_LBURIN2
            * Edit Checks : AAddForm_LBURIN21 AddForm_LBURIN22 AddForm_LBURIN23
            * Descreiption: Add forms.
            ***************************/

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Instance curIns = dpAction.Record.DataPage.Instance;
            Subject subject = dpAction.Record.Subject;
            bool doAdd = false;
            string form_oid = "LBURIN2";
            if (curIns.Folder.OID == "UNS") return null;
            if (dpAction.Field.OID == "VISDAT" )
            {
                DataPage dpg_LBURIN = dpAction.Record.DataPage.Instance.DataPages.FindByFormOID("LBURIN1");
                if (dpAction.Active && dpAction.StandardValue() is DateTime && dpg_LBURIN != null && check_form(dpg_LBURIN)) doAdd = true;
                AddForms(dpAction.Record.DataPage.Instance, form_oid, doAdd, subject);


            }
            else
            {
                DataPoints dps_VISDAT = CustomFunction.FetchAllDataPointsForOIDPath("VISDAT", "VISIT", dpAction.Record.DataPage.Instance.Folder.OID, subject);
                if (dps_VISDAT != null && dps_VISDAT.Count > 0 && dps_VISDAT[0].Active && dps_VISDAT[0].StandardValue() is DateTime && check_form(dpAction.Record.DataPage)) doAdd = true;
                AddForms(dpAction.Record.DataPage.Instance, form_oid, doAdd, subject); 

                
            }
            return null;
        }
        DataPage newDpage = null;
        void AddForms(Instance ins, string formOID, bool doAdd, Subject subject)
        {
            newDpage = ins.DataPages.FindByFormOID(formOID);
            if (ins != null && ins.Active)
            {
                if (doAdd)
                {
                    if (newDpage == null)
                    {
                        Form newForm = Form.FetchByOID(formOID, ins.Subject.CRFVersion.ID);
                        newDpage = new DataPage(ins, newForm, ins.SubjectMatrixID);
                        ins.DataPages.Add(newDpage);
                    }
                    else
                    {
                        newDpage.Active = true;
                    }
                }
                else if (newDpage != null)
                {
                    newDpage.Active = false;
                }
            }

        }
        System.Collections.Generic.List<string> field_oid = new System.Collections.Generic.List<string>()
        {
            "URNPR", "URNGLUC", "URNBL", "UNRLE"
        };

        bool check_form(DataPage dpg)
        {
            bool check = false;
            DataPoint dp_URINYN = dpg.MasterRecord.DataPoints.FindByFieldOID("URINYN");
            if (dp_URINYN != null && dp_URINYN.Active && dp_URINYN.Data == "Y")
            {
                foreach (string i in field_oid)
                { 
                  DataPoint dp = dpg.MasterRecord.DataPoints.FindByFieldOID(i);
                    if (dp != null && dp.Active && dp.Data == "ABNORMAL")
                    {
                        check = true;
                        break;
                    }
                }
            }
            return check;
}
}
}
