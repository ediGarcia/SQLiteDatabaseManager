using SQLiteDatabaseManager.Exceptions;
using System;

namespace SQLiteDatabaseManager.Attributes
{
	/// <summary>
	/// Custom field data for SELECT statements.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	// ReSharper disable once InconsistentNaming
	public class SQLiteCustomFieldAttribute : SQLiteField
	{
		#region Properties

		/// <summary>
		/// Custom field data.
		/// </summary>
		public string FieldData { get; }

		#endregion

		public SQLiteCustomFieldAttribute(string fieldName, string fieldData) : base(fieldName)
		{
			ExceptionHelper.CheckNull(fieldData, "The field data");
			FieldData = fieldData;
		}
	}
}