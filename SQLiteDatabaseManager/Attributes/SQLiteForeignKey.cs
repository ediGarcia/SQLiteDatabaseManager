using SQLiteDatabaseManager.Exceptions;
using System;

namespace SQLiteDatabaseManager.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	// ReSharper disable once InconsistentNaming
	public class SQLiteForeignKey : SQLiteField
	{

		#region Properties

		/// <summary>
		/// Indicates whether the current column contains the PRIMARY KEY constraint.
		/// </summary>
		public bool IsPrimaryKey { get; set; }

		/// <summary>
		/// Foreign key target column.
		/// </summary>
		public string TargetColumn { get; }

		#endregion

		public SQLiteForeignKey(string name, string targetColumn) : base(name)
		{
			ExceptionHelper.CheckNull(targetColumn, "The target column name");
			TargetColumn = targetColumn.ToUpper();
		}
	}
}
