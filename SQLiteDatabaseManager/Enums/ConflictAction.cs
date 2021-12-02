namespace SQLiteDatabaseManager.Enums
{
	/// <summary>
	/// Action taken when an UNIQUE constraint exception is thrown.
	/// </summary>
	public enum ConflictAction
	{
		ThrowError,
		Ignore,
		Replace
	}
}
