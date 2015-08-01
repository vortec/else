using Else.Extensibility;
using NUnit.Framework;

namespace Else.Tests
{
    [TestFixture]
    class QueryObjectTest
    {
        [TestCase]
        public void ParseQueryTest()
        {
            var query = new Query();
            
            // Test empty query
            query.Parse("");

            Assert.AreEqual("", query.Raw);
            Assert.AreEqual("", query.Keyword);
            Assert.AreEqual("", query.Arguments);
            Assert.IsFalse(query.KeywordComplete);
            Assert.IsTrue(query.Empty);
            Assert.IsFalse(query.HasArguments);
            Assert.IsFalse(query.IsPath);

            // Test query with arguments
            var q = "google search argument";
            query.Parse(q);

            Assert.AreEqual(q, query.Raw);
            Assert.AreEqual("google", query.Keyword);
            Assert.AreEqual("search argument", query.Arguments);
            Assert.IsTrue(query.KeywordComplete);
            Assert.IsFalse(query.Empty);
            Assert.IsTrue(query.HasArguments);
            Assert.IsFalse(query.IsPath);

            // Test query with no arguments
            query.Parse("the_keyword");
            Assert.AreEqual("the_keyword", query.Keyword);
            Assert.IsFalse(query.HasArguments);
            Assert.IsFalse(query.KeywordComplete);

            // test path query
            query.Parse(@"C:\\Users\\james\\Directory\\File");
            Assert.IsTrue(query.IsPath);

            // test constructor
            var q2 = new Query("yahoo keyword");
            Assert.AreEqual("yahoo keyword", q2.Raw);
        }
    }
}
