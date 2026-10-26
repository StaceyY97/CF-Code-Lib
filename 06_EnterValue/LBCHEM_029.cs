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
public class LBCHEM_029 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
* Created By : Qian Yang 23Feb2024
* Custom Function :LBCHEM_029
* Edit Checks : LBCHEM_029
* Description : Calculates Creatinine Clearance
*********************************************************************************************************/
            string CREAT_CLEAR = "CALCCREA";
            string LBFORM = "LBCHEM";
            string WEIGHT = "VSWEIGHT";
            string AGE = "AGE";
            string SEX = "SEX";
            string FEMALE = "F";
            string DMFORM = "DM";
            string DMFOLDER = "DM";
            /************************CF Variables Declaration************************/
            DataPoint dptCREAT_CLEAR = null;
            double age, Main_Cal, Cal_CREAT, wght = double.MinValue;
            bool isFemale = false;
            /************************************************************************/

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dptCREAT = afp.ActionDataPoint;
            Subject sub = dptCREAT.Record.Subject;

            if (dptCREAT == null) return null;

            DataPoint gender = GetDataPoint(sub, "", DMFOLDER, DMFORM, SEX);
            if (gender == null) return null;
            isFemale = (string.Compare(gender.Data, FEMALE, true) == 0);
            DataPoint Age = GetDataPoint(sub, "", DMFOLDER, DMFORM, AGE);
            if (Age == null) return null;
            if ( !Number.IsValidNumber(Age.Data)) return null;
            age = double.Parse(Age.Data);
            DataPoint Weight = GetDataPoint(sub, "", "SCR", "VS1", WEIGHT);
            if (Weight == null) return null;
            if (!Number.IsValidNumber(Weight.Data)) return null;
            wght = double.Parse(Weight.Data);
            if(Weight.UnitDictionaryEntry.UserUnitString.ToUpper() == "LB") wght = wght*0.4536;

            Main_Cal = double.MinValue;
            if (!dptCREAT.Active || dptCREAT.Data == string.Empty || !Number.IsValidNumber(dptCREAT.Data)) return null;
            Cal_CREAT = GetStdValue(dptCREAT);
            dptCREAT_CLEAR = dptCREAT.Record.DataPoints.FindByFieldOID(CREAT_CLEAR);
            if (dptCREAT_CLEAR == null || Cal_CREAT == double.MinValue) return null;
            Main_Cal = ((140 - age) / Cal_CREAT) * (wght / 72);
            if (isFemale) Main_Cal = Main_Cal * 0.85;
            Main_Cal = Math.Round(Main_Cal, 2, MidpointRounding.AwayFromZero);

            if (Main_Cal != double.MinValue)
                EnterData(dptCREAT_CLEAR, Main_Cal.ToString());
            else
                EnterData(dptCREAT_CLEAR, string.Empty);
            return null;
        }

        private double GetStdValue(DataPoint dp_Analyte)
        {
            double stdvalue = double.MinValue;
            double compareValue = 0;
             bool trigger = true;
            if (!CustomFunction.DataPointIsEmpty(dp_Analyte) && Number.IsValidNumber(dp_Analyte.Data) && dp_Analyte.Record.DataPage.Lab != null && dp_Analyte.AnalyteRange != null)
            {
                compareValue = double.Parse (dp_Analyte.Data);
                Medidata.Core.Objects.Labs.AnalyteRange range = dp_Analyte.Record.DataPage.Lab.FindRangeByAnalyte(dp_Analyte.AnalyteRange.AnalyteID);
                //if (range != null)
                //{
                //    if(compareValue < range.LowRange ||  compareValue > range.HighRange) trigger = false;
                //}
                if(trigger) 
                {
                    stdvalue = compareValue;
                    return stdvalue;
                }
            }
            return stdvalue;
        }
        public void EnterData(DataPoint dp, string val)
        {
            if (dp != null && dp.Active && string.Compare(dp.Data, val, true) != 0)
            {
                dp.UnFreeze();
                dp.Enter(val, string.Empty, 0);
            }
        }

        DataPoint GetDataPoint(Subject currentSubj, string parentFolderOID, string folderOID, string formOID, string feildOID)
        {
            DataPoint dt = null;
            Instance Folder = null;
            if (currentSubj != null && currentSubj.Active)
            {
                if (parentFolderOID != "")
                {
                    Instance tempIns = currentSubj.Instances.FindByFolderOID(parentFolderOID);
                    if (tempIns == null) return dt;
                    Folder = tempIns.Instances.FindByFolderOID(folderOID);
                    if (Folder == null) return dt;
                }
                else
                {
                    Folder = currentSubj.Instances.FindByFolderOID(folderOID);
                    if (Folder == null) return dt;
                }
                DataPage tempPage = Folder.DataPages.FindByFormOID(formOID);
                if (tempPage == null) return dt;
                DataPoint tempDP = tempPage.MasterRecord.DataPoints.FindByFieldOID(feildOID);
                if (tempDP == null) return dt;
                if (tempDP.Data != string.Empty)
                {
                    return tempDP;
                }
            }
            return dt;
}
}
}
