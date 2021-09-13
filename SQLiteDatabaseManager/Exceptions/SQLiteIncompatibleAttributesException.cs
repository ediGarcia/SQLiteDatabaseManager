using System;

namespace SQLiteDatabaseManager.Exceptions
{
	// ReSharper disable once InconsistentNaming
	public class SQLiteIncompatibleAttributesException : Exception
	{
		public SQLiteIncompatibleAttributesException(string message) : base(message) { }
	}
}
