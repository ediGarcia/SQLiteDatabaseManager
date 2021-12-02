using SQLiteDatabaseManager.Exceptions;

namespace SQLiteDatabaseManager.Attributes
{
	// ReSharper disable once InconsistentNaming
	public class SQLiteManyToManyData : SQLiteField
	{
		#region Properties

		/// <summary>
		/// Relation table name.
		/// </summary>
		public string RelationTable { get; }

		/// <summary>
		/// Source foreign key from the relation table.
		/// </summary>
		public string SourceRelationColumn { get; }

		/// <summary>
		/// Target foreign key from the relation table.
		/// </summary>
		public string TargetRelationColumn { get; }

		#endregion

		public SQLiteManyToManyData(string relationTable, string sourceColumn, string sourceRelationColumn, string targetRelationColumn) : base(sourceColumn)
		{
			ExceptionHelper.CheckNull(relationTable, "The relation table name");
			ExceptionHelper.CheckNull(sourceRelationColumn, "The source relation column name");
			ExceptionHelper.CheckNull(targetRelationColumn, "The target relation column name");

			RelationTable = relationTable.ToUpper();
			SourceRelationColumn = sourceRelationColumn.ToUpper();
			TargetRelationColumn = targetRelationColumn.ToUpper();
		}
	}
}
