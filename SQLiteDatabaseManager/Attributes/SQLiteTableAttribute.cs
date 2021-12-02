using SQLiteDatabaseManager.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SQLiteDatabaseManager.Attributes
{
	/// <summary>
	/// SQLite source table data.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	// ReSharper disable once InconsistentNaming
	public class SQLiteTableAttribute : Attribute
	{
		#region Properties

		/// <summary>
		/// Table alias for joined SELECT commands.
		/// </summary>
		public string Alias
		{
			get => _alias;
			set => _alias = value?.ToUpper();
		}

		/// <summary>
		/// Table name.
		/// </summary>
		public string Table { get; }

		/// <summary>
		/// Where clause to filter results in SELECT statements.
		/// </summary>
		public string Where { get; set; }

		#endregion

		private string _alias;

		public SQLiteTableAttribute([NotNull]string table)
		{
			ExceptionHelper.CheckNull(table, "The table name");
			Table = table.ToUpper();
		}
	}
}
