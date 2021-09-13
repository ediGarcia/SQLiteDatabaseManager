using SQLiteDatabaseManager.Exceptions;
using System;

namespace SQLiteDatabaseManager.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	// ReSharper disable once InconsistentNaming
	public abstract class SQLiteField : Attribute
	{
		#region Properties

		/// <summary>
		/// Field name.
		/// </summary>
		public string Name { get; }

		#endregion

		protected SQLiteField(string name)
		{
			ExceptionHelper.CheckNull(name, "The column name");
			Name = name.ToUpper();
		}
	}
}
