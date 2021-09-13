using System;

namespace SQLiteDatabaseManager.Exceptions
{
	public static class ExceptionHelper
	{
		#region CheckNull
		/// <summary>
		/// Checks if the specified field is null or empty and throw an exception if needed.
		/// </summary>
		/// <param name="fieldValue"></param>
		/// <param name="fieldName"></param>
		/// <param name="validateEmpty"></param>
		/// <param name="validateWhiteSpace"></param>
		public static void CheckNull(string fieldValue, string fieldName, bool validateEmpty = true, bool validateWhiteSpace = true)
		{
			if (fieldValue is null
				|| validateEmpty && fieldName == ""
				|| validateWhiteSpace && fieldName.Trim() == "")
				throw new ArgumentNullException(fieldName, $"{fieldName} cannot be null{(validateEmpty || validateWhiteSpace ? " or empty" : "")}.");
		}
		#endregion
	}
}
