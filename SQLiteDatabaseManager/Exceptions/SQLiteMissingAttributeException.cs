using System;
// ReSharper disable InconsistentNaming

namespace SQLiteDatabaseManager.Exceptions
{
	public class SQLiteMissingAttributeException : Exception
	{
		public SQLiteMissingAttributeException(string message) : base(message) { }
	}
}
