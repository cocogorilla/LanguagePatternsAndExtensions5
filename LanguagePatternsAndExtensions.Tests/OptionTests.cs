using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AutoFixture;
using Xunit;
using static LanguagePatternsAndExtensions.Option<string>;

namespace LanguagePatternsAndExtensions.Tests;

public class TestClass
{
    public string Whatever { get; set; }
}

public class OptionTests
{
    [Fact]
    public void CanPatternMatchOnSome()
    {
        var nonNullInput = Guid.NewGuid().ToString();
        var some = nonNullInput.ToOption();

        var actual = some switch
        {
            (true, var value) => value,
            _ => throw new Exception("should not arrive")
        };

        Assert.Equal(nonNullInput, actual);
    }

    [Fact]
    public void CanPatternMatchOnEmptyNone()
    {
        var empty = ((string)null).ToOption();
        var actual = empty switch
        {
            (false, _) => "empty",
            _ => throw new Exception("should not arrive")
        };
        Assert.Equal("empty", actual);
    }

    [Fact]
    public void SomeIsTrue()
    {
        var nonNullInput = Guid.NewGuid().ToString();
        var some = nonNullInput.ToOption();
        if (some)
        {
            Assert.True(some.IsSome);
        }
        else
        {
            Assert.Fail("should not be none");
        }
    }

    [Fact]
    public void NoneIsFalse()
    {
        var empty = ((string)null).ToOption();
        if (empty)
        {
            Assert.Fail("should not be some");
        }
        else
        {
            Assert.True(empty.IsNone);
        }
    }

    [Fact]
    public void CanLiftTsIntoOptions()
    {
        var nonnullstring = Guid.NewGuid().ToString();
        var nullstring = (string)null;

        Option<string> some = nonnullstring;
        Option<string> none = nullstring;

        Assert.Equal(nonnullstring, some.GetValue(x => x));
        Assert.Equal(None(), none);
    }

    [Fact]
    public void OptionUsesAreCorrect()
    {
        var fixture = new Fixture();
        var nonnullstring = fixture.Create<string>();
        var nullstring = (string)null;
        var sut = nonnullstring.ToOption();
        var outcome = sut.Match(
            x => x, "");
        var outcomeNoneOverload = nullstring.ToOption().Match(
            x => x, () => "overloadednone");
        var outcomeSomeOverload = sut.Match(
            x => x, () => throw new Exception("should not arrive"));
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
        Assert.Equal("overloadednone", outcomeNoneOverload);
        Assert.Equal(nonnullstring, outcomeSomeOverload);
    }

    [Theory, Gen]
    public void CanExitEarlyForSuccess(
        TestClass expected)
    {
        var sut = expected.ToOption();

        TestClass actual;

        if (sut.IsSome)
            actual = sut.GetValue(x => x);
        else
            actual = null;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetValueIsUnsafeAndCanCatastrophicallyFail()
    {
        var a = ((string)null).ToOption();

        var actual = Record.Exception(() =>
        {
            var fail = a.GetValue(x => x.ToUpper());
        });

        Assert.IsType<NullReferenceException>(actual);
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

    [Fact]
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