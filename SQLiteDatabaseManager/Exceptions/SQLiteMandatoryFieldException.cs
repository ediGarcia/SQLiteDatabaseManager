using System;

namespace SQLiteDatabaseManager.Exceptions
{
	// ReSharper disable once InconsistentNaming
	public class SQLiteMandatoryFieldException : Exception
	{
		public SQLiteMandatoryFieldException(string message) : base(message) { }
	}
}
