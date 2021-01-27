using System;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi.UnitTests.OAuth2TokenTests
{
    public class WhenCheckingIfExpired
    {
        [Test]
        public void ThenItShouldReturnFalseIfExpiredInPlusAcquiredAtLessThanNow()
        {
            var token = new OAuth2Token
            {
                ExpiresIn = 100,
                AcquiredAt = DateTime.Now.AddSeconds(-5),
            };
            
            Assert.IsFalse(token.HasExpired());
        }
        
        [Test]
        public void ThenItShouldReturnTrueIfExpiredInPlusAcquiredAtLessThanNowButWithTolerance()
        {
            var token = new OAuth2Token
            {
                ExpiresIn = 100,
                AcquiredAt = DateTime.Now.AddSeconds(-5),
            };
            
            Assert.IsTrue(token.HasExpired(95));
        }
        
        [Test]
        public void ThenItShouldReturnTrueIfExpiredInPlusAcquiredAtEqualsNow()
        {
            var token = new OAuth2Token
            {
                ExpiresIn = 100,
                AcquiredAt = DateTime.Now.AddSeconds(-100),
            };
            
            Assert.IsTrue(token.HasExpired());
        }
        
        [Test]
        public void ThenItShouldReturnTrueIfExpiredInPlusAcquiredAtGreaterThanNow()
        {
            var token = new OAuth2Token
            {
                ExpiresIn = 100,
                AcquiredAt = DateTime.Now.AddSeconds(-120),
            };
            
            Assert.IsTrue(token.HasExpired());
        }
        
        [Test]
        public void ThenItShouldReturnFalseIfExpiredInIsZero()
        {
            var token = new OAuth2Token
            {
                ExpiresIn = 0,
                AcquiredAt = DateTime.Now.AddSeconds(-500),
            };
            
            Assert.IsFalse(token.HasExpired());
        }
    }
}