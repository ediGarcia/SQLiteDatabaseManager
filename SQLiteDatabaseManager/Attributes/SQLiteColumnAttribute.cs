using System;
using SQLiteDatabaseManager.Enums;

namespace SQLiteDatabaseManager.Attributes
{
	/// <summary>
	/// Source/destination table column info.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	// ReSharper disable once InconsistentNaming
	public class SQLiteColumnAttribute : SQLiteField
	{
		#region Properties

		/// <summary>
		/// Column alias.
		/// </summary>
		public string Alias
		{
			get => _alias;
			set => _alias = value?.ToUpper();
		}

		/// <summary>
		/// Default INSERT/UPDATE value used when the field value is null.
		/// </summary>
		public object DefaultValue { get; set; }

		/// <summary>
		/// Indicates whether the current column contains the PRIMARY KEY constraint.
		/// </summary>
		public bool IsPrimaryKey { get; set; }

		/// <summary>
		/// Gets and sets the maximum length for the current column. 0 or negative values indicate unlimited length.
		/// </summary>
		public int MaxLength { get; set; } = -1;

		/// <summary>
		/// Joined source table.
		/// </summary>
		public string TableAlias
		{
			get => _tableAlias;
			set => _tableAlias = value?.ToUpper();
		}

		/// <summary>
		/// Defines the current column behaviour for the INSERT and UPDATE methods.
		/// </summary>
		public UpdateBehaviour UpdateBehaviour { get; set; }

		#endregion

		private string _alias;
		private string _tableAlias;

		public SQLiteColumnAttribute(string name) : base(name) { }
	}
}
