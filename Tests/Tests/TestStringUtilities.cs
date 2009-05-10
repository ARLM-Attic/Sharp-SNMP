/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 2008/9/30
 * Time: 11:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Lextm.SharpSnmpLib.Mib;
using NUnit.Framework;


namespace Lextm.SharpSnmpLib.Tests
{
    [TestFixture]
    public class TestStringUtilities
    {
        [Test]
        public void TestExtractValue()
        {
            string test = "org(3)";
            Assert.AreEqual(3, StringUtility.ExtractValue(test));
        }
        
        [Test]
        public void TestExtractName()
        {
            string test = "org(3)";
            Assert.AreEqual("org", StringUtility.ExtractName(test));
            
            string test1 = "iso";
            Assert.AreEqual("iso", StringUtility.ExtractName(test1));
        }
    }
}
