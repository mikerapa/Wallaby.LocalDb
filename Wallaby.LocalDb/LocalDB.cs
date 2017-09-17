using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wallaby.LocalDb
{
    public class LocalDB
    {
        private const string DEFAULT_DATA_FOLDER = "Data";
        private const string BASE_CONNECTION_STRING = @"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True";
        /// <summary>
        /// Gets the default path used for saving the database file.
        /// </summary>
        /// <returns></returns>
        public static string GetDatabaseDefaultPath() => 
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DEFAULT_DATA_FOLDER);
        

        /// <summary>
        /// Derive the full path of the database file
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static string GetDatabaseFullPath(string databaseName, string basePath) =>
            Path.Combine(basePath, databaseName + ".mdf");
        

        /// <summary>
        /// Gets the path of the log file
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static string GetDatabaseLogPath(string databaseName, string basePath) =>
            Path.Combine(basePath, databaseName + "_log.ldf");

        /// <summary>
        /// Create a database with the specified name in the default location
        /// </summary>
        /// <param name="databaseName"></param>
        public static void CreateDatabase(string databaseName) => CreateDatabase(databaseName, GetDatabaseDefaultPath());

        /// <summary>
        /// Create a database with the specified name in the given path.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="basePath"></param>
        public static void CreateDatabase(string databaseName, string basePath)
        {
            string fullFilePath = GetDatabaseFullPath(databaseName, basePath);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            try
            {
                using (var connection = new SqlConnection(BASE_CONNECTION_STRING))
                {
                    connection.Open();
                    DetachDatabase(databaseName);
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = $"CREATE DATABASE {databaseName} ON (NAME = N'{databaseName}', FILENAME = '{fullFilePath}')";
                    command.ExecuteNonQuery();
                }

            }
            catch { throw;}
        }



        /// <summary>
        /// Determines if a database exists as a file
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static bool DatabaseExists(string databaseName) => (DatabaseExists(databaseName, GetDatabaseDefaultPath()));

        /// <summary>
        /// Determines if a database exists as a file
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static bool DatabaseExists(string databaseName, string basePath) => (File.Exists(GetDatabaseFullPath(databaseName, basePath)));

        /// <summary>
        /// Deletes a database
        /// </summary>
        /// <param name="databaseName"></param>
        public static void RemoveDatabase(string databaseName) => RemoveDatabase(databaseName, GetDatabaseDefaultPath());

        /// <summary>
        /// Deletes the database
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="basePath"></param>
        public static void RemoveDatabase(string databaseName, string basePath)
        {
            File.Delete(GetDatabaseFullPath(databaseName, basePath));
            File.Delete(GetDatabaseLogPath(databaseName, basePath));
        }

        public static void DetachDatabase(string databaseName)
        {
            try
            {
                using (var connection = new SqlConnection(BASE_CONNECTION_STRING))
                {
                    connection.Open();
                    new SqlCommand($"exec sp_detach_db '{databaseName}'", connection).ExecuteNonQuery();
                }
            }
            catch { }
        }
    }

}

