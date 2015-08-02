using Else.Extensibility;
using NUnit.Framework;

namespace Else.Tests
{
    [TestFixture]
    class QueryObjectTest
    {
        [Test]
        public void TestEmptyQuery()
        {
            var query = new Query("");
            Assert.That(query.Raw, Is.EqualTo(""));
            Assert.That(query.Keyword, Is.EqualTo(""));
            Assert.That(query.Arguments, Is.EqualTo(""));
            Assert.IsFalse(query.KeywordComplete);
            Assert.IsTrue(query.Empty);
            Assert.IsFalse(query.HasArguments);
            Assert.IsFalse(query.IsPath);
        }

        [Test]
        public void TestQueryWithKeyword()
        {
            var query = new Query("the_keyword");
            Assert.That(query.Raw, Is.EqualTo("the_keyword"));
            Assert.That(query.Keyword, Is.EqualTo("the_keyword"));
            Assert.That(query.Arguments, Is.Empty);
            Assert.IsFalse(query.KeywordComplete);
            Assert.IsFalse(query.Empty);
            Assert.IsFalse(query.HasArguments);
            Assert.IsFalse(query.IsPath);
        }

        [Test]
        public void TestQueryWithKeywordAndArguments()
        {
            var query = new Query("google search argument");
            Assert.That(query.Raw, Is.EqualTo("google search argument"));
            Assert.That(query.Keyword, Is.EqualTo("google"));
            Assert.That(query.Arguments, Is.EqualTo("search argument"));
            Assert.IsTrue(query.KeywordComplete);
            Assert.IsFalse(query.Empty);
            Assert.IsTrue(query.HasArguments);
            Assert.IsFalse(query.IsPath);
        }

        [Test]
        public void TestQueryWithPath()
        {
            var query = new Query(@"C:\\Program Files\\Directory\\File");
            Assert.That(query.Raw, Is.EqualTo(@"C:\\Program Files\\Directory\\File"));
            Assert.That(query.Keyword, Is.Empty);
            Assert.That(query.Arguments, Is.Empty);
            Assert.IsFalse(query.KeywordComplete);
            Assert.IsFalse(query.Empty);
            Assert.IsFalse(query.HasArguments);
            Assert.IsTrue(query.IsPath);
        }
    }
}
