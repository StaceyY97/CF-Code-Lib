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
using Medidata.Core.Objects;
using Medidata.Core.Common;
using Medidata.Core.Common.Utilities;
using Medidata.Interfaces;
using Medidata.Interfaces.Clinical;
using Medidata.Interfaces.Refs;
using Medidata.Utilities;
using Medidata.Utilities.Interfaces;
using Medidata.RaveWebServices.Outbound;

namespace CustomFunctions
{
	#region System Class - Do Not Modify
	using Medidata.CustomFunctions.Debug;

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
			if(!CacheManager.IsThreadAbortException(exception)) System.Windows.Forms.MessageBox.Show(exception.ToString());
		}
	}
	#endregion

	/// <summary>
	/// Sample CustomFunction.
	/// </summary>
	public class CustomFunction1 : CustomFunctionBase
	{
		public object Eval(object ThisObject)
		{
			return 0;
		}
	}
}
