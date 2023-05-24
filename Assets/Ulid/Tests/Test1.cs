using System;
using NUnit.Framework;
using Random = Unity.Mathematics.Random;

namespace LibSugar.Unity.Tests
{

    public class Test1
    {
        [SetUp]
        public void Init()
        {
            BurstUlid.InitStatic();
        }
        
        [Test]
        public void TestNew()
        {
            BurstUlid.NewUlid();
        }
    
        [Test]
        public void TestNewOuterRandom()
        {
            var random = new Random((uint)DateTimeOffset.Now.ToUnixTimeSeconds());
            BurstUlid.NewUlid(ref random);
        }
        
        [Test]
        public void TestToStringAndParse()
        {
            var ulid = BurstUlid.NewUlid();
            var str = ulid.ToString();
            var ulid2 = BurstUlid.Parse(str);
            
            Assert.AreEqual(ulid, ulid2);
        }
    
        [Test]
        public void TestGuidCast()
        {
            var ulid = BurstUlid.NewUlid();
            var guid = ulid.ToGuid();
            var ulid2 = BurstUlid.FromGuid(guid);
            
            Assert.AreEqual(ulid, ulid2);
        }
        
        [Test]
        public void TestToFixedStringAndParse()
        {
            var ulid = BurstUlid.NewUlid();
            var str = ulid.ToFixedString();
            var ulid2 = BurstUlid.Parse(str);
            
            Assert.AreEqual(ulid, ulid2);
        }
        
        [Test]
        public void TestNetFormat()
        {
            var ulid = BurstUlid.NewUlid();
            var net = ulid.ToNetFormat();
            var ulid2 = BurstUlid.FromNetFormat(net);
            
            Assert.AreEqual(ulid, ulid2);
        }
    }


}

