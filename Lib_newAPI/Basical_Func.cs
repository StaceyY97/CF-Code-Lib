
///Get non-log Datapoint
IDataPoint GetDP(string field_Oid, string form_Oid, string folder_Oid, ISubject subj) => subj.Instances.FindByFolderOid(folder_Oid)?.DataPages.FindByFormOid(form_Oid)?.MasterRecord.AllDataPoints.FindByFieldOid(field_Oid);
///Get log Datapoint, can apply to non-log, if the target Datapoint is non-log, there would be only one Datepoint if the folder OID is not null
IReadOnlyList<IDataPoint> dps_term = subject.GetDataPointsByOidPath(oid_term, oid_form_tartget, oid_folder_tartget==""?null:oid_folder_tartget);


bool DoCheck(IDataPoint dp, bool IsDate, bool checkEmpty = true) => IsDate ? IsValidDp(dp, checkEmpty) && IsValidDate(dp) : IsValidDp(dp, checkEmpty);
bool IsValidDp(IDataPoint dp, bool checkEmpty) => dp != null && dp.Active && dp.IsVisibleRaw && (checkEmpty ? dp.Data != string.Empty : true);
bool IsValidDate(IDataPoint dp) => dp != null && dp.Active && dp.IsVisibleRaw && (dp.StandardValue is DateTime || dp.StandardValue is TimeSpan);

IReadOnlyList<IDataPoint> dps_query = dpAction.Field.OID == "EXPERM" ? new List<IDataPoint>() { dpAction } : SUBJ.GetDataPointsByOidPath("EXPERM", "EX", "EX");
