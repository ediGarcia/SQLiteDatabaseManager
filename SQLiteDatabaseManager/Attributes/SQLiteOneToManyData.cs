using SQLiteDatabaseManager.Exceptions;
using System;

namespace SQLiteDatabaseManager.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	// ReSharper disable once InconsistentNaming
	public class SQLiteOneToManyData : SQLiteField
	{
		#region Properties

		/// <summary>
		/// Target column name.
		/// </summary>
		public string TargetColumn { get; }

		#endregion

		public SQLiteOneToManyData(string name, string targetColumn) : base(name)
		{
			ExceptionHelper.CheckNull(targetColumn, "The target column name");
			TargetColumn = targetColumn.ToUpper();
		}
	}
}
