using IntAirAct;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace IntAirActTests
{


    /// <summary>
    ///This is a test class for IARouteTest and is intended
    ///to contain all IARouteTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IARouteTest
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


        /// <summary>
        ///A test for IARoute Constructor
        ///</summary>
        [TestMethod()]
        public void IARouteConstructorTest()
        {
            string action = "GET";
            string resource = "/tests";
            string actualAction;
            string actualResource;
            IARoute target = new IARoute(action, resource);
            actualAction = target.Action;
            actualResource = target.Resource;
            Assert.AreEqual(action, actualAction);
            Assert.AreEqual(resource, actualResource);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsAgainstNullTest()
        {
            string action = "GET";
            string resource = "/tests";
            IARoute target = new IARoute(action, resource);
            object obj = null;
            bool expected = false;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTrueTest()
        {
            string action = "GET";
            string resource = "/tests";
            IARoute route1 = new IARoute(action, resource);
            IARoute route2 = new IARoute(action, resource);
            Assert.AreEqual(route1, route2);
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            string action = "GET";
            string resource = "/tests";
            IARoute route1 = new IARoute(action, resource);
            IARoute route2 = new IARoute(action, resource);
            Assert.AreEqual(route1.GetHashCode(), route2.GetHashCode());
        }
    }
}
