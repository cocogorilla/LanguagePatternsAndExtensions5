using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Idioms;
using Xunit;

namespace LanguagePatternsAndExtensions.Tests
{
    public class LifetimeManagerTests
    {
        public class TestObject
        {
            public Guid SomethingToTest { get; set; }
        }

        [Theory, Gen]
        public void IsGuarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(LifeTimeManager<TestObject>).GetConstructors());
        }

        [Theory, Gen]
        public async Task NewObjectIsGeneratedForFirstCall(
            TestObject expected,
            IFixture fixture)
        {
            fixture.Inject<Func<Task<TestObject>>>(
                async () => await Task.FromResult(expected));
            var sut = fixture.Create<LifeTimeManager<TestObject>>();
            var actual = await sut.ReceiveMessage();
            Assert.Equal(expected, actual);
        }

        [Theory, Gen]
        public async Task NewObjectIsGeneratedOnlyOnceOnFirstCall(
            IFixture fixture)
        {
            var callCount = 0;
            fixture.Inject<Func<Task<int>>>(
                async () =>
                {
                    callCount++;
                    return callCount;
                });
            var sut = fixture.Create<LifeTimeManager<int>>();
            var actual = await sut.ReceiveMessage();
            Assert.Equal(1, actual);
        }

        [Theory, Gen]
        public async Task SameObjectIsGeneratedWhenNotExpired(
            TestObject expected,
            IFixture fixture)
        {
            fixture.Inject<Func<Task<TestObject>>>(
                async () => await Task.FromResult(expected));
            fixture.Inject<Func<TestObject, bool>>(
                (t) => t != expected);
            var sut = fixture.Create<LifeTimeManager<TestObject>>();

            var initial = await sut.ReceiveMessage();
            var actual = await sut.ReceiveMessage();
            var successive = await sut.ReceiveMessage();

            Assert.Equal(expected, initial);
            Assert.Equal(expected, actual);
            Assert.Equal(expected, successive);
        }

        [Theory, Gen]
        public async Task NewObjectIsGeneratedWhenIsExpired(
            int expiresAt,
            TestObject oldObject,
            TestObject newObject,
            IFixture fixture)
        {
            int callCount = 0;
            fixture.Inject<Func<Task<TestObject>>>(
                async () =>
                {
                    if (callCount == 0) return await Task.FromResult(oldObject);
                    if (callCount == expiresAt) return await Task.FromResult(newObject);
                    return await Task.FromException<TestObject>(new Exception("should never have gotten here"));
                });
            fixture.Inject<Func<TestObject, bool>>(
                (t) => ++callCount == expiresAt);
            var sut = fixture.Create<LifeTimeManager<TestObject>>();

            for (var i = 0; i <= expiresAt + 5; i++)
            {
                var actual = await sut.ReceiveMessage();
                Assert.Equal(i < expiresAt ? oldObject : newObject, actual);
            }
        }

        [Theory, Gen]
        public async Task InstancesThrowWhenNullIsReturned(
            IFixture fixture)
        {
            fixture.Inject<Func<Task<string>>>(
                async () => null);
            var sut = fixture.Create<LifeTimeManager<string>>();
            await Assert.ThrowsAsync<LifeTimeManagerException>(async () => await sut.ReceiveMessage());
        }

        [Theory, Gen]
        public async Task NewObjectIsGeneratedWhenForciblyUpdated(
            TestObject oldObject,
            TestObject newObject,
            IFixture fixture)
        {
            int callCount = 0;
            fixture.Inject<Func<Task<TestObject>>>(
                async () =>
                {
                    if (callCount == 0) return await Task.FromResult(oldObject);
                    if (callCount == 1) return await Task.FromResult(newObject);
                    return await Task.FromException<TestObject>(new Exception("should never have gotten here"));
                }); ;

            var sut = fixture.Create<LifeTimeManager<TestObject>>();

            var e1 = await sut.ReceiveMessage();
            Assert.Equal(e1, oldObject);
            var e2 = await sut.ReceiveMessage();
            Assert.Equal(e2, oldObject);
            callCount++;
            var e3 = await sut.ReceiveMessage(true);
            Assert.Equal(e3, newObject);
        }
    }
}
