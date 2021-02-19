using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Xunit;
using Xunit.Sdk;
using static LanguagePatternsAndExtensions.Option<string>;

namespace LanguagePatternsAndExtensions.Tests
{
    public class TestClass
    {
        public string Whatever { get; set; }
    }

    public class OptionTests
    {

        [Fact]
        public void OptionUsesAreCorrect()
        {
            var fixture = new Fixture();
            var nonnullstring = fixture.Create<string>();
            var nullstring = (string)null;
            var sut = nonnullstring.ToOption();
            var outcome = sut.Match(
                "", x => x);
            var apples = "apples".ToOption();
            Assert.Equal("apples", apples.GetValue(x => x));
            Assert.Throws<NullReferenceException>(() =>
            {
                string empty = null;
                var foo = empty.ToOption();
                var fail = foo.GetValue(x => x.Length);
            });
            Assert.Equal(nonnullstring, outcome);
            var sut2 = nullstring.ToOption();
            Assert.Equal(None(), sut2);
            var sut3 = Some(nonnullstring);
            Assert.Equal(sut, sut3);
            Assert.Equal(Option<int>.None(), Option<int>.None());
            Assert.Equal(Some(nullstring), None());
            Assert.Equal(None(), None());
            Assert.Equal(Option<IEnumerable<string>>.None(), ((IEnumerable<string>)null).ToOption());
            Assert.NotEqual("test".ToOption(), string.Empty.ToOption());
            Assert.Equal("test".ToOption(), "test".ToOption());
            Assert.Equal("test".ToOption(), Some("test"));
        }

        [Theory, Gen]
        public void TwoSomeOptionsSameValueAreEqual(
            string foo)
        {
            var optionOne = Some(foo);
            var optionTwo = Some(foo);

            Assert.Equal(optionOne, optionTwo);
        }

        [Theory, Gen]
        public void TwoSameValuesButStoredSeparatelyOptionsAreEqual(
            int foo,
            int goo)
        {
            foo = Math.Abs(foo);
            goo = Math.Abs(goo);
            goo = goo + (foo - goo);
            var optionOne = foo.ToOption();
            var optionTwo = goo.ToOption();

            Assert.Equal(optionOne, optionTwo);
        }

        [Theory, Gen]
        public void NoneIsNotSome(
            int foo)
        {
            var testone = foo.ToOption();
            var testtwo = Option<int>.None();

            Assert.NotEqual(testone, testtwo);
        }

        [Theory, Gen]
        public void NoneIsNoneSetCorrectly()
        {
            var foo = Option<int>.None();

            Assert.True(foo.IsNone);
            Assert.False(foo.IsSome);
        }

        [Theory, Gen]
        public void CanUseParsing(
            TestClass input)
        {
            var dictionary = new ConcurrentDictionary<int, TestClass>();
            dictionary.AddOrUpdate(4, x => input, (x, y) => input);
            Option<TestClass> final;
            if (dictionary.TryGetValue(4, out TestClass found))
            {
                final = Option<TestClass>.Some(found);
            }
            else
            {
                final = Option<TestClass>.None();
            }
            Assert.True(final.IsSome);
            Assert.False(final.IsNone);
        }

        [Theory, Gen]
        public void CanChainSelectManyQueryForm(
            string one,
            string two,
            string three)
        {
            var a = one.ToOption();
            var b = two.ToOption();
            var c = three.ToOption();

            var actual =
                from aa in a
                from bb in b
                from cc in c
                select aa + bb + cc;

            Assert.Equal(one + two + three, actual.GetValue(x => x));
        }

        [Theory, Gen]
        public void CanChainSelectManyWithNullQueryForm(
            string one,
            string two,
            string three)
        {
            three = null;
            var a = one.ToOption();
            var b = two.ToOption();
            var c = three.ToOption();

            var actual =
                from aa in a
                from bb in b
                from cc in c
                select aa + bb + cc;

            Assert.Equal(None(), actual);
        }

        [Theory, Gen]
        public void CanChainSelectMany(
            string one,
            string two,
            string three)
        {
            var a = one.ToOption();
            var b = two.ToOption();
            var c = three.ToOption();

            var actual = a
                .SelectMany(x =>
                    b.SelectMany(y =>
                        c.SelectMany(z =>
                            (x + y + z).ToOption())));

            Assert.Equal(one + two + three, actual.GetValue(x => x));
        }

        [Theory, Gen]
        public void CanChainSelectManyWithNull(
            string one,
            string two,
            string three)
        {
            three = null;
            var a = one.ToOption();
            var b = two.ToOption();
            var c = three.ToOption();

            var actual = a
                .SelectMany(x =>
                    b.SelectMany(y =>
                        c.SelectMany(z =>
                            (x + y + z).ToOption())));

            Assert.Equal(None(), actual);
        }

        [Theory, Gen]
        public void CanDealWithMixedTypes(
            int one,
            string two,
            TestClass tclass)
        {
            var a = one.ToOption();
            var b = two.ToOption();
            var c = tclass.ToOption();

            var actual =
                from aa in a
                from bb in b
                from cc in c
                select new TestClass { Whatever = aa + bb };

            Assert.Equal(one + two, actual.GetValue(x => x).Whatever);
        }

        [Fact]
        public void NullableTypesPerformCorrectly()
        {
            Nullable<int> nullint = null;
            Nullable<int> nonnullint = 42;
            Nullable<int> zeroint = 0;

            Assert.Equal(Option<int>.None(), nullint.ToOption());
            Assert.Equal(nonnullint, nonnullint.Value.ToOption().GetValue(x => x));
            Assert.Equal(zeroint, zeroint.Value.ToOption().GetValue(x => x));
        }
    }
}