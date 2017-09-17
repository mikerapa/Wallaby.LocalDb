using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wallaby.LocalDb.Test
{
    [TestClass]
    public class LocalDBStaticMemberTest
    {
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
