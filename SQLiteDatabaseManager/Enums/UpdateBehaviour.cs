namespace SQLiteDatabaseManager.Enums
{
	public enum UpdateBehaviour
	{
		/// <summary>
		/// Allows the insertion of null values.
		/// </summary>
		AllowNull,

		/// <summary>
		/// Ignores property (value will not be sent to the database).
		/// </summary>
		Ignore,

		/// <summary>
		/// Ignores property when null (value will not be sent to the database).
		/// </summary>
		IgnoreNull,

		/// <summary>
		/// Prevents null value.
		/// </summary>
		Mandatory
	}
}
