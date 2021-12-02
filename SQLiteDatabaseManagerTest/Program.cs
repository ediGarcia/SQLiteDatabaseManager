using SQLiteDatabaseManager;
using SQLiteDatabaseManager.Attributes;
using SQLiteDatabaseManager.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Security.Policy;

namespace SQLiteDatabaseManagerTest
{
	class Program
	{
		static void Main(string[] args)
		{
			string path = @"C:\Users\avaz\Downloads\chinook.db";

		//SQLiteHelper.RealSelect<TableTest>(@"C:\Users\avaz\Downloads\sqliteadmin\chinook.db", new TableTest { Id = 3 }, "CLI_ID_INT IS NOT NULL", 3);
		//Artist[] artists = SQLiteHelper.Select<Artist>(path).ToArray();
		//Album[] albums = SQLiteHelper.Select<Album>(@"C:\Users\avaz\Downloads\sqliteadmin\chinook.db").ToArray();
		//Employee[] employees = SQLiteHelper.Select<Employee>(@"C:\Users\avaz\Downloads\sqliteadmin\chinook.db").ToArray();
		//Test[] tests = SQLiteHelper.Select<Test>(@"C:\Users\avaz\Downloads\sqliteadmin\demo.db").ToArray();
		//TestInsert(path);
		//TestUpdate(path);
		//var c = TestExists(path);
		//TestDelete(path);
		Playlist[] playlists = SQLiteHelper.Select<Playlist>(path).ToArray();



		/*FileInfo arquivoInfo = new FileInfo(@"C:\Users\avaz\Desktop\a.zip");
		FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(@"ftp://ftp.rdisoftware.com/capgemini/outgoing/FusionInventory_Capgemini.zip"));
		request.Method = WebRequestMethods.Ftp.UploadFile;
		request.Credentials = new NetworkCredential("avaz", "C@pgemini5");
		request.UseBinary = true;
		request.ContentLength = arquivoInfo.Length;
		using (FileStream fs = arquivoInfo.OpenRead())
		{
			byte[] buffer = new byte[2048];
			int bytesSent = 0;
			int bytes = 0;
			using (Stream stream = request.GetRequestStream())
			{
				while (bytesSent < arquivoInfo.Length)
				{
					bytes = fs.Read(buffer, 0, buffer.Length);
					stream.Write(buffer, 0, bytes);
					bytesSent += bytes;
				}
			}
		}*/

		/*FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(@"ftp://ftp.rdisoftware.com/capgemini/outgoing/FusionInventory_Capgemini.zip"));
		request.Method = WebRequestMethods.Ftp.DownloadFile;
		request.Credentials = new NetworkCredential("avaz", "C@pgemini5");
		request.UseBinary = true;
		using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
		{
			using (Stream rs = response.GetResponseStream())
			{
				using (FileStream ws = new FileStream(@"C:\Users\avaz\Desktop\a.zip", FileMode.Create))
				{
					byte[] buffer = new byte[2048];
					int bytesRead = rs.Read(buffer, 0, buffer.Length);
					while (bytesRead > 0)
					{
						ws.Write(buffer, 0, bytesRead);
						bytesRead = rs.Read(buffer, 0, buffer.Length);
					}
				}
			}
		}*/


			var b = 0;
		}

		private static void TestInsert(string path)
		{
			Employee newEmployee = new()
			{
				Id = 1000,
				Title = "Mr.",
				FirstName = null,
				LastName = "Vaz",
				Email = "eddy.garcia07@gmail.com",
				Address = "Back Shopping St., 3395",
				PostalCode = "18074-385",
				//Country = "Brazil",
				//State = "São Paulo",
				City = "Sorocaba",
				BirthDate = new DateTime(1991, 5, 1),
				HireDate = new DateTime(2018, 5, 16),
				Phone = "(15) 99191-0786",
				ReportsTo = new Employee
				{
					Id = 3
				}
			};
			SQLiteHelper.Insert(path, newEmployee);
			var b = 0;	
		}

		private static void TestUpdate(string path)
		{
			Employee newEmployee = new()
			{
				Id = 1000,
				Title = "Mr.",
				FirstName = "Power",
				LastName = "Guido",
				//Email = "eddy.garcia07@gmail.com",
				Address = "Back Shopping St., 3395",
				//PostalCode = "18074-385",
				//Country = "Brazil",
				//State = "São Paulo",
				City = "Sorocaba",
				BirthDate = new DateTime(1991, 5, 1),
				HireDate = new DateTime(2018, 5, 16),
				Phone = "(15) 99191-0786",
				ReportsTo = new Employee
				{
					Id = 4
				}
			};
			SQLiteHelper.Update(path, newEmployee);
			var b = 0;
		}

		private static bool TestExists(string path)
		{
			Employee newEmployee = new()
			{
				Id = 1000,
				ReportsTo = new Employee
				{
					Id = 4
				}
				/*Title = "Mr.",
				FirstName = "Power",
				LastName = "Guido",
				//Email = "eddy.garcia07@gmail.com",
				Address = "Back Shopping St., 3395",
				//PostalCode = "18074-385",
				//Country = "Brazil",
				//State = "São Paulo",
				City = "Sorocaba",
				BirthDate = new DateTime(1991, 5, 1),
				HireDate = new DateTime(2018, 5, 16),
				Phone = "(15) 99191-0786",
				ReportsTo = new Employee
				{
					Id = 4
				}*/
			};
			return SQLiteHelper.Exists(path, newEmployee, checkOnlyPrimary: true);
		}

		private static void TestDelete(string path)
		{
			Employee newEmployee = new()
			{
				Id = 1000,
				ReportsTo = new Employee
				{
					Id = 4
				}
				/*Title = "Mr.",
				FirstName = "Power",
				LastName = "Guido",
				//Email = "eddy.garcia07@gmail.com",
				Address = "Back Shopping St., 3395",
				//PostalCode = "18074-385",
				//Country = "Brazil",
				//State = "São Paulo",
				City = "Sorocaba",
				BirthDate = new DateTime(1991, 5, 1),
				HireDate = new DateTime(2018, 5, 16),
				Phone = "(15) 99191-0786",
				ReportsTo = new Employee
				{
					Id = 4
				}*/
			};
			SQLiteHelper.Delete(path, newEmployee);
		}
	}

	[SQLiteTable("TAB_CLI_CLIENTS", Where = "CLI_INT_HASDEBT = 0")]
	[SQLiteJoin("TAB_DEB_DEBTS", "DEB", "CLI_INT_ID = DEB_CLI_INT_ID", Mode = JoinMode.Outer)]
	public class TableTest
	{
		[SQLiteColumn("CLI_INT_ID")]
		public long? Id { get; set; }

		[SQLiteColumn("CLI_TXT_NAME")]
		public string Name { get; set; }

		[SQLiteCustomField("TOTAL_DEBT", "DEB_DBL_DEBT * CLI_INT_QUANTITY")]
		public double? TotalDebt { get; set; }

		[SQLiteColumn("DEB_TXT_DEBNAME", TableAlias = "DEB")]
		public string DebtName { get; set; }
	}

	[SQLiteTable("ARTISTS", Alias = "ART")]
	public class Artist
	{
		[SQLiteColumn("ARTISTID")]
		public long? Id { get; set; }

		[SQLiteColumn("NAME")]
		public string Name { get; set; }
		
		[SQLiteOneToManyData("ARTISTID", "ARTISTID")]
		public List<Album> Albuns { get; set; }
	}

	[SQLiteTable("ALBUMS", Alias = "ALB")]
	[SQLiteJoin("ARTISTS", "ART", "ALB.ARTISTID = ART.ARTISTID")]
	public class Album
	{
		[SQLiteColumn("ALBUMID")]
		public long? Id { get; set; }

		[SQLiteColumn("ARTISTID")]
		public long? ArtistId { get; set; }

		[SQLiteColumn("TITLE")]
		public string Title { get; set; }

		[SQLiteColumn("NAME", TableAlias = "ART")]
		public string ArtistName { get; set; }

		[SQLiteCustomField("ALBUMTITLE", "ART.NAME || ' - ' || ALB.TITLE")]
		public string AlbumTitle { get; set; }

		//[SQLiteForeignKey("ARTISTID", "ARTISTID")]
		//public Artist Artist { get; set; }
	}

	[SQLiteTable("Demo", Alias = "DEM")]
	public class Demo
	{
		[SQLiteColumn("ID")]
		public long? Id { get; set; }

		[SQLiteColumn("Name")]
		public string Name { get; set; }

		[SQLiteColumn("Hint")]
		public string Hint { get; set; }
	}

	[SQLiteTable("Test", Alias = "TST")]
	[SQLiteJoin("DEMO", "DEM", "TST.HINTID = DEM.ID")]
	public class Test
	{
		[SQLiteColumn("ID")]
		public long? Id { get; set; }

		[SQLiteColumn("HintId")]
		public long? HintId { get; set; }

		[SQLiteColumn("Name")]
		public string Name { get; set; }

		[SQLiteColumn("ID", TableAlias = "DEM")]
		public long? IdFromHint { get; set; }
	}

	[SQLiteTable("Employees", Alias = "Emp")]
	public class Employee
	{
		[SQLiteColumn("EmployeeId", IsPrimaryKey = true)]
		public long? Id { get; set; }

		[SQLiteColumn("LastName", MaxLength = 20)]
		public string LastName { get; set; }

		[SQLiteColumn("FirstName", MaxLength = 20, UpdateBehaviour = UpdateBehaviour.Mandatory, DefaultValue = "Mirela")]
		public string FirstName { get; set; }

		[SQLiteColumn("Title", MaxLength = 30)]
		public string Title { get; set; }

		[SQLiteForeignKey("ReportsTo", "EmployeeId")]
		public Employee ReportsTo { get; set; }

		[SQLiteColumn("BirthDate")]
		public DateTime? BirthDate { get; set; }

		[SQLiteColumn("HireDate")]
		public DateTime? HireDate { get; set; }

		[SQLiteColumn("Address", MaxLength = 70)]
		public string Address { get; set; }

		[SQLiteColumn("City", MaxLength = 40)]
		public string City { get; set; }

		[SQLiteColumn("State", MaxLength = 40)]
		public string State { get; set; }

		[SQLiteColumn("Country", MaxLength = 40, UpdateBehaviour = UpdateBehaviour.IgnoreNull)]
		public string Country { get; set; }

		[SQLiteColumn("PostalCode", MaxLength = 10)]
		public string PostalCode { get; set; }

		[SQLiteColumn("Phone", MaxLength = 24)]
		public string Phone { get; set; }

		[SQLiteColumn("Fax", MaxLength = 24)]
		public string Fax { get; set; }

		[SQLiteColumn("Email", MaxLength = 60)]
		public string Email { get; set; }
	}

	[SQLiteTable("playlists")]
	public class Playlist
	{
		[SQLiteColumn("PlaylistId", IsPrimaryKey = true)]
		public long? Id { get; set; }

		[SQLiteColumn("Name", MaxLength = 120)]
		public string Name { get; set; }

		[SQLiteManyToManyData("playlist_track", "PlaylistId", "PlaylistId", "TrackId")]
		public List<Track> Tracks { get; set; }
	}

	[SQLiteTable("genres")]
	public class Genre
	{
		[SQLiteColumn("GenreId", IsPrimaryKey = true)]
		public long? Id { get; set; }

		[SQLiteColumn("Name", MaxLength = 120)]
		public string Name { get; set; }
	}

	[SQLiteTable("media_types")]
	public class MediaType
	{
		[SQLiteColumn("MediaTypeId", IsPrimaryKey = true)]
		public long? Id { get; set; }

		[SQLiteColumn("Name", MaxLength = 120)]
		public string Name { get; set; }
	}

	[SQLiteTable("tracks", Alias = "trk")]
	[SQLiteJoin("genres", "gen", "trk.genreId = gen.genreId")]
	[SQLiteJoin("media_types", "mdt", "trk.mediaTypeId = mdt.mediaTypeId")]
	public class Track
	{
		[SQLiteColumn("TrackId", IsPrimaryKey = true)]
		public long? Id { get; set; }

		[SQLiteColumn("Name", MaxLength = 120)]
		public string Name { get; set; }

		[SQLiteForeignKey("AlbumId", "AlbumId")]
		public Album Album { get; set; }

		[SQLiteForeignKey("MediaTypeId", "MediaTypeId")]
		public MediaType MediaType { get; set; }

		[SQLiteColumn("Name", TableAlias = "mdt")]
		public string MediaTypeName { get; set; }

		[SQLiteForeignKey("GenreId", "GenreId")]
		public Genre Genre { get; set; }

		[SQLiteColumn("Name", TableAlias = "gen")]
		public string GenreName { get; set; }

		[SQLiteColumn("Composer", MaxLength = 200)]
		public string Composer { get; set; }

		[SQLiteColumn("Milliseconds")]
		public long? Milliseconds { get; set; }

		[SQLiteColumn("Bytes")]
		public long? Bytes { get; set; }

		[SQLiteColumn("UnitPrice")]
		public decimal? UnitPrice { get; set; }
	}
}
