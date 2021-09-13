namespace SQLiteDatabaseManager.Enums
{
	// ReSharper disable once InconsistentNaming
	public enum SQLiteFileMode
	{
		/// <summary>
		/// Creates the database file if it doesn't exist.
		/// </summary>
		Create,

		/// <summary>
		/// Creates the database file. If the file already exists, an <see cref="System.IO.IOException"/> exception is thrown.
		/// </summary>
		CreateNew,

		/// <summary>
		/// Creates the database file, overwrites any pre-existing files.
		/// </summary>
		Overwrite
	}
}
