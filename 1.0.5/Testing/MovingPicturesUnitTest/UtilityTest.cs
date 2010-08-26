using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace MovingPicturesUnitTest
{
  
    /// <summary>
    ///This is a test class for UtilityTest and is intended
    ///to contain all UtilityTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UtilityTest {
        
        private TestContext testContextInstance;
        private FileInfo fileTest;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            fileTest = new FileInfo(@"c:\movies\The Big Movie cd2.avi");
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for trimSpaces
        ///</summary>
        [TestMethod()]
        public void trimSpacesTest() {
            string input = "A  B   C";
            string expected = "A B C";
            string actual;
            actual = Utility.TrimSpaces(input);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TitleToDisplayName
        ///</summary>
        [TestMethod()]
        public void TitleToDisplayNameTest() {
            string title = "Big Movie, The"; 
            string expected = "The Big Movie";
            string actual;
            actual = Utility.TitleToDisplayName(title);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TitleToArchiveName
        ///</summary>
        [TestMethod()]
        public void TitleToArchiveNameTest() {
            string title = "The Big Movie";
            string expected = "Big Movie, The";
            string actual;
            actual = Utility.TitleToArchiveName(title);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveFileStackMarkers
        ///</summary>
        [TestMethod()]
        public void RemoveFileStackMarkersTest1() {
            FileInfo file = fileTest;
            string expected = "The Big Movie";
            string actual;
            actual = Utility.GetFileNameWithoutExtensionAndStackMarkers(file);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveFileStackMarkers
        ///</summary>
        [TestMethod()]
        public void RemoveFileStackMarkersTest() {
            string filename = fileTest.Name; 
            string expected = "The Big Movie";
            string actual;
            actual = Utility.GetFileNameWithoutExtensionAndStackMarkers(filename);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for normalizeTitle
        ///</summary>
        [TestMethod()]
        public void normalizeTitleTest() {
            string title = "Star Wars: Episode IV – A New Hope";
            string expected = "star wars episode 4 a new hope";
            string actual;
            actual = Utility.NormalizeTitle(title);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for normalizeTitle
        ///</summary>
        [TestMethod()]
        public void normalizeTitleTest2() {
            string title = "Spïdér-Man I";
            string expected = "spider man";
            string actual;
            actual = Utility.NormalizeTitle(title);
            Assert.AreEqual(expected, actual);
        } 

        /// <summary>
        ///A test for IsMediaPortalVideoFile
        ///</summary>
        [TestMethod()]
        public void IsMediaPortalVideoFileTest() {
            FileInfo file = fileTest;
            bool expected = true;
            bool actual;
            actual = Utility.IsMediaPortalVideoFile(file);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for isFolderMultipart
        ///</summary>
        [TestMethod()]
        public void isFolderMultipartTest() {
            string name = "cd4"; // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Utility.isFolderMultipart(name);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for isFolderDedicated
        ///</summary>
        [TestMethod()]
        public void isFolderDedicatedTest() {
            //DirectoryInfo folder = null; // TODO: Initialize to an appropriate value
            //int expectedCount = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual = false;
            //actual = Utility.isFolderDedicated(folder, expectedCount);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for isKnownSubFolder
        ///</summary>
        [TestMethod()]
        public void isKnownSubFolderTest() {
            string name = "video_ts"; // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Utility.isKnownSubFolder(name);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for isFileMultiPart
        ///</summary>
        [TestMethod()]
        public void isFileMultiPartTest1() {
            string filename = fileTest.FullName; // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Utility.isFileMultiPart(filename);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for isFileMultiPart
        ///</summary>
        [TestMethod()]
        public void isFileMultiPartTest() {
            FileInfo file = fileTest; // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Utility.isFileMultiPart(file);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsDriveRoot
        ///</summary>
        [TestMethod()]
        public void IsDriveRootTest1() {
            string path = @"X:\" ; 
            bool expected = true;
            bool actual;
            actual = Utility.IsDriveRoot(path);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsDriveRoot
        ///</summary>
        [TestMethod()]
        public void IsDriveRootTest() {
            DirectoryInfo directory = fileTest.Directory; // TODO: Initialize to an appropriate value
            bool expected = false; 
            bool actual;
            actual = Utility.IsDriveRoot(directory);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetVideoFileCount
        ///</summary>
        [TestMethod()]
        public void GetVideoFileCountTest() {
            //DirectoryInfo folder = null; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual = 0;
            //actual = Utility.GetVideoFileCount(folder);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetMovieBaseDirectory
        ///</summary>
        [TestMethod()]
        public void GetMovieBaseDirectoryTest() {
            //DirectoryInfo directory = null; // TODO: Initialize to an appropriate value
            DirectoryInfo expected = null; // TODO: Initialize to an appropriate value
            DirectoryInfo actual = null;
            //actual = Utility.GetMovieBaseDirectory(directory);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetEnumValueDescription
        ///</summary>
        [TestMethod()]
        public void GetEnumValueDescriptionTest() {
            object value = VideoFormat.DVD;
            string expected = @"\video_ts\video_ts.ifo";
            string actual;
            actual = Utility.GetEnumValueDescription(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for EnumToList
        ///</summary>
        public void EnumToListTestHelper<T>() {
            List<T> expected = null; // TODO: Initialize to an appropriate value
            List<T> actual = null;
            //actual = Utility.EnumToList<T>();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void EnumToListTest() {
            EnumToListTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for CreateFilename
        ///</summary>
        [TestMethod()]
        public void CreateFilenameTest() {
            string subject = "MyN*wFile`Name.ext";
            string expected = "MyN_wFile`Name.ext";
            string actual;
            actual = Utility.CreateFilename(subject);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetFilesRecursive
        ///</summary>
        [TestMethod()]
        public void GetFilesRecursiveTest() {
            //DirectoryInfo directory = null; // TODO: Initialize to an appropriate value
            List<FileInfo> expected = null; // TODO: Initialize to an appropriate value
            List<FileInfo> actual = null;
            //actual = Utility.GetFilesRecursive(directory);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetVideoFilesRecursive
        ///</summary>
        [TestMethod()]
        public void GetVideoFilesRecursiveTest() {
            //DirectoryInfo directory = null; // TODO: Initialize to an appropriate value
            List<FileInfo> expected = null; // TODO: Initialize to an appropriate value
            List<FileInfo> actual = null;
            //actual = Utility.GetVideoFilesRecursive(directory);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
