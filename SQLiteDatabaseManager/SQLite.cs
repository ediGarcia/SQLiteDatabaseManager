using SQLiteDatabaseManager.Enums;
using System.Collections.Generic;
using System.Data;

namespace SQLiteDatabaseManager
{
	// ReSharper disable once InconsistentNaming
	public class SQLite
	{
		#region Properties

		/// <summary>
		/// Database file path.
		/// </summary>
		public string Path { get; }

		#endregion

		public SQLite(string path, SQLiteFileMode fileMode = SQLiteFileMode.Create)
		{
			Path = path;
			SQLiteHelper.CreateDatabase(path, fileMode);
		}

		#region Public methods

		#region Delete

		#region Delete(T)
		/// <summary>
		/// Removes the specified data from the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Data to be removed.</param>
		/// <returns></returns>
		public void Delete<T>(T data) =>
			SQLiteHelper.Delete(Path, data);
		#endregion

		#region Delete(List<T>)
		/// <summary>
		/// Removes the specified data from the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Data to be removed.</param>
		/// <returns></returns>
		public void Delete<T>(List<T> data) =>
			SQLiteHelper.Delete(Path, data);
		#endregion

		#endregion

		#region ExecuteNonQuery
		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="commandText"></param>
		/// <param name="parameters"></param>
		public void ExecuteNonQuery(string commandText, Dictionary<string, object> parameters) =>
			SQLiteHelper.ExecuteNonQuery(Path, commandText, parameters);
		#endregion

		#region ExecuteNonQueryAsync
		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="commandText"></param>
		/// <param name="parameters"></param>
		public void ExecuteNonQueryAsync(string commandText, Dictionary<string, object> parameters) =>
			SQLiteHelper.ExecuteNonQueryAsync(Path, commandText, parameters);
		#endregion

		#region ExecuteQuery
		/// <summary>
		/// Executes a sqlite command that returns values.
		/// </summary>
		/// <param name="commandText"></param>
		public DataTable ExecuteQuery(string commandText) =>
			SQLiteHelper.ExecuteQuery(Path, commandText);
		#endregion

		#region ExecuteQuery<T>
		/// <summary>
		/// Executes a sqlite command that returns values.
		/// </summary>
		/// <param name="commandText"></param>
		public List<T> ExecuteQuery<T>(string commandText) =>
			SQLiteHelper.ExecuteQuery<T>(Path, commandText);
		#endregion

		#region Exists
		/// <summary>
		/// Indicates whether the specified tuple exists in the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <param name="data">Object fields to filter the response.</param>
		/// <param name="checkOnlyPrimary">Indicates whether only primary keys should be considered.</param>
		/// <returns>SQL conditional expression to filter the results.</returns>
		public bool Exists<T>(T data, string sqlFilter = null, bool checkOnlyPrimary = true) =>
			SQLiteHelper.Exists(Path, data, sqlFilter, checkOnlyPrimary);
		#endregion

		#region Insert

		#region Insert(T, [bool])
		/// <summary>
		/// Inserts the specified data into the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Data to be inserted into the database.</param>
		/// <param name="updateLocalData">Indicates whether the local data should be updated after the INSERT. Useful to update AUTOINCREMENT columns and default values.</param>
		public void Insert<T>(T data, bool updateLocalData = true) =>
			SQLiteHelper.Insert(Path, data, updateLocalData);
		#endregion

		#region Insert(List<T>, [bool])
		/// <summary>
		/// Inserts the specified data into the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Data to be inserted into the database.</param>
		/// <param name="updateLocalData">Indicates whether the local data should be updated after the INSERT. Useful to update AUTOINCREMENT columns and default values.</param>
		public void Insert<T>(List<T> data, bool updateLocalData = true) =>
			SQLiteHelper.Insert(Path, data, updateLocalData);
		#endregion

		#endregion

		#region InsertOrUpdate

		#region InsertOrUpdate(T, [bool])
		/// <summary>
		/// Insert or updates the specified data in the database based on its primary keys.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Data to be inserted or updated in the database.</param>
		/// <param name="updateLocalData">Indicates whether the local data should be updated after the INSERT. Useful to update AUTOINCREMENT columns and default values.</param>
		public void InsertOrUpdate<T>(T data, bool updateLocalData = true) =>
			SQLiteHelper.InsertOrUpdate(Path, data, updateLocalData);
		#endregion

		#region InsertOrUpdate(List<T>, [bool])
		/// <summary>
		/// Insert or updates the specified data in the database based on its primary keys.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Data to be inserted or updated in the database.</param>
		/// <param name="updateLocalData">Indicates whether the local data should be updated after the INSERT. Useful to update AUTOINCREMENT columns and default values.</param>
		public void InsertOrUpdate<T>(List<T> data, bool updateLocalData = true) =>
			SQLiteHelper.InsertOrUpdate(Path, data, updateLocalData);
		#endregion

		#endregion

		#region Select
		/// <summary>
		/// Runs a SELECT statement into the specified database and returns the results.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <param name="limit">Maximum number of returned items from the database.</param>
		/// <param name="filter">Object fields to filter the response.</param>
		/// <returns>SQL conditional expression to filter the results.</returns>
		public List<T> Select<T>(T filter = default, string sqlFilter = null, int? limit = null) =>
			SQLiteHelper.Select(Path, filter, sqlFilter, limit);
		#endregion

		#region SelectSingle
		/// <summary>
		/// Runs a SELECT statement into the specified database and returns only the first result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="filter">Object fields to filter the response.</param>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <returns></returns>
		public T SelectSingle<T>(T filter = default, string sqlFilter = null) =>
			SQLiteHelper.SelectSingle(Path, filter, sqlFilter);
		#endregion

		#region Update

		#region Update(T)
		/// <summary>
		/// Updates the specified data in the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Data to be updated in the database.</param>
		public void Update<T>(T data) =>
			SQLiteHelper.Update(Path, data);
		#endregion

		#region Update(List<T>)
		/// <summary>
		/// Updates the specified data in the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Data to be updated in the database.</param>
		public void Update<T>(List<T> data) =>
			SQLiteHelper.Update(Path, data);
		#endregion

		#endregion

		#endregion
	}
}
