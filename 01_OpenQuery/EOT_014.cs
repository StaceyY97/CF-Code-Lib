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
public class EOT_014 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
       
         Programmer Name: MXZ/26Jaan2024
        * Custom Function: EOT_014
        * Edit Check(s): EOT_015, EOT_015_1
        * Descriptions:If TTREAS=Pregnancy then "Result" on latest Pregnancy Test form must be 'Positive'. When
<EOT.TTREAS & LBPREG.LBORRES are not null>  
        **************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint Dpt_TTREAS = afp.ActionDataPoint;

            Subject current_subject = Dpt_TTREAS.Record.Subject;
            // Declaration Start
            string querytext = "Reason for treatment discontinuation was reported as 'Pregnancy'; however, \"Result\" on latest Pregnancy Test form is 'Negative'. Please verify & update.";
            bool queryvalue = false;
            string FieldOID_LBHCG = "LBHCG", FieldOID_LBDAT = "LBDAT", FormOID = "LBPREG";
            string Actionv = "POSITIVE", REAS = "4";


            if (Dpt_TTREAS.Data != string.Empty && Dpt_TTREAS.Data == REAS)
            {
                DataPoints dpts = CustomFunction.FetchAllDataPointsForOIDPath(FieldOID_LBHCG, FormOID, null, current_subject);

                DataPoint dptlatest = GetLatestDataPoint(dpts, FieldOID_LBDAT);

                queryvalue = dptlatest!=null && dptlatest.Data!=string.Empty && dptlatest.Data!=Actionv;
            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, Dpt_TTREAS, queryvalue, afp.CheckID, afp.CheckHash);

            return null;
        }
        

        DataPoint GetLatestDataPoint(DataPoints dpts, string FieldOID)
        {
            DateTime dt = DateTime.MinValue;
            DataPoint dptlatest = null;
            for (int i = 0; i < dpts.Count; i++)
            {
                if (!dpts[i].Active) continue;

                if(dpts[i].Data != string.Empty )
                {
                    DataPoint dpt = dpts[i].Record.DataPoints.FindByFieldOID(FieldOID);

                    if(dpt!=null && dpt.StandardValue() is DateTime && (DateTime)dpt.StandardValue() > dt)
                    {
                        dt = (DateTime)dpt.StandardValue();
                        dptlatest = dpts[i];
                    }
                }
            }

            return dptlatest;
}
}
}
