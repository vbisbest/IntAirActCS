using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntAirAct;
using System.Collections.Generic;

namespace IntAirActTests
{
    [TestClass]
    public class IADeserializationTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
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
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void BodyAsStringTest()
        {
            string expected = "example string";
            IADeSerialization deSerialization = new IADeSerialization();
            deSerialization.SetBodyWithString(expected);
            string actual = deSerialization.BodyAs<String>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void BodyAsNumberTest()
        {
            int expected = 50;
            IADeSerialization deSerialization = new IADeSerialization();
            deSerialization.SetBodyWith(expected);
            int actual = deSerialization.BodyAs<int>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void BodyAsAnArrayOfStringTest()
        {
            string[] expected = new string[] { "example string" };
            IADeSerialization deSerialization = new IADeSerialization();
            deSerialization.SetBodyWith(expected);
            string[] actual = deSerialization.BodyAs<string[]>();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void BodyAsAnArrayOfNumbersTest()
        {
            int[] expected = new int[] { 50 };
            IADeSerialization deSerialization = new IADeSerialization();
            deSerialization.SetBodyWith(expected);
            int[] actual = deSerialization.BodyAs<int[]>();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void BodyAsADictionaryTest()
        {
            Dictionary<string, string> expected = new Dictionary<string, string>() { {"key", "value"} };
            IADeSerialization deSerialization = new IADeSerialization();
            deSerialization.SetBodyWith(expected);
            Dictionary<string, string> actual = deSerialization.BodyAs<Dictionary<string, string>>();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
