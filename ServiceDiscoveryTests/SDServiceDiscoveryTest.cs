using ServiceDiscovery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace ServiceDiscoveryTests
{
    
    
    /// <summary>
    ///This is a test class for SDServiceDiscoveryTest and is intended
    ///to contain all SDServiceDiscoveryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SDServiceDiscoveryTest
    {
        private static TraceSource logger = new TraceSource("ServiceDiscoveryTests");

        const string SERVICE_TYPE = "_test._tcp.";
        const ushort SERVICE_PORT = 80;

        private TestContext testContextInstance;
        private SDServiceDiscovery serviceDiscovery;

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


        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void SDServiceDiscoveryTestInitialize()
        {
            this.serviceDiscovery = new SDServiceDiscovery();
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            this.serviceDiscovery.Stop();
        }

        /// <summary>
        ///A test for SDServiceDiscovery Constructor
        ///</summary>
        [TestMethod()]
        public void PublishAndDiscoveryTest()
        {
            this.PublishWithName("");
        }

        private void PublishWithName(string name)
        {
            bool found = false;

            this.serviceDiscovery.ServiceFound += delegate(SDService service , bool ownService)
            {
                logger.TraceEvent(TraceEventType.Information, 0, String.Format("Found service: {0} ownService: {1}", service, ownService));
                if (ownService)
                {
                    found = true;
                }
            };

            this.serviceDiscovery.SearchForServices(SERVICE_TYPE);

            Assert.IsTrue(this.serviceDiscovery.IsSearching);

            this.serviceDiscovery.PublishService(SERVICE_TYPE, SERVICE_PORT);

            Assert.IsTrue(this.serviceDiscovery.IsPublishing);

            //wait
            System.Threading.Thread.Sleep(60000);

            this.serviceDiscovery.StopSearchingForServices(SERVICE_TYPE);

            Assert.IsFalse (this.serviceDiscovery.IsSearching);

            this.serviceDiscovery.StopPublishingService(SERVICE_TYPE, SERVICE_PORT);

            Assert.IsFalse(this.serviceDiscovery.IsPublishing);

            if (!found)
            {
                Assert.Fail();
            }
        }
    }
}
