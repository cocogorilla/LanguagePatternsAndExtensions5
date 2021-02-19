using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Xunit;

namespace LanguagePatternsAndExtensions.Tests
{
    public class ExtensionTests
    {
        [Fact]
        public void AsReadonlyCollectionIsCorrect()
        {
            var expected = Enumerable.Repeat(new Random().Next(), 10);
            var actual = expected.AsReadonlyCollection();
            Assert.IsAssignableFrom<IReadOnlyCollection<int>>(actual);
            Assert.True(expected.SequenceEqual(actual));
        }

        [Fact]
        public void NullIsEmptyEnumerable()
        {
            string input = null;
            var myString = input.AsEnumerable();
            Assert.Empty(myString);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("apples")]
        [InlineData("tests")]
        public void NonNullIsExpected(string expected)
        {
            var emptyExpected = "None";
            var actual = expected.AsEnumerable();
            Assert.Equal(expected ?? emptyExpected, actual.DefaultIfEmpty(emptyExpected).Single());
        }

        [Fact]
        public async Task IterAsyncOrderedIsCorrect()
        {
            var fixture = new Fixture();
            var expecteds = new HashSet<string>(fixture.CreateMany<string>(100));
            var runcount = 0;
            Func<string, Task> act = async x =>
            {
                Assert.Contains(x, expecteds);
                runcount++;
                await Task.CompletedTask;
            };
            await expecteds.IterAsync(act);
            Assert.Equal(runcount, expecteds.Count);
        }

        [Fact]
        public async Task IterAsyncIndexedOrderedIsCorrect()
        {
            var fixture = new Fixture();
            var expecteds = new HashSet<string>(fixture.CreateMany<string>(100));
            var runcount = -1;
            Func<string, int, Task> act = async (x, y) =>
            {
                Assert.Contains(x, expecteds);
                runcount = y;
                await Task.CompletedTask;
            };
            await expecteds.IterAsync(act);
            Assert.Equal(runcount, expecteds.Count - 1);
        }

        [Fact]
        public async Task NonGenericIterAsyncOrderedIsCorrect()
        {
            var fixture = new Fixture();
            var expecteds = (IEnumerable)new HashSet<string>(fixture.CreateMany<string>(100));
            var runcount = 0;
            Func<object, Task> act = async x =>
            {
                Assert.Contains(x, expecteds.Cast<object>());
                runcount++;
                await Task.CompletedTask;
            };
            await expecteds.IterAsync(act);
            Assert.Equal(runcount, expecteds.Cast<object>().Count());
        }

        [Fact]
        public async Task NonGenericIterAsyncIndexedOrderedIsCorrect()
        {
            var fixture = new Fixture();
            var expecteds = (IEnumerable)new HashSet<string>(fixture.CreateMany<string>(100));
            var runcount = -1;
            Func<object, int, Task> act = async (x, y) =>
            {
                Assert.Contains(x, expecteds.Cast<object>());
                runcount = y;
                await Task.CompletedTask;
            };
            await expecteds.IterAsync(act);
            Assert.Equal(runcount, expecteds.Cast<object>().Count() - 1);
        }

        [Fact]
        public void NonGenericIterIsCorrect()
        {
            var fixture = new Fixture();
            var expecteds = (IEnumerable)new HashSet<string>(fixture.CreateMany<string>(100));
            var runcount = 0;
            Action<object> act = x =>
            {
                Assert.Contains(x, expecteds.Cast<object>());
                runcount++;
            };
            expecteds.Iter(act);
            Assert.Equal(runcount, expecteds.Cast<object>().Count());
        }

        [Fact]
        public void NonGenericIterIndexedIsCorrect()
        {
            var fixture = new Fixture();
            var expecteds = (IEnumerable)new HashSet<string>(fixture.CreateMany<string>(100));
            var runcount = -1;
            Action<object, int> act = (x, y) =>
            {
                Assert.Contains(x, expecteds.Cast<object>());
                runcount = y;
            };
            expecteds.Iter(act);
            Assert.Equal(runcount, expecteds.Cast<object>().Count() - 1);
        }

        [Fact]
        public void BasicIterIsCorrect()
        {
            var fixture = new Fixture();
            var expecteds = new HashSet<string>(fixture.CreateMany<string>(100));
            var runcount = 0;
            Action<string> act = x =>
            {
                Assert.Contains(x, expecteds.Cast<object>());
                runcount++;
            };
            expecteds.Iter(act);
            Assert.Equal(runcount, expecteds.Count);
        }

        [Fact]
        public void BasicIterIndexedIsCorrect()
        {
            var fixture = new Fixture();
            var expecteds = new HashSet<string>(fixture.CreateMany<string>(100));
            var runcount = -1;
            Action<string, int> act = (x, y) =>
            {
                Assert.Contains(x, expecteds);
                runcount = y;
            };
            expecteds.Iter(act);
            Assert.Equal(runcount, expecteds.Count - 1);
        }

        [Fact]
        public void RandomElementPicksFromKnownList()
        {
            var fixture = new Fixture();
            var knowns = new HashSet<string>(fixture.CreateMany<string>(100));
            var actual = knowns.RandomElement();
            Assert.Contains(actual, knowns);
        }

        [Fact]
        public void RandomElementUsingPicksSameFromList()
        {
            var fixture = new Fixture();
            var seed = fixture.Create<int>();
            var random1 = new Random(seed);
            var random2 = new Random(seed);
            Assert.Equal(random1.Next(), random2.Next());
            var knowns = new HashSet<string>(fixture.CreateMany<string>(100));
            Assert.Equal(
                knowns.RandomElementUsing(random1),
                knowns.RandomElementUsing(random2));
        }
    }
}