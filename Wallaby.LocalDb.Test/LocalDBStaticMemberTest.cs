using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace Wallaby.LocalDb.Test
{
    [TestClass]
    public class LocalDBStaticMemberTest
    {
        private Dictionary<string, string> createdDatabases = new Dictionary<string, string>();


        /// <summary>
        /// Test to make sure the GetDatabaseDefaultPath method return the default path and that the path is valid
        /// </summary>
        [TestMethod]
        public void DefaultPathTest()
        {
            string defaultPath = LocalDB.GetDatabaseDefaultPath();
            Assert.IsNotNull(defaultPath);
            Assert.IsTrue(defaultPath.Contains("Data"));
        }

        /// <summary>
        /// Make sure the GetDatabaseFullPath method provides a valid path
        /// </summary>
        [TestMethod]
        public void DatabaseFullPathTest()
        {
            const string databaseName = "TempDatabaseName";
            string fullPath = LocalDB.GetDatabaseFullPath(databaseName, LocalDB.GetDatabaseDefaultPath());
            Assert.IsTrue(fullPath.Contains(databaseName));
            Assert.IsTrue(fullPath.EndsWith(".mdf"));
            Assert.AreEqual(Path.GetExtension(fullPath), ".mdf");

        }

        /// <summary>
        /// Make sure the database log file path is correct
        /// </summary>
        [TestMethod]
        public void DatabaseLogFilePathTest()
        {
            const string databaseName = "Mike";
            string logPath = LocalDB.GetDatabaseLogPath(databaseName, Path.GetTempPath());
            Assert.IsTrue(logPath.EndsWith("_log.ldf"));
            Assert.IsTrue(logPath.Contains(databaseName));
            Assert.AreEqual(Path.GetExtension(logPath), ".ldf");

        }

        /// <summary>
        ///  Make sure the DatabaseExists method doesn't return a true when the database doesn't exist
        /// </summary>
        [TestMethod]
        public void DatabaseExistsTest()
        {
            Assert.IsFalse(LocalDB.DatabaseExists("FalseDatabaseName"));
        }

        [TestMethod]
        public void GetConnectionInvalidDatabaseTest()
        {
            // Make sure you get a sql exception if the database isn't valid
            // using the default path
            Assert.ThrowsException<FileNotFoundException>(()=> LocalDB.GetConnection("InvalidDatabaseName"));
            // passing in a path
            Assert.ThrowsException<FileNotFoundException>(() => LocalDB.GetConnection("InvalidDatabaseName", Path.GetTempPath()));

            try { LocalDB.GetConnection("InvalidDatabaseName"); }
            catch (FileNotFoundException ex)
            {
                Assert.IsTrue(ex.Message.Contains("InvalidDatabaseName"));
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            foreach (var database in createdDatabases)
            {
                KillDatabase(database.Key, database.Value);
            }
        }

        [TestMethod]
        public void GetConnectionTest()
        {
            Random rand = new Random(); 
            // Create a database with a random name and open a connection
            string databaseName = $"database_{String.Join("", Enumerable.Range(0, 8).Select(c => (char)rand.Next(97, 122)).ToArray<char>())}";
            createdDatabases.Add(databaseName, Path.GetTempPath());
            LocalDB.CreateDatabase(databaseName, Path.GetTempPath());
            SqlConnection connection = LocalDB.GetConnection(databaseName, Path.GetTempPath());

            // Make sure the connection is valid and open
            Assert.IsNotNull(connection, "Connection object returned from LocalDB.GetConnection is null");
            Assert.IsTrue(connection.State.Equals(ConnectionState.Open));

            Assert.AreEqual(connection.Database, databaseName);


            // Make sure the connection can be closed
            connection.Close();
            Assert.AreEqual(connection.State, ConnectionState.Closed);

        }

        /// <summary>
        /// Used to clean up a temporary database without testing it. 
        /// </summary>
        private void KillDatabase(string databaseName, string basePath)
        {
            LocalDB.DetachDatabase(databaseName);
            LocalDB.RemoveDatabase(databaseName, basePath);
        }


        [TestMethod]
        public void CreateAndDeleteTest()
        {
            const string testDBName = "TestDB2";
            string tempBasePath = Path.GetTempPath();
            LocalDB.DetachDatabase(testDBName);


            // Before starting the test, make sure these files don't exist
            File.Delete(LocalDB.GetDatabaseFullPath(testDBName, tempBasePath));
            File.Delete(LocalDB.GetDatabaseLogPath(testDBName, tempBasePath));


            Assert.IsFalse(LocalDB.DatabaseExists(testDBName, tempBasePath));
            LocalDB.CreateDatabase(testDBName, tempBasePath);
            Assert.IsTrue(LocalDB.DatabaseExists(testDBName, tempBasePath));
            LocalDB.DetachDatabase(testDBName);
            LocalDB.RemoveDatabase(testDBName, tempBasePath);
            Assert.IsFalse(LocalDB.DatabaseExists(testDBName, tempBasePath));
            LocalDB.DetachDatabase(testDBName);

            //Make sure the files have been deleted
            Assert.IsFalse(File.Exists(LocalDB.GetDatabaseFullPath(testDBName, tempBasePath)));
            Assert.IsFalse(File.Exists(LocalDB.GetDatabaseLogPath(testDBName, tempBasePath)));
        }
    }
}
