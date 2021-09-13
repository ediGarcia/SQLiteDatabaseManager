using System;

namespace SQLiteDatabaseManager.Exceptions
{
	// ReSharper disable once InconsistentNaming
	public class SQLiteMaximumLengthExceededException : Exception
	{
		public SQLiteMaximumLengthExceededException(string message) : base(message) { }
	}
}
