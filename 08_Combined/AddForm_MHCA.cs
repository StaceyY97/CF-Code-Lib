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
public class AddForm_MHCA : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*******************************************************
* Created By : YueXu 18Nov2024
* Custom Function : AddForm_MHCA
* Edit Checks : Related ECs
* Descreiption: Add form and set default Characterization of disease at enrollment.
***************************/

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;
            Instance curIns = dpAction.Record.DataPage.Instance;
            string formOid = "MHCA";
            string fieldOid = "MHCAT";
            string type = "";

            bool doAdd = false;

            if (dpAction.Field.OID == "CANTYPE")
            {
                DataPoints IEYN = CustomFunction.FetchAllDataPointsForOIDPath("IEYN", "IE", "SCRN", subject);
                DataPoints IEYN1 = CustomFunction.FetchAllDataPointsForOIDPath("IEYN1", "IE", "SCRN", subject);
                DataPoints IEYN2 = CustomFunction.FetchAllDataPointsForOIDPath("IEYN2", "IE", "SCRN", subject);
                if (dpAction.Data != "" && (Check(IEYN[0]) || Check(IEYN1[0]) || Check(IEYN2[0]))) doAdd = true;
                type = dpAction.Data;
            }
            else
            {
                DataPoint IE1 = dpAction.Record.DataPoints.FindByFieldOID("IEYN1");
                DataPoint IE2 = dpAction.Record.DataPoints.FindByFieldOID("IEYN2");
                DataPoints CANTYPE = CustomFunction.FetchAllDataPointsForOIDPath("CANTYPE", "DSENR", "SCRN", subject);
                if (CANTYPE[0].Data != "" && (Check(dpAction) || Check(IE1) || Check(IE2))) doAdd = true;
                type = CANTYPE[0].Data;
            }

            AddForms(curIns, formOid, doAdd, subject);

            if (doAdd && newDpage != null)
            {
                if (type == "1") hide(BC, CRPC, NSCLC);
                else if (type == "2") hide(CRPC, NSCLC, BC);
                else if (type == "3") hide(NSCLC, BC, CRPC);
            }

            return null;
        }
        DataPage newDpage = null;

        System.Collections.Generic.List<string> BC = new System.Collections.Generic.List<string>()
    {
        "BCLOC", "BCTYP", "BCGRD", "BCBRCA1", "BCBRCA2", "BCPIK3CA", "BCESR1", "BCP53", "BCBCHER2", "BCNONE", "BCOTH", "BCER",
        "BCPR", "BCKI67", "BCHER2", "BCIHC", "BCFISH"
    }
        ;
        System.Collections.Generic.List<string> CRPC = new System.Collections.Generic.List<string>()
    {
        "PCHIS", "PCGRADE", "PCGLPR", "PCGLSC", "PCGLTO", "PCPSA", "PCBRCA1",
        "PCBRCA2", "PCATM", "PCPALB2", "PCCHEK2", "PCHOXB13", "PCMLH1", "PCMSH2", "PCMSH6", "PCPMS2", "PCNONE", "PCOTH"
    }
        ;

        System.Collections.Generic.List<string> NSCLC = new System.Collections.Generic.List<string>()
    {
        "LCTYPE", "LCEGFR", "LCALK", "LCROS1", "LCBRAF", "LCKRAS", "LCMET", "LCRET", "LCNTRK", "LCNONE", "LCOTH"

    }
        ;


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

        bool Check(DataPoint a)
        {
            if (a != null && a.Active && a.Data == "Y") return true;
            return false;
        }

        void hide(List<string> a, List<string> b, List<string> c)
        {
            set_hide(a, true);
            set_hide(b, false);
            set_hide(c, false);
        }
        void set_hide(List<string> a, bool condition)
        {
            foreach (string i in a)
            {
                DataPoint dp = newDpage.MasterRecord.DataPoints.FindByFieldOID(i);
                if (dp != null) dp.IsVisible = condition;
            }
}
}
}
