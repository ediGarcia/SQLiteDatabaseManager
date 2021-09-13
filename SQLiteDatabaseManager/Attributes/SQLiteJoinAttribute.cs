using System;
using SQLiteDatabaseManager.Enums;
using SQLiteDatabaseManager.Exceptions;

namespace SQLiteDatabaseManager.Attributes
{
	/// <summary>
	/// JOIN clause data.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	// ReSharper disable once InconsistentNaming
	public class SQLiteJoinAttribute : Attribute
	{
		#region Properties

		/// <summary>
		/// Joined table alias.
		/// </summary>
		public string Alias { get; }

		/// <summary>
		/// The join constraints expression.
		/// </summary>
		public string Constraint { get; }

		/// <summary>
		/// Join mode.
		/// </summary>
		public JoinMode Mode { get; set; } = JoinMode.Inner;

		/// <summary>
		/// Target table name.
		/// </summary>
		public string Table { get; }

		#endregion

		public SQLiteJoinAttribute(string table, string alias, string constraint)
		{
			ExceptionHelper.CheckNull(table, "The table name");
			ExceptionHelper.CheckNull(alias, "The table alias");
			ExceptionHelper.CheckNull(constraint, "The join constraint");

			Table = table.ToUpper();
			Alias = alias.ToUpper();
			Constraint = constraint;
		}
	}
}
