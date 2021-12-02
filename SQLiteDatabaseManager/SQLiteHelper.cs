using HelperExtensions;
using SQLiteDatabaseManager.Attributes;
using SQLiteDatabaseManager.Enums;
using SQLiteDatabaseManager.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SQLiteDatabaseManager
{
	// ReSharper disable once InconsistentNaming
	public static class SQLiteHelper
	{
		private static readonly string WhereClauseStarter = " WHERE 1 = 1";

		#region Public methods

		#region CreateDatabase

		/// <summary>
		/// Creates a database file.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="fileMode"></param>
		public static void CreateDatabase(string path, SQLiteFileMode fileMode)
		{
			if (File.Exists(path))
				switch (fileMode)
				{
					case SQLiteFileMode.CreateNew:
						throw new IOException($"The file \"{path}\" already exists.");

					case SQLiteFileMode.Overwrite:
						File.Delete(path);
						SQLiteConnection.CreateFile(path);
						break;
				}

			else
				SQLiteConnection.CreateFile(path);
		}
		#endregion

		#region Delete

		#region Delete(SQLiteConnection, T)
		/// <summary>
		/// Removes the specified data from the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An open SQLite connection.</param>
		/// <param name="data">Data to be removed.</param>
		/// <returns></returns>
		public static void Delete<T>(SQLiteConnection connection, T data)
		{
			Type targetType = typeof(T);
			SQLiteTableAttribute tableAttribute = GetTableAttribute(targetType);

			StringBuilder command = new("DELETE FROM ");
			command.Append(tableAttribute.Table).AppendIfNotNullOrWhiteSpace(tableAttribute.Alias, " AS ")
				.Append(WhereClauseStarter).AppendIfNotNullOrWhiteSpace(tableAttribute.Where, " AND (", ")");

			int propertyCount = 0;

			#region Get properties' data (columns and foreign keys).
			targetType.GetProperties().ForEach(propInfo =>
			{
				SQLiteField field = GetSQLiteFieldAttribute(propInfo);

				if (field is SQLiteColumnAttribute column && (column.Alias.IsNullOrWhiteSpace() || column.TableAlias == tableAttribute.Alias))
				{
					if (data is not null && propInfo.GetValue(data) is { } propValue)
						command.Append(" AND ").AppendIfNotNullOrWhiteSpace(tableAttribute.Alias, suffix: ".")
							.Append(column.Name, " = '", propValue, "'");

					propertyCount++;
				}

				else if (field is SQLiteForeignKey foreignKey)
				{
					object propValue = propInfo.GetValue(data);

					if (propValue is not null)
					{
						object innerValue = propInfo.PropertyType.GetProperties()
							.FirstOrDefault(_ =>
								_.GetCustomAttribute(typeof(SQLiteColumnAttribute)) is SQLiteColumnAttribute targetColumn
								&& targetColumn.Name == foreignKey.TargetColumn).GetValue(propValue);

						if (innerValue is not null)
							command.Append(" AND ").AppendIfNotNullOrWhiteSpace(tableAttribute.Alias, suffix: ".")
								.Append(foreignKey.Name, " = '", innerValue, "'");

						propertyCount++;
					}
				}
			});
			#endregion

			if (propertyCount == 0)
				return;
			
			ExecuteNonQuery(connection, command.ToString());
		}
		#endregion

		#region Delete(SQLiteConnection, List<T>)
		/// <summary>
		/// Removes the specified data from the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An open SQLite connection.</param>
		/// <param name="data">Data to be removed.</param>
		/// <returns></returns>
		public static void Delete<T>(SQLiteConnection connection, List<T> data) =>
			data.ForEach(_ => Delete(connection, _));
		#endregion

		#region Delete(string, T)
		/// <summary>
		/// Removes the specified data from the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path.</param>
		/// <param name="data">Data to be removed.</param>
		/// <returns></returns>
		public static void Delete<T>(string path, T data) =>
			Delete(OpenConnection(path), data);
		#endregion

		#region Delete(string, List<T>)
		/// <summary>
		/// Removes the specified data from the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path.</param>
		/// <param name="data">Data to be removed.</param>
		/// <returns></returns>
		public static void Delete<T>(string path, List<T> data) =>
			Delete(OpenConnection(path), data);
		#endregion

		#endregion

		#region ExecuteNonQuery

		#region ExecuteNonQuery(DbCommand)
		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="command"></param>
		public static void ExecuteNonQuery(DbCommand command)
		{
			command.ExecuteNonQuery();
			command.Dispose();
		}
		#endregion

		#region ExecuteNonQuery(SQLiteConnection, string)
		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="commandText"></param>
		public static void ExecuteNonQuery(SQLiteConnection connection, string commandText)
		{
			using DbCommand command = connection.CreateCommand();
			command.CommandText = commandText;
			command.ExecuteNonQuery();
		}
		#endregion

		#region ExecuteNonQuery(string, string)
		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="commandText"></param>
		public static void ExecuteNonQuery(string path, string commandText)
		{
			using DbCommand command = OpenConnection(path).CreateCommand();
			command.CommandText = commandText;
			command.ExecuteNonQuery();
		}
		#endregion

		#region ExecuteNonQuery(string, string, Dictionary<string, object>)

		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="commandText"></param>
		/// <param name="parameters"></param>
		public static void ExecuteNonQuery(string path, string commandText, Dictionary<string, object> parameters)
		{
			SQLiteConnection connection = OpenConnection(path);
			SQLiteCommand command = connection.CreateCommand();

			command.CommandText = commandText;
			// ReSharper disable once AccessToDisposedClosure
			parameters.ForEach(_ => command.Parameters.AddWithValue(_.Key, _.Value));

			command.ExecuteNonQuery();
			command.Dispose();
		}
		#endregion

		#endregion

		#region ExecuteNonQueryAsync

		#region ExecuteNonQueryAsync(DbCommand)
		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="command"></param>
		public static async void ExecuteNonQueryAsync(DbCommand command)
		{
			await command.ExecuteNonQueryAsync();
			command.Dispose();
		}
		#endregion

		#region ExecuteNonQueryAsync(SQLiteConnection, string)
		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="commandText"></param>
		public static async void ExecuteNonQueryAsync(SQLiteConnection connection, string commandText)
		{
			await using DbCommand command = connection.CreateCommand();
			command.CommandText = commandText;
			await command.ExecuteNonQueryAsync();
		}
		#endregion

		#region ExecuteNonQueryAsync(string, string)
		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="commandText"></param>
		public static async void ExecuteNonQueryAsync(string path, string commandText)
		{
			await using DbCommand command = OpenConnection(path).CreateCommand();
			command.CommandText = commandText;
			await command.ExecuteNonQueryAsync();
			command.Dispose();
		}
		#endregion

		#region ExecuteNonQuery(string, string, Dictionary<string, object>)

		/// <summary>
		/// Executes a non-query sqlite command.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="commandText"></param>
		/// <param name="parameters"></param>
		public static void ExecuteNonQueryAsync(string path, string commandText, Dictionary<string, object> parameters)
		{
			SQLiteConnection connection = OpenConnection(path);
			SQLiteCommand command = connection.CreateCommand();

			command.CommandText = commandText;
			// ReSharper disable once AccessToDisposedClosure
			parameters.ForEach(_ => command.Parameters.AddWithValue(_.Key, _.Value));

			command.ExecuteNonQueryAsync();
			command.Dispose();
		}
		#endregion

		#endregion

		#region ExecuteQuery

		#region ExecuteQuery(DbCommand)
		/// <summary>
		/// Executes a sqlite command that returns values.
		/// </summary>
		/// <param name="command"></param>
		public static DataTable ExecuteQuery(DbCommand command)
		{
			DataTable result = new();
			new SQLiteDataAdapter(command.CommandText, (SQLiteConnection)command.Connection).Fill(result);
			return result;
		}
		#endregion

		#region ExecuteQuery<T>(DbCommand)
		/// <summary>
		/// Executes a sqlite command that returns values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="command"></param>
		/// <returns></returns>
		public static List<T> ExecuteQuery<T>(DbCommand command)
		{
			DataRowCollection rows = ExecuteQuery(command).Rows;
			List<T> models = new(rows.Count);

			models.AddRange(from DataRow dataRow in rows select FillModel<T>(dataRow));
			return models;
		}
		#endregion

		#region ExecuteQuery(SQLiteConnection, string)
		/// <summary>
		/// Executes a sqlite command that returns values.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="commandText"></param>
		public static DataTable ExecuteQuery(SQLiteConnection connection, string commandText)
		{
			DbCommand command = connection.CreateCommand();
			command.CommandText = commandText;

			DataTable result = new();
			new SQLiteDataAdapter(command.CommandText, connection).Fill(result);

			return result;
		}
		#endregion

		#region ExecuteQuery<T>(SQLiteConnection, string)
		/// <summary>
		/// Executes a sqlite command that returns values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection"></param>
		/// <param name="commandText"></param>
		/// <returns></returns>
		public static List<T> ExecuteQuery<T>(SQLiteConnection connection, string commandText)
		{
			DataRowCollection rows = ExecuteQuery(connection, commandText).Rows;
			List<T> models = new(rows.Count);

			models.AddRange(from DataRow dataRow in rows select FillModel<T>(dataRow));
			return models;
		}
		#endregion

		#region ExecuteQuery(string, string)
		/// <summary>
		/// Executes a sqlite command that returns values.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="commandText"></param>
		public static DataTable ExecuteQuery(string path, string commandText) =>
			ExecuteQuery(OpenConnection(path), commandText);
		#endregion

		#region ExecuteQuery<T>(string, string)
		/// <summary>
		/// Executes a sqlite command that returns values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="commandText"></param>
		/// <returns></returns>
		public static List<T> ExecuteQuery<T>(string path, string commandText) =>
			ExecuteQuery<T>(OpenConnection(path), commandText);
		#endregion

		#endregion

		#region Exists

		#region Exists(SQLiteConnection, T, [string], [bool])
		/// <summary>
		/// Indicates whether the specified tuple exists in the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An open SQLite connection.</param>
		/// <param name="data">Data to search.</param>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <param name="checkOnlyPrimary">Indicates whether only primary keys should be considered.</param>
		/// <returns></returns>
		public static bool Exists<T>(SQLiteConnection connection, T data, string sqlFilter = null, bool checkOnlyPrimary = true)
		{
			if (data is null)
				throw new ArgumentNullException(nameof(data), "The search data cannot be null.");

			Type targetType = typeof(T);
			SQLiteTableAttribute tableAttribute = GetTableAttribute(targetType);

			StringBuilder command = new("SELECT COUNT(*) FROM ");
			command.Append(tableAttribute.Table).AppendIfNotNullOrWhiteSpace(tableAttribute.Alias, " AS ")
				.Append(WhereClauseStarter).AppendIfNotNullOrWhiteSpace(tableAttribute.Where, " AND (", ")")
				.AppendIfNotNullOrWhiteSpace(sqlFilter, " AND (", ")");

			int propertyCount = 0;

			#region Get properties' data (columns and foreign keys).
			targetType.GetProperties().ForEach(propInfo =>
			{
				SQLiteField field = GetSQLiteFieldAttribute(propInfo);

				if (field is SQLiteColumnAttribute column && (!checkOnlyPrimary || column.IsPrimaryKey))
				{
					if (propInfo.GetValue(data) is { } propValue)
						command.Append(" AND ").AppendIfNotNullOrWhiteSpace(column.TableAlias ?? tableAttribute.Alias, suffix: ".")
							.Append(column.Name, " = '", propValue, "'");

					propertyCount++;
				}

				else if (field is SQLiteForeignKey foreignKey && (!checkOnlyPrimary || foreignKey.IsPrimaryKey))
				{
					object propValue = propInfo.GetValue(data);

					if (propValue is not null)
					{
						object innerValue = propInfo.PropertyType.GetProperties()
							.FirstOrDefault(_ =>
								_.GetCustomAttribute(typeof(SQLiteColumnAttribute)) is SQLiteColumnAttribute targetColumn
								&& targetColumn.Name == foreignKey.TargetColumn).GetValue(propValue);

						if (innerValue is not null)
							command.Append(" AND ").AppendIfNotNullOrWhiteSpace(tableAttribute.Alias, suffix: ".")
								.Append(foreignKey.Name, " = '", innerValue, "'");

						propertyCount++;
					}
				}
			});
			#endregion

			if (checkOnlyPrimary && propertyCount == 0)
				return false;

			if (!checkOnlyPrimary)
				GenerateJoin<T>(command);

			return (long)ExecuteQuery(connection, command.ToString()).Rows[0][0] > 0;
		}
		#endregion

		#region Exists(string, T, [string], [bool?])
		/// <summary>
		/// Indicates whether the specified tuple exists in the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path.</param>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <param name="data">Data to search.</param>
		/// <param name="checkOnlyPrimary">Indicates whether only primary keys should be considered.</param>
		/// <returns>SQL conditional expression to filter the results.</returns>
		public static bool Exists<T>(string path, T data = default, string sqlFilter = null, bool checkOnlyPrimary = true) =>
			Exists(OpenConnection(path), data, sqlFilter, checkOnlyPrimary);
		#endregion

		#endregion

		#region FillModel

		#region FillModel([T], [DataRow])
		/// <summary>
		/// Fills the model with the data from the <see cref="DataRow"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <param name="dataTable"></param>
		public static void FillModel<T>(T model, DataRow dataTable)
		{
			Type targetType = typeof(T);

			targetType.GetProperties().ForEach(prop =>
			{
				if (prop.GetCustomAttribute(typeof(SQLiteField)) is SQLiteField fieldData
					&& dataTable.Table.Columns.Contains(fieldData.Name))
					targetType.GetProperty(prop.Name)?.SetValue(model, dataTable[fieldData.Name]);
			});
		}
		#endregion

		#region FillModel([DataRow])
		/// <summary>
		/// Returns a specified type model with the data from the <see cref="DataRow"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataTable"></param>
		public static T FillModel<T>(DataRow dataTable)
		{
			T model = Activator.CreateInstance<T>();
			FillModel(model, dataTable);

			return model;
		}
		#endregion

		#endregion

		#region GenerateConnectionString
		/// <summary>
		/// Generates the connection string
		/// </summary>
		/// <param name="path"></param>
		/// <param name="versionNumber"></param>
		/// <returns></returns>
		public static string GenerateConnectionString(string path, int versionNumber = 3) =>
			$"Data Source={path}; Version={versionNumber};";
		#endregion

		#region Insert

		#region Insert(SQLiteConnection, T, [bool], [ConflictAction])
		/// <summary>
		/// Inserts the specified data into the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An open SQLite connection.</param>
		/// <param name="data">Data to be inserted into the database.</param>
		/// <param name="updateLocalData">Indicates whether the local data should be updated after the INSERT. Useful to update AUTOINCREMENT columns and default values.</param>
		/// <param name="conflictAction">Sets the action taken when a UNIQUE constraint error is thrown.</param>
		public static void Insert<T>(SQLiteConnection connection, T data, bool updateLocalData = true, ConflictAction conflictAction = ConflictAction.ThrowError)
		{
			if (data is null)
				throw new ArgumentNullException(nameof(data), "The target data cannot be null.");

			Type targetType = typeof(T);
			PropertyInfo[] targetTypeProperties = targetType.GetProperties();
			SQLiteCommand command = connection.CreateCommand();
			StringBuilder commandText = new("INSERT ");
			StringBuilder valuesList = new("VALUES (");

			if (conflictAction == ConflictAction.Ignore)
				commandText.Append(" OR IGNORE ");
			else if (conflictAction == ConflictAction.Replace)
				commandText.Append(" OR REPLACE ");

			commandText.Append("INTO ", GetTableAttribute(targetType).Table, " (");

			#region Get properties' data.
			targetTypeProperties.ForEach(propInfo =>
			{
				SQLiteField field = GetSQLiteFieldAttribute(propInfo);
				string parameterName = $"@{propInfo.Name}";

				if (field is SQLiteColumnAttribute column)
				{
					object propValue = propInfo.GetValue(data);

					CheckUpdateProperty(column, propValue);

					if (propValue is null && column.DefaultValue is null || column.UpdateBehaviour is UpdateBehaviour.Ignore)
						return;

					commandText.Append(" ", column.Name, ",");
					AddUpdateParameter(valuesList, command, parameterName, propValue ?? column.DefaultValue);
				}

				else if (field is SQLiteForeignKey foreignKey)
				{
					object propValue = propInfo.GetValue(data);

					if (propValue is null)
						return;

					commandText.Append(" ", foreignKey.Name, ",");
					AddUpdateParameter(valuesList, command, parameterName, propInfo.PropertyType.GetProperties()
						.FirstOrDefault(_ =>
							_.GetCustomAttribute(typeof(SQLiteColumnAttribute)) is SQLiteColumnAttribute targetColumn
							&& targetColumn.Name == foreignKey.TargetColumn).GetValue(propValue));
				}
			});
			#endregion

			if (command.Parameters.Count == 0)
				return;

			commandText.Length -= 1;
			commandText.Append(" ) ");

			valuesList.Length -= 1;
			valuesList.Append(" )");

			commandText.Append(valuesList.ToString());
			command.CommandText = commandText.ToString();

			ExecuteNonQuery(command); //Execute the INSERT command.

			// Updates the local data.
			if (updateLocalData)
			{
				T updatedData = SelectSingle<T>(connection, sqlFilter: $"rowId = '{connection.LastInsertRowId}'");
				targetTypeProperties.ForEach(propInfo => propInfo.SetValue(data, propInfo.GetValue(updatedData)));
			}
		}
		#endregion

		#region Insert(SQLiteConnection, List<T>, [bool], [ConflictAction])
		/// <summary>
		/// Inserts the specified data into the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An open SQLite connection.</param>
		/// <param name="data">Data to be inserted into the database.</param>
		/// <param name="updateLocalData">Indicates whether the local data should be updated after the INSERT. Useful to update AUTOINCREMENT columns and default values.</param>
		/// <param name="conflictAction">Sets the action taken when a UNIQUE constraint error is thrown.</param>
		public static void Insert<T>(SQLiteConnection connection, List<T> data, bool updateLocalData = true, ConflictAction conflictAction = ConflictAction.ThrowError) =>
			data?.ForEach(_ => Insert(connection, _, updateLocalData, conflictAction));
		#endregion

		#region Insert(string, T, [bool], [ConflictAction])
		/// <summary>
		/// Inserts the specified data into the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path.</param>
		/// <param name="data">Data to be inserted into the database.</param>
		/// <param name="updateLocalData">Indicates whether the local data should be updated after the INSERT. Useful to update AUTOINCREMENT columns and default values.</param>
		/// <param name="conflictAction">Sets the action taken when a UNIQUE constraint error is thrown.</param>
		public static void Insert<T>(string path, T data, bool updateLocalData = true, ConflictAction conflictAction = ConflictAction.ThrowError) =>
			Insert(OpenConnection(path), data, updateLocalData, conflictAction);
		#endregion

		#region Insert(string, List<T>, [bool], [ConflictAction])
		/// <summary>
		/// Inserts the specified data into the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path.</param>
		/// <param name="data">Data to be inserted into the database.</param>
		/// <param name="updateLocalData">Indicates whether the local data should be updated after the INSERT. Useful to update AUTOINCREMENT columns and default values.</param>
		/// <param name="conflictAction">Sets the action taken when a UNIQUE constraint error is thrown.</param>
		public static void Insert<T>(string path, List<T> data, bool updateLocalData = true, ConflictAction conflictAction = ConflictAction.ThrowError)
		{
			SQLiteConnection connection = OpenConnection(path);
			data?.ForEach(_ => Insert(connection, _, updateLocalData, conflictAction));
		}
		#endregion

		#endregion

		#region OpenConnection
		/// <summary>
		/// Opens a connection with a sqlite database file.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="versionNumber"></param>
		/// <returns></returns>
		public static SQLiteConnection OpenConnection(string path, int versionNumber = 3) =>
			new SQLiteConnection(GenerateConnectionString(path, versionNumber)).OpenAndReturn();
		#endregion

		#region Select

		#region Select(SQLiteConnection, [T], [string], [int?])
		/// <summary>
		/// Runs a SELECT statement into the specified database and returns the results.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An open SQLite connection.</param>
		/// <param name="filter">Object fields to filter the response.</param>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <param name="limit">Maximum number of returned items from the database.</param>
		/// <returns></returns>
		public static List<T> Select<T>(SQLiteConnection connection, T filter = default, string sqlFilter = null, int? limit = null)
		{
			Type targetType = typeof(T);
			SQLiteTableAttribute tableAttribute = GetTableAttribute(targetType);
			Dictionary<string, string> columns = new();
			Dictionary<string, SQLiteForeignKey> foreignKeys = new();
			Dictionary<string, SQLiteOneToManyData> oneToManyKeys = new();
			Dictionary<string, SQLiteManyToManyData> manyToManyKeys = new();
			StringBuilder commandText = new("SELECT");

			#region Get the properties' data (columns, foreign keys, one-to-many and many-to-many keys).
			targetType.GetProperties().ForEach(propInfo =>
			{
				SQLiteField field = GetSQLiteFieldAttribute(propInfo);

				switch (field)
				{
					case SQLiteColumnAttribute column:
						AddSelectColumn(commandText, column.TableAlias ?? tableAttribute.Alias, column.Name, column.Alias);
						AddSelectColumnDictionary(columns, column.Name, propInfo.Name);
						break;

					case SQLiteCustomFieldAttribute customField:
						commandText.Append(" ", customField.FieldData, " AS ", customField.Name, ",");
						columns.Add(customField.Name, propInfo.Name);
						break;

					case SQLiteForeignKey foreignKey:
						if (!columns.ContainsKey(foreignKey.Name))
						{
							AddSelectColumn(commandText, tableAttribute.Alias, foreignKey.Name, null);
							AddSelectColumnDictionary(columns, foreignKey.Name, null);
						}

						foreignKeys.Add(propInfo.Name, foreignKey);
						break;

					case SQLiteOneToManyData oneToMany:
						oneToManyKeys.Add(propInfo.Name, oneToMany);
						break;

					case SQLiteManyToManyData manyToMany:
						manyToManyKeys.Add(propInfo.Name, manyToMany);
						break;
				}
			});
			#endregion

			// If no properties contains the necessary attributes, then there is nothing to return.
			if (columns.Count == 0)
				return new List<T>();

			#region Gets data for the command text (FROM, WHERE, JOIN and LIMIT).

			// Source table (FROM clause).
			commandText.Length -= 1;
			commandText.Append(" FROM ", tableAttribute.Table).AppendIfNotNullOrWhiteSpace(tableAttribute.Alias, " AS ");

			GenerateJoin<T>(commandText); // JOIN clause.

			// Conditions (WHERE clause).
			commandText.Append(WhereClauseStarter).AppendIfNotNullOrWhiteSpace(tableAttribute.Where, " AND (", ")")
				.AppendIfNotNullOrWhiteSpace(sqlFilter, " AND (", ")");

			if (filter is not null)
				typeof(T).GetProperties().ToList().ForEach(propInfo =>
				{
					if (GetSQLiteFieldAttribute(propInfo) is SQLiteColumnAttribute column &&
						propInfo.GetValue(filter) is { } propValue)
						commandText.Append(" AND ").AppendIfNotNullOrWhiteSpace(tableAttribute.Alias, suffix: ".")
							.Append(column.Name, " = '", propValue, "'");
				});

			// LIMIT.
			if (limit is not null)
				commandText.Append(" LIMIT ", limit);

			#endregion

			#region Run queries.
			// Run main query.
			DataTable queryResult = ExecuteQuery(connection, commandText.ToString());

			// Makes sure that every column name is upper case.
			foreach (DataColumn column in queryResult.Columns)
				column.ColumnName = column.ColumnName.ToUpper();

			List<T> results = ConvertDataTable<T>(queryResult, columns);

			for (int i = 0; i < results.Count; i++)
			{
				T result = results[i];

				#region Get foreign keys data from the database.
				foreach (string propertyName in foreignKeys.Keys)
				{
					PropertyInfo propInfo = targetType.GetProperty(propertyName);
					SQLiteForeignKey foreignKey = foreignKeys[propertyName];

					propInfo.SetValue(result,
						typeof(SQLiteHelper).GetMethods().First(_ => _.Name == nameof(SelectSingle) && _.GetParameters()[0].ParameterType == typeof(SQLiteConnection))
							.MakeGenericMethod(propInfo.PropertyType).Invoke(null,
								new object[] { connection, null, $"{foreignKey.TargetColumn} = '{queryResult.Rows[i][foreignKey.Name]}'" }));
				}
				#endregion

				#region Gets the one-to-many-key's data from the database.
				foreach (string propertyName in oneToManyKeys.Keys)
				{
					PropertyInfo propInfo = targetType.GetProperty(propertyName);
					Type propType = propInfo.PropertyType.GetGenericArguments() is { Length: > 0 } listTypes ? listTypes[0] : propInfo.PropertyType;
					SQLiteOneToManyData oneToManyData = oneToManyKeys[propertyName];
					string targetColumn = oneToManyData.TargetColumn;

					// Gets the target table alias.
					if (Attribute.GetCustomAttribute(propType, typeof(SQLiteTableAttribute)) is SQLiteTableAttribute innerListTableAttribute
						&& !innerListTableAttribute.Alias.IsNullOrWhiteSpace())
						targetColumn = $"{innerListTableAttribute.Alias}.{targetColumn}";

					propInfo.SetValue(result,
						typeof(SQLiteHelper)
							.GetMethods()
							.First(_ => _.Name == nameof(Select) && _.GetParameters()[0].ParameterType == typeof(SQLiteConnection))
							.MakeGenericMethod(propType)
							.Invoke(null,
								new object[] { connection, null, $"{targetColumn} = '{queryResult.Rows[i][oneToManyData.Name]}'", null }));
				}
				#endregion

				#region Gets the many-to-many keys from the database.
				foreach (string propertyName in manyToManyKeys.Keys)
				{
					PropertyInfo propInfo = targetType.GetProperty(propertyName);
					Type propType = propInfo.PropertyType.GetGenericArguments() is { Length: > 0 } listTypes ? listTypes[0] : propInfo.PropertyType;
					SQLiteManyToManyData manyToManyData = manyToManyKeys[propertyName];
					string manyToManyFilter =
						$"{manyToManyData.TargetRelationColumn} in (SELECT {manyToManyData.TargetRelationColumn} FROM {manyToManyData.RelationTable} WHERE {manyToManyData.SourceRelationColumn} = {queryResult.Rows[i][manyToManyData.SourceRelationColumn]})";
					
					propInfo.SetValue(result,
						typeof(SQLiteHelper)
							.GetMethods()
							.First(_ => _.Name == nameof(Select) &&
							            _.GetParameters()[0].ParameterType == typeof(SQLiteConnection))
							.MakeGenericMethod(propType)
							.Invoke(null,
								new object[] { connection, null, manyToManyFilter, null }));
				} 
				#endregion
			}
			#endregion

			return results;
		}
		#endregion

		#region Select(string, [T], [string], [int?])
		/// <summary>
		/// Runs a SELECT statement into the specified database and returns the results.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path.</param>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <param name="limit">Maximum number of returned items from the database.</param>
		/// <param name="filter">Object fields to filter the response.</param>
		/// <returns>SQL conditional expression to filter the results.</returns>
		public static List<T> Select<T>(string path, T filter = default, string sqlFilter = null, int? limit = null) =>
			Select(OpenConnection(path), filter, sqlFilter, limit);
		#endregion

		#endregion

		#region SelectSingle

		#region SelectSingle(SQLiteConnection, [T], [string])
		/// <summary>
		/// Runs a SELECT statement into the specified database and returns only the first result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An opened SQLite connection.</param>
		/// <param name="filter">Object fields to filter the response.</param>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <returns></returns>
		public static T SelectSingle<T>(SQLiteConnection connection, T filter = default, string sqlFilter = null) =>
			Select(connection, filter, sqlFilter, 1).FirstOrDefault();
		#endregion

		#region SelectSingle(string, [T], [string])
		/// <summary>
		/// Runs a SELECT statement into the specified database and returns only the first result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path</param>
		/// <param name="filter">Object fields to filter the response.</param>
		/// <param name="sqlFilter">SQL conditional expression to filter the results.</param>
		/// <returns></returns>
		public static T SelectSingle<T>(string path, T filter = default, string sqlFilter = null) =>
			Select(path, filter, sqlFilter, 1).FirstOrDefault();
		#endregion

		#endregion

		#region Update

		#region Update(SQLiteConnection, T)
		/// <summary>
		/// Updates the specified data in the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An open SQLite connection.</param>
		/// <param name="data">Data to be updated in the database.</param>
		public static void Update<T>(SQLiteConnection connection, T data)
		{
			if (data is null)
				throw new ArgumentNullException(nameof(data), "The UPDATE data cannot be null.");

			Type targetType = typeof(T);
			SQLiteTableAttribute tableAttribute = GetTableAttribute(targetType);
			PropertyInfo[] targetTypeProperties = targetType.GetProperties();
			SQLiteCommand command = connection.CreateCommand();
			StringBuilder commandText = new("UPDATE");
			StringBuilder conditions = new(WhereClauseStarter);

			commandText.Append(" ", tableAttribute.Table, " SET");

			#region Get properties' data.
			targetTypeProperties.ForEach(propInfo =>
			{
				SQLiteField field = GetSQLiteFieldAttribute(propInfo);
				string parameterName = $"@{propInfo.Name}";

				if (field is SQLiteColumnAttribute column && (column.TableAlias.IsNullOrWhiteSpace() || column.TableAlias == tableAttribute.Alias))
				{
					object propValue = propInfo.GetValue(data);

					CheckUpdateProperty(column, propValue);

					if (column.UpdateBehaviour is UpdateBehaviour.Ignore || propValue is null && column.UpdateBehaviour is UpdateBehaviour.IgnoreNull)
						return;

					if (column.IsPrimaryKey)
						conditions.Append(" AND ", column.Name, " = ", parameterName);
					else
						commandText.Append(" ", column.Name, " = ", parameterName, ",");

					command.Parameters.AddWithValue(parameterName, propValue ?? column.DefaultValue);
				}

				if (field is SQLiteForeignKey foreignKey)
				{
					object propValue = propInfo.GetValue(data);

					if (propValue is null)
						return;

					commandText.Append(" ").AppendIfNotNullOrWhiteSpace(tableAttribute.Alias, suffix: ".").Append(foreignKey.Name, " = ", parameterName, ",");
					command.Parameters.AddWithValue(parameterName, propInfo.PropertyType.GetProperties()
						.FirstOrDefault(_ =>
							_.GetCustomAttribute(typeof(SQLiteColumnAttribute)) is SQLiteColumnAttribute targetColumn
							&& targetColumn.Name == foreignKey.TargetColumn).GetValue(propValue));
				}
			});
			#endregion

			// No data to update.
			if (command.Parameters.Count == 0)
				return;

			commandText.Length -= 1;
			commandText.Append(conditions);
			command.CommandText = commandText.ToString();

			ExecuteNonQuery(command); //Execute the UPDATE command.
		}
		#endregion

		#region Update(SQLiteConnection, List<T>)
		/// <summary>
		/// Updates the specified data into the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connection">An open SQLite connection.</param>
		/// <param name="data">Data to be updated in the database.</param>
		public static void Update<T>(SQLiteConnection connection, List<T> data) =>
			data?.ForEach(_ => Update(connection, _));
		#endregion

		#region Update(string, T)
		/// <summary>
		/// Updates the specified data in the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path.</param>
		/// <param name="data">Data to be updated in the database.</param>
		public static void Update<T>(string path, T data) =>
			Update(OpenConnection(path), data);
		#endregion

		#region Update(string, List<T>)
		/// <summary>
		/// Updates the specified data in the database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path">The SQLite database file path.</param>
		/// <param name="data">Data to be updated in the database.</param>
		public static void Update<T>(string path, List<T> data)
		{
			SQLiteConnection connection = OpenConnection(path);
			data?.ForEach(_ => Insert(connection, _));
		}
		#endregion

		#endregion

		#endregion

		#region Private methods

		#region AddSelectColumn
		/// <summary>
		/// Adds the specified column to the SELECT command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="tablePrefix"></param>
		/// <param name="columnName"></param>
		/// <param name="columnAlias"></param>
		private static void AddSelectColumn(StringBuilder command, string tablePrefix, string columnName, string columnAlias) =>
			command.Append(" ")
				.AppendIfNotNullOrWhiteSpace(tablePrefix, suffix: ".")
				.Append(columnName)
				.AppendIfNotNullOrWhiteSpace(columnAlias, " AS ")
				.Append(",");
		#endregion

		#region AddSelectColumnDictionary
		/// <summary>
		/// Adds the column and its corresponding property into the dictionary.
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="columnName"></param>
		/// <param name="propertyName"></param>
		private static void AddSelectColumnDictionary(Dictionary<string, string> columns, string columnName, string propertyName)
		{
			if (columns.ContainsKey(columnName))
			{
				int columnCount = 1;

				while (!columns.TryAdd($"{columnName}{columnCount}", propertyName))
					columnCount++;
			}
			else
				columns.Add(columnName, propertyName);
		}
		#endregion

		#region AddUpdateParameter
		/// <summary>
		/// Adds the specified parameter with value to the INSERT/UPDATE <see cref="SQLiteCommand"/>'s <see cref="SQLiteParameterCollection"/>.
		/// </summary>
		/// <param name="valuesList"></param>
		/// <param name="command"></param>
		/// <param name="parameterName"></param>
		/// <param name="value"></param>
		private static void AddUpdateParameter(StringBuilder valuesList, SQLiteCommand command, string parameterName, object value)
		{
			valuesList.Append(" ", parameterName, ",");
			command.Parameters.AddWithValue(parameterName, value);
		}
		#endregion

		#region ConvertDataTable
		/// <summary>
		/// Converts the specified <see cref="DataTable"/> into the target type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataTable"></param>
		/// <param name="fields">Dictionary linking the <see cref="DataTable"/> columns and the target type's fields.</param>
		/// <returns></returns>
		private static List<T> ConvertDataTable<T>(DataTable dataTable, Dictionary<string, string> fields)
		{
			List<T> results = new(dataTable.Rows.Count);

			foreach (DataRow row in dataTable.Rows)
			{
				T currentItem = Activator.CreateInstance<T>();
				Type currentType = typeof(T);

				foreach (DataColumn column in dataTable.Columns)
					if (fields[column.ColumnName] is { } propertyName && row[column] is not DBNull)
						currentType.GetProperty(propertyName).SetValue(currentItem, row[column]);

				results.Add(currentItem);
			}

			return results;
		}
		#endregion

		#region GenerateJoin
		/// <summary>
		/// Generates the JOIN clause for the specified model type.
		/// </summary>
		/// <typeparam name="T">Model type from which the join data will be extracted.</typeparam>
		/// <param name="commandText">SQL command string.</param>
		private static void GenerateJoin<T>(StringBuilder commandText) =>
			(Attribute.GetCustomAttributes(typeof(T), typeof(SQLiteJoinAttribute)) as SQLiteJoinAttribute[]).ForEach(join =>
			{
				string joinCommand = join.Mode switch
				{
					JoinMode.Cross => "CROSS",
					JoinMode.Outer => "OUTER",
					_ => "INNER"
				};

				commandText.Append(" ", joinCommand, " JOIN ", join.Table).AppendIfNotNullOrWhiteSpace(join.Alias, " AS ")
					.Append(" ON ", join.Constraint);
			});
		#endregion

		#region GetSQLiteFieldAttribute
		/// <summary>
		/// Retrieves the <see cref="SQLiteField"/> attribute of the specified property.
		/// </summary>
		/// <param name="propInfo"></param>
		/// <returns></returns>
		// ReSharper disable once InconsistentNaming
		private static SQLiteField GetSQLiteFieldAttribute(PropertyInfo propInfo)
		{
			SQLiteField field = null;

			SetSQLiteFieldAttribute<SQLiteColumnAttribute>(propInfo, ref field);
			SetSQLiteFieldAttribute<SQLiteCustomFieldAttribute>(propInfo, ref field);
			SetSQLiteFieldAttribute<SQLiteForeignKey>(propInfo, ref field);
			SetSQLiteFieldAttribute<SQLiteManyToManyData>(propInfo, ref field);
			SetSQLiteFieldAttribute<SQLiteOneToManyData>(propInfo, ref field);

			return field;
		}
		#endregion

		#region CheckUpdateProperty
		/// <summary>
		/// Verifies whether the specified property is correctly set for an INSERT or UPDATE command.
		/// </summary>
		/// <param name="propAttribute"></param>
		/// <param name="propValue"></param>
		private static void CheckUpdateProperty(SQLiteColumnAttribute propAttribute, object propValue)
		{
			if (propAttribute.UpdateBehaviour == UpdateBehaviour.Mandatory
				&& propValue is null
				&& propAttribute.DefaultValue is null)
				throw new SQLiteMandatoryFieldException(
					$"The property \"{propAttribute.Name}\" must contain a value or a default value.");

			if (propAttribute.MaxLength > 0 && propValue?.ToString().Length > propAttribute.MaxLength)
				throw new SQLiteMaximumLengthExceededException(
					$"The property \"{propAttribute.Name}\" value's length must be equal or less than {propAttribute.MaxLength}.");
		}
		#endregion

		#region GetTableAttribute
		/// <summary>
		/// Verifies whether the specified type contains the <see cref="SQLiteTableAttribute"/> attribute and returns it.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static SQLiteTableAttribute GetTableAttribute(Type type)
		{
			if (Attribute.GetCustomAttribute(type, typeof(SQLiteTableAttribute)) is not SQLiteTableAttribute tableAttribute)
				throw new SQLiteMissingAttributeException("The SQLiteTableAttribute attribute is mandatory to run SQLite commands.");

			return tableAttribute;
		}
		#endregion

		#region SetSQLiteFieldAttribute
		/// <summary>
		/// Gets the <see cref="SQLiteField"/> attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propInfo"></param>
		/// <param name="field"></param>
		// ReSharper disable once InconsistentNaming
		private static void SetSQLiteFieldAttribute<T>(PropertyInfo propInfo, ref SQLiteField field) where T : SQLiteField
		{
			SQLiteField foundAttribute = (T)propInfo.GetCustomAttribute(typeof(T));

			// Only one attribute allowed for each property.
			if (foundAttribute is not null && field is not null)
				throw new SQLiteIncompatibleAttributesException(
					"Only one SQLiteField attribute is allowed for each property.");

			if (foundAttribute is not null)
				field = foundAttribute;
		}
		#endregion

		#endregion
	}
}