using AutoFixture;
using AutoFixture.Idioms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace LanguagePatternsAndExtensions.Tests;

public class OutcomeTests
{
    [Theory, Gen]
    public void SuccessContainsExpectedValueForQuery(
        Guid expected,
        string successValue)
    {
        var sut = Success.Of(expected);
        var result = sut.Match(x =>
        {
            Assert.Equal(expected, x);
            return successValue;
        }, x => throw new Exception("should not be in this case"));
        Assert.Equal(result, successValue);
    }

    [Theory, Gen]
    public void SuccessContainsExpectedValueForQueryTraverse(
        Guid expected,
        string successValue)
    {
        var sut = Success.Of(expected);
        var result = sut.Traverse(x =>
        {
            Assert.Equal(expected, x);
            return successValue;
        }, x => throw new Exception("should not be in this case"));
        Assert.Equal(result, successValue);
    }

    [Theory, Gen]
    public void SuccessContainsExpectedValueForCommand(
        Guid expected)
    {
        var sut = Success.Of(expected);
        sut.Match(x =>
        {
            Assert.Equal(expected, x);
        }, x => throw new Exception("should not be in this case"));
    }

    [Theory, Gen]
    public void SuccessContainsExpectedValueForCommandTraverse(
        Guid expected)
    {
        var sut = Success.Of(expected);
        sut.Traverse(x =>
        {
            Assert.Equal(expected, x);
        }, x => throw new Exception("should not be in this case"));
    }

    [Fact]
    public void SuccessSucceeded()
    {
        var sut = Success.Of("anything");
        Assert.True(sut.Succeeded);
        Assert.Empty(sut.ErrorMessage);
    }

    [Fact]
    public void OverloadResolutionSuccessFailure()
    {
        var inputString = "this is a successful value";

        var actual = Success.Of(inputString);

        Assert.True(actual.Succeeded);
    }

    [Fact]
    public void FailureMessageCannotBeEmptyOrWhiteSpace()
    {
        string[] inputs = ["", "  "];

        foreach (var input in inputs)
        {
            Assert.Throws<ArgumentException>(() => Failure.Nok<string>(input));
        }
    }

    [Fact]
    public void SuccessBitIsCorrect()
    {
        var input = Outcome<int>.Success(42);
        Assert.True(input.IsSuccess);
        Assert.False(input.IsFailure);
    }

    [Fact]
    public void FailureBitIsCorrect()
    {
        var input = Outcome<int>.Failure("test");
        Assert.False(input.IsSuccess);
        Assert.True(input.IsFailure);
    }

    [Theory, Gen]
    public void FailureMessageIsExpectedForQuery(
        string customError)
    {
        var sut = Failure.Nok<int>(customError);
        var result = sut.Match(x => throw new Exception("should not be in this case"),
            (x) =>
            {
                Assert.Equal(customError, x);
                return customError;
            });
        Assert.Equal(customError, result);
    }

    [Theory, Gen]
    public void FailureMessageIsExpectedForCommand(
        string expected)
    {
        var sut = Failure.Nok<int>(expected);
        sut.Match(x => throw new Exception("should not be in this case"),
            (x) =>
            {
                Assert.Equal(expected, x);
            });
    }

    [Fact]
    public void FailureDidNotSucceed()
    {
        var sut = Failure.Nok<int>("some message");
        Assert.False(sut.Succeeded);
    }

    [Theory, Gen]
    public void CanExitEarlyForSuccess(
        string fail,
        TestClass expected)
    {
        var sut = Success.Of(expected);

        TestClass actual;

        if (sut.Succeeded)
            actual = sut.GetValue(x => x);
        else
            actual = null;

        Assert.Equal(expected, actual);
    }

    [Theory, Gen]
    public void CanExitEarlyForFailure(
        string fail)
    {
        var a = Outcome<int>.Failure(fail);

        var actual = a.GetError();

        Assert.Equal(fail, actual);
    }

    [Theory, Gen]
    public void GettingSuccessFromErrorIsUnsafeAndThrows(
        string fail)
    {
        var actual = Record.Exception(() =>
        {
            var sut = Failure.Nok<int>(fail);
            var result = sut.GetValue(x => x);
        });

        Assert.IsType<InvalidOperationException>(actual);
    }

    [Theory, Gen]
    public void GettingErrorFromSuccessIsUnsafeAndThrows(
        string success)
    {
        var actual = Record.Exception(() =>
        {
            var sut = Success.Of(success);
            var result = sut.GetError();
        });

        Assert.IsType<InvalidOperationException>(actual);
    }

    [Theory, Gen]
    public void OutcomesAreGuarded(
        GuardClauseAssertion assertion)
    {
        assertion.Verify(typeof(Outcome<string>).GetConstructors());
    }

    [Fact]
    public void SuccessOfUnitsAreEqual()
    {
        var outcome1 = Success.Of(Unit.Default);
        var outcome2 = Success.Of(Unit.Default);
        Assert.Equal(outcome1, outcome2);
    }

    [Theory, Gen]
    public void FailureOfUnitsAreEqual(
        string message)
    {
        var outcome1 = Failure.Nok<Unit>(message);
        var outcome2 = Failure.Nok<Unit>(message);
        Assert.Equal(outcome1, outcome2);
    }

    [Fact]
    public void SuccessfulUnitIsCorrect()
    {
        var expected = Success.Of(Unit.Default);
        var actual = Success.Ok();
        Assert.Equal(expected, actual);
    }

    [Theory, Gen]
    public void FailureUnitIsCorrect(string expectedMessage)
    {
        var expected = Failure.Nok<Unit>(expectedMessage);
        var actual = Failure.Nok<Unit>(expectedMessage);
        expected.Match(x => { }, x =>
        {
            actual.Match(y => { }, y =>
            {
                Assert.Equal(x, y);
            });
        });
    }

    [Theory, Gen]
    public void OutcomeEqualitySuccessIsCorrect(string value)
    {
        var oa = Success.Of(value);
        var ob = Success.Of(value);
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeEqualitySuccessGenericIsCorrect(string value)
    {
        var oa = Success.Of<string, string>(value);
        var ob = Success.Of<string, string>(value);
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Theory, Gen]
    public void HashCodeEqualityComparisonForSuccessIsCorrect(string value)
    {
        var a = Success.Of(value);
        var b = Success.Of(value);
        Assert.True(a.GetHashCode() == b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeEqualityComparisonForSuccessGenericIsCorrect(string value)
    {
        var a = Success.Of<string, string>(value);
        var b = Success.Of<string, string>(value);
        Assert.True(a.GetHashCode() == b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeInEqualityComparisonForSuccessIsCorrect(string value, string value2)
    {
        var a = Success.Of(value);
        var b = Success.Of(value2);
        Assert.False(a.GetHashCode() == b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeInEqualityComparisonForSuccessGenericIsCorrect(string value, string value2)
    {
        var a = Success.Of<string, string>(value);
        var b = Success.Of<string, string>(value2);
        Assert.False(a.GetHashCode() == b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeEqualityComparisonForFailureIsCorrect(string error)
    {
        var a = Failure.Nok<string>(error);
        var b = Failure.Nok<string>(error);
        Assert.True(a.GetHashCode() == b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeEqualityComparisonGenericForFailureIsCorrect(string error)
    {
        var a = Failure.Nok<string, string>(error);
        var b = Failure.Nok<string, string>(error);
        Assert.True(a.GetHashCode() == b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeInEqualityComparisonForFailureIsCorrect(string error, string error2)
    {
        var a = Failure.Nok<int>(error);
        var b = Failure.Nok<int>(error2);
        Assert.False(a.GetHashCode() == b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeInEqualityComparisonGenericForFailureIsCorrect(string error, string error2)
    {
        var a = Failure.Nok<int, string>(error);
        var b = Failure.Nok<int, string>(error2);
        Assert.False(a.GetHashCode() == b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeEqualityComparisonOnErrorMessageForFailureIsCorrect(string error, string error2)
    {
        var a = Failure.Nok<decimal>(error);
        var b = Failure.Nok<decimal>(error2);
        Assert.True(a.GetHashCode() != b.GetHashCode());
    }

    [Theory, Gen]
    public void HashCodeEqualityComparisonGenericOnErrorMessageForFailureIsCorrect(string error, string error2)
    {
        var a = Failure.Nok<decimal, string>(error);
        var b = Failure.Nok<decimal, string>(error2);
        Assert.True(a.GetHashCode() != b.GetHashCode());
    }

    [Fact]
    public void OutcomeUnitEqualitySuccessIsCorrect()
    {
        var oa = Success.Of(Unit.Default);
        var ob = Success.Of(Unit.Default);
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Fact]
    public void OutcomeUnitEqualityGenericSuccessIsCorrect()
    {
        var oa = Success.Of<Unit, string>(Unit.Default);
        var ob = Success.Of<Unit, string>(Unit.Default);
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Fact]
    public void OutcomeEmptyEqualitySuccessIsCorrect()
    {
        var oa = Success.Ok();
        var ob = Success.Ok();
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Fact]
    public void OutcomeEmptyEqualityGenericSuccessIsCorrect()
    {
        var oa = Success.Ok<string>();
        var ob = Success.Ok<string>();
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeEqualityFailureIsCorrect(string value, string errorMessage)
    {
        var oa = Failure.Nok<Guid>(errorMessage);
        var ob = Failure.Nok<Guid>(errorMessage);
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeEqualityGenericFailureIsCorrect(string value, int errorMessage)
    {
        var oa = Failure.Nok<Guid, int>(errorMessage);
        var ob = Failure.Nok<Guid, int>(errorMessage);
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeUnitEqualityFailureIsCorrect(string errorMessage)
    {
        var oa = Failure.Nok<double?>(errorMessage);
        var ob = Failure.Nok<double?>(errorMessage);
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeEmptyEqualityFailureIsCorrect(string errorMessage)
    {
        var oa = Failure.Nok<Fixture>(errorMessage);
        var ob = Failure.Nok<Fixture>(errorMessage);
        Assert.True(oa == ob);
        Assert.True(oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeInequalitySuccessIsCorrect(string value)
    {
        var oa = Success.Of(value);
        var ob = Success.Of(value);
        Assert.False(oa != ob);
        Assert.False(!oa.Equals(ob));
    }

    [Fact]
    public void OutcomeUnitInequalitySuccessIsCorrect()
    {
        var oa = Success.Of(Unit.Default);
        var ob = Success.Of(Unit.Default);
        Assert.False(oa != ob);
        Assert.False(!oa.Equals(ob));
    }

    [Fact]
    public void OutcomeEmptyInequalitySuccessIsCorrect()
    {
        var oa = Success.Ok();
        var ob = Success.Ok();
        Assert.False(oa != ob);
        Assert.False(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeInequalityFailureIsCorrect(string value, string errorMessage)
    {
        var oa = Failure.Nok<string>(errorMessage);
        var ob = Failure.Nok<string>(errorMessage);
        Assert.False(oa != ob);
        Assert.False(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeUnitInequalityFailureIsCorrect(string errorMessage)
    {
        var oa = Failure.Nok<Unit>(errorMessage);
        var ob = Failure.Nok<Unit>(errorMessage);
        Assert.False(oa != ob);
        Assert.False(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeEmptyInequalityFailureIsCorrect(string errorMessage)
    {
        var oa = Failure.Nok<Unit>(errorMessage);
        var ob = Failure.Nok<Unit>(errorMessage);
        Assert.False(oa != ob);
        Assert.False(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeEqualityDifferentSuccessIsCorrect(string value, string value2)
    {
        var oa = Success.Of(value);
        var ob = Success.Of(value2);
        Assert.True(oa != ob);
        Assert.True(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void SuccessAndFailureAreNotEqual(string error)
    {
        var a = Success.Ok();
        var b = Failure.Nok<Unit>(error);

        Assert.True(a != b);
        Assert.True(!a.Equals(b));
    }

    [Theory, Gen]
    public void AnonymousObjectComparisonFails(string error)
    {
        Assert.True(!Success.Ok().Equals(new { }));
        Assert.True(!Failure.Nok<Unit>(error).Equals(new { }));
    }

    [Theory, Gen]
    public void OutcomeUnitEqualityDifferentFailureIsCorrect(string errorMessage, string errorMessage2)
    {
        var oa = Failure.Nok<Unit>(errorMessage);
        var ob = Failure.Nok<Unit>(errorMessage2);
        Assert.True(oa != ob);
        Assert.True(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeInequalityDifferentSuccessIsCorrect(string value, string value2)
    {
        var oa = Success.Of(value);
        var ob = Success.Of(value2);
        Assert.True(oa != ob);
        Assert.True(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeInequalityDifferentFailureIsCorrect(string errorMessage, string errorMessage2)
    {
        var oa = Failure.Nok<string>(errorMessage);
        var ob = Failure.Nok<string>(errorMessage);
        Assert.False(oa != ob);
        Assert.False(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void OutcomeUnitInequalityDifferentFailureIsCorrect(string errorMessage, string errorMessage2)
    {
        var oa = Failure.Nok<Unit>(errorMessage);
        var ob = Failure.Nok<Unit>(errorMessage2);
        Assert.True(oa != ob);
        Assert.True(!oa.Equals(ob));
    }

    [Theory, Gen]
    public void SuccessToStringIsCorrect(string value)
    {
        var sut = Success.Of(value);
        Assert.Equal($"Success: {value}", sut.ToString());
    }

    [Theory, Gen]
    public void FailureToStringIsCorrect(string errorMessage)
    {
        var sut = Failure.Nok<string>(errorMessage);
        Assert.Equal($"Failure: {errorMessage}", sut.ToString());
    }

    [Theory, Gen]
    public void SimpleSelectSuccess(
        int expected)
    {
        var a = Success.Of(expected);

        var actual = a.Select(x => x + 1);

        Assert.Equal(Success.Of(expected + 1), actual);
    }

    [Theory, Gen]
    public void SimpleSelectSuccessQueryForm(
        int expected)
    {
        var a = Success.Of(expected);

        var actual =
            from aa in a
            select aa + 1;

        Assert.Equal(Success.Of(expected + 1), actual);
    }

    [Theory, Gen]
    public void SimpleSelectFailureQueryForm(
        string expectedError)
    {
        var a = Failure.Nok<int>(expectedError);

        var actual =
            from aa in a
            select aa + 1;

        Assert.Equal(Failure.Nok<int>(expectedError), actual);
    }

    [Theory, Gen]
    public void CanChainSelectManyQueryForm(
        string one,
        string two,
        string three)
    {
        var a = Success.Of(one);
        var b = Success.Of(two);
        var c = Success.Of(three);

        var actual =
            from aa in a
            from bb in b
            from cc in c
            select aa + bb + cc;

        Assert.Equal(Success.Of(one + two + three), actual);
    }

    [Theory, Gen]
    public void CanChainSelectManyWithFailureQueryForm(
        string one,
        string two,
        string three)
    {
        var a = Success.Of(one);
        var c = Failure.Nok<string>(two);
        var b = Success.Of(three);

        var actual =
            from aa in a
            from bb in b
            from cc in c
            select aa + bb + cc;

        Assert.Equal(Failure.Nok<string>(two), actual);
    }

    [Theory, Gen]
    public void CanChainSelectMany(
        string one,
        string two,
        string three)
    {
        var a = Success.Of(one);
        var b = Success.Of(two);
        var c = Success.Of(three);

        var actual = a
            .SelectMany(x =>
                b.SelectMany(y =>
                    c.SelectMany(z =>
                        Success.Of(x + y + z))));

        Assert.Equal(Success.Of(one + two + three), actual);
    }

    [Theory, Gen]
    public void CanChainSelectManyWithFailure(
        string one,
        string two,
        string three)
    {
        var a = Success.Of(one);
        var b = Failure.Nok<string>(two);
        var c = Success.Of(three);

        var actual = a
            .SelectMany(x =>
                b.SelectMany(y =>
                    c.SelectMany(z =>
                        Success.Of(x + y + z))));

        Assert.Equal(Failure.Nok<string>(two), actual);
    }

    [Theory, Gen]
    public void CanDealWithMixedTypes(
        int one,
        string two,
        TestClass tclass)
    {
        var a = Success.Of(one);
        var b = Success.Of(two);
        var c = Success.Of(tclass);

        var actual =
            from aa in a
            from bb in b
            from cc in c
            select new TestClass { Whatever = aa + bb };

        actual.Match(
            x => Assert.Equal(one + two, x.Whatever),
            x => throw new Exception("should not get here"));
    }

    [Theory, Gen]
    public async Task CanSimpleSelectAsync(
        int expected)
    {
        var a = Task.FromResult(Success.Of(expected));

        var actual = await a.Select(x => x + 1);

        Assert.Equal(Success.Of(expected + 1), actual);
    }

    [Theory, Gen]
    public async Task CanChainSelectManyAsync(
        int one,
        int two)
    {
        var a = Task.FromResult(Success.Of(one));
        var b = Task.FromResult(Success.Of(two));

        var actual = await a
            .SelectMany(x =>
                b.SelectMany(y =>
                    Task.FromResult(Success.Of(x + y))));

        Assert.Equal(Success.Of(one + two), actual);
    }

    [Theory, Gen]
    public async Task CanChainSelectManyOverloadAsync(
        int one,
        int two)
    {
        var a = Task.FromResult(Success.Of(one));
        var b = Task.FromResult(Success.Of(two));
        var actual = await a
            .SelectMany(
                x => b,
                (x, y) => x + y);
        Assert.Equal(Success.Of(one + two), actual);
    }

    [Theory, Gen]
    public async Task CanGetFirstErrorAsync(
        int one,
        string two)
    {
        var a = Task.FromResult(Success.Of(one));
        var b = Task.FromResult(Failure.Nok<int>(two));

        var actual = await a
            .SelectMany(x =>
                b.SelectMany(y =>
                    Task.FromResult(Success.Of(x + y))));

        Assert.Equal(Failure.Nok<int>(two), actual);
    }

    [Theory, Gen]
    public async Task CanGetFirstErrorComprehensionAsync(
        int one,
        string expectedError,
        TestClass two)
    {
        var a = Task.FromResult(Success.Of(one));
        var b = Task.FromResult(Failure.Nok<TestClass>(expectedError));

        var actual =
            from aa in a
            from bb in b
            select new TestClass { Whatever = aa + bb.Whatever };

        Assert.Equal(Failure.Nok<TestClass>(expectedError), await actual);
    }

    [Theory, Gen]
    public async Task CanTraverseAsyncSuccess(
        int initial)
    {
        var outcome = Outcome<int>.Success(initial);
        var successFunc = async (int x) =>
        {
            await Task.Delay(10).ConfigureAwait(false);
            return x * 2;
        };
        var errorFunc = async (string error) =>
        {
            Assert.Fail("should not error");
            return default(int);
        };

        var result = await outcome.TraverseAsync(
            successFunc,
            errorFunc);

        Assert.Equal(initial * 2, result);
    }

    [Theory, Gen]
    public async Task CanMatchAsyncSuccess(
        int initial)
    {
        var outcome = Outcome<int>.Success(initial);
        var successFunc = async (int x) =>
        {
            await Task.Delay(10).ConfigureAwait(false);
            return x * 2;
        };
        var errorFunc = async (string error) =>
        {
            Assert.Fail("should not error");
            return default(int);
        };

        var result = await outcome.MatchAsync(
            successFunc,
            errorFunc);

        Assert.Equal(initial * 2, result);
    }

    [Theory, Gen]
    public async Task CanMatchAsyncFailure(
        string expectedError)
    {
        var outcome = Failure.Nok<int>(expectedError);
        var successFunc = async (int x) =>
        {
            Assert.Fail("should not succeed");
            return default(int);
        };
        var errorFunc = async (string error) =>
        {
            await Task.Delay(10).ConfigureAwait(false);
            return error.Length;
        };

        var result = await outcome.MatchAsync(
            successFunc,
            errorFunc);

        Assert.Equal(expectedError.Length, result);
    }

    [Theory, Gen]
    public void CanUseSuccessPathWithCustomError(
        string input,
        string expected,
        Mock<Func<string, Outcome<int, CustomError>>> op1,
        Mock<Func<string, Outcome<int, CustomError>>> op2,
        Mock<Func<string, Outcome<int, CustomError>>> op3,
        CustomError error)
    {
        op1.Setup(x => x(It.IsAny<string>())).Returns(Outcome<int, CustomError>.Success(1));
        op2.Setup(x => x(It.IsAny<string>())).Returns(Outcome<int, CustomError>.Success(2));
        op3.Setup(x => x(It.IsAny<string>())).Returns(Outcome<int, CustomError>.Success(3));

        var actual = from a in op1.Object(input)
                     from b in op2.Object(a.ToString())
                     from c in op3.Object(b.ToString())
                     select c;

        Assert.Equal(Success.Of<int, CustomError>(3), actual);
        op1.Verify(x => x(input), Times.Once);
        op2.Verify(x => x(1.ToString()), Times.Once);
        op3.Verify(x => x(2.ToString()), Times.Once);
    }

    [Theory, Gen]
    public void CanUseCustomErrorPath(
        string input,
        Mock<Func<string, Outcome<int, CustomError>>> op1,
        Mock<Func<string, Outcome<int, CustomError>>> op2,
        Mock<Func<string, Outcome<int, CustomError>>> op3,
        CustomError error)
    {
        op1.Setup(x => x(It.IsAny<string>())).Returns(Outcome<int, CustomError>.Success(1));
        op2.Setup(x => x(It.IsAny<string>())).Returns(Outcome<int, CustomError>.Failure(error));
        op3.Setup(x => x(It.IsAny<string>())).Returns(Outcome<int, CustomError>.Success(3));

        var actual = from a in op1.Object(input)
                     from b in op2.Object(a.ToString())
                     from c in op3.Object(b.ToString())
                     select c;

        Assert.Equal(Failure.Nok<int, CustomError>(error), actual);
        op1.Verify(x => x(input), Times.Once);
        op2.Verify(x => x(1.ToString()), Times.Once);
        op3.Verify(x => x(It.IsAny<string>()), Times.Never);
    }

    [Theory, Gen]
    public void CanCustomErrorPathString(
        string input,
        string error)
    {
        var actual = Failure.Nok<string, string>(error);

        Assert.Equal(error, actual.Match(x => "no match", x => x));
    }

    [Theory, Gen]
    public void TwoOutcomesEqualIsSuccess(
        TestClass input)
    {
        var input2 = input;
        var a = Outcome<TestClass, CustomError>.Success(input);
        var b = Outcome<TestClass, CustomError>.Success(input2);

        Assert.Equal(a, b);
    }

    [Theory, Gen]
    public void TwoOutcomesNotEqualIsSuccess(
        TestClass input,
        TestClass input2)
    {
        var a = Outcome<TestClass, CustomError>.Success(input);
        var b = Outcome<TestClass, CustomError>.Success(input2);

        Assert.NotEqual(a, b);
    }

    [Theory, Gen]
    public void TwoOutcomesFailureEqualIsSuccess(
        CustomError input)
    {
        var input2 = input;
        var a = Outcome<TestClass, CustomError>.Failure(input);
        var b = Outcome<TestClass, CustomError>.Failure(input2);

        Assert.Equal(a, b);
    }

    [Theory, Gen]
    public void TwoOutcomesFailureNotEqualIsSuccess(
        CustomError input,
        CustomError input2)
    {
        var a = Outcome<TestClass, CustomError>.Failure(input);
        var b = Outcome<TestClass, CustomError>.Failure(input2);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void MatchAndSuccessForUnit()
    {
        var actual = Success.Ok<string>();

        Assert.True(actual.Succeeded);
        Assert.True(actual.IsSuccess);
        Assert.False(actual.IsFailure);
    }

    [Theory, Gen]
    public void MatchAndFailForUnit(
        string error)
    {
        var actual = Failure.Nok<Unit>(error);

        Assert.False(actual.Succeeded);
        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
    }

    [Theory, Gen]
    public void ActionMatching(
        string expected)
    {
        var container = new List<string>();
        var sut = Outcome<string, CustomError>.Success(expected);

        sut.Match(
            x => container.Add(x),
            _ => Assert.Fail("not this one"));

        Assert.Equal(expected, container.Single());
    }

    [Theory, Gen]
    public async Task AsyncMatchingSuccess(
        string expected)
    {
        var successFunc = async (string x) => expected;
        var failFunc = async (string error) => "not expected";
        var input = Success.Of<string, string>("input");

        var actual = await input.MatchAsync(
            successFunc, failFunc);

        Assert.Equal(expected, actual);
    }

    [Theory, Gen]
    public async Task AsyncMatchingFailure(
        string expected)
    {
        var successFunc = async (string x) => "not expected";
        var failFunc = async (string error) => expected;
        var input = Failure.Nok<string, string>("input");

        var actual = await input.MatchAsync(
            successFunc, failFunc);

        Assert.Equal(expected, actual);
    }

    // Map tests (Outcome<TValue>)

    [Theory, Gen]
    public void MapTransformsSuccessValue(int input)
    {
        var sut = Success.Of(input);

        var actual = sut.Map(x => x * 2);

        Assert.Equal(Success.Of(input * 2), actual);
    }

    [Theory, Gen]
    public void MapPreservesFailure(string error)
    {
        var sut = Failure.Nok<int>(error);

        var actual = sut.Map(x => x * 2);

        Assert.Equal(Failure.Nok<int>(error), actual);
    }

    // Bind tests (Outcome<TValue>)

    [Theory, Gen]
    public void BindChainsSuccessToSuccess(int input)
    {
        var sut = Success.Of(input);

        var actual = sut.Bind(x => Success.Of(x.ToString()));

        Assert.Equal(Success.Of(input.ToString()), actual);
    }

    [Theory, Gen]
    public void BindChainsSuccessToFailure(int input, string error)
    {
        var sut = Success.Of(input);

        var actual = sut.Bind<int, string>(_ => Failure.Nok<string>(error));

        Assert.Equal(Failure.Nok<string>(error), actual);
    }

    [Theory, Gen]
    public void BindShortCircuitsOnFailure(string error)
    {
        var sut = Failure.Nok<int>(error);
        bool called = false;

        var actual = sut.Bind(x =>
        {
            called = true;
            return Success.Of(x.ToString());
        });

        Assert.False(called);
        Assert.Equal(Failure.Nok<string>(error), actual);
    }

    // Map tests (Outcome<TSuccess, TFailure>)

    [Theory, Gen]
    public void MapGenericTransformsSuccessValue(int input)
    {
        var sut = Outcome<int, CustomError>.Success(input);

        var actual = sut.Map(x => x.ToString());

        Assert.Equal(Success.Of<string, CustomError>(input.ToString()), actual);
    }

    [Theory, Gen]
    public void MapGenericPreservesFailure(CustomError error)
    {
        var sut = Outcome<int, CustomError>.Failure(error);

        var actual = sut.Map(x => x.ToString());

        Assert.Equal(Failure.Nok<string, CustomError>(error), actual);
    }

    // Bind tests (Outcome<TSuccess, TFailure>)

    [Theory, Gen]
    public void BindGenericChainsSuccessToSuccess(int input)
    {
        var sut = Outcome<int, CustomError>.Success(input);

        var actual = sut.Bind(x => Outcome<string, CustomError>.Success(x.ToString()));

        Assert.Equal(Success.Of<string, CustomError>(input.ToString()), actual);
    }

    [Theory, Gen]
    public void BindGenericChainsSuccessToFailure(int input, CustomError error)
    {
        var sut = Outcome<int, CustomError>.Success(input);

        var actual = sut.Bind<int, CustomError, string>(_ => Outcome<string, CustomError>.Failure(error));

        Assert.Equal(Failure.Nok<string, CustomError>(error), actual);
    }

    [Theory, Gen]
    public void BindGenericShortCircuitsOnFailure(CustomError error)
    {
        var sut = Outcome<int, CustomError>.Failure(error);
        bool called = false;

        var actual = sut.Bind(x =>
        {
            called = true;
            return Outcome<string, CustomError>.Success(x.ToString());
        });

        Assert.False(called);
        Assert.Equal(Failure.Nok<string, CustomError>(error), actual);
    }

    // Implicit conversion tests (Outcome<TValue>)

    [Theory, Gen]
    public void ImplicitConversionCreatesSuccess(int value)
    {
        Outcome<int> sut = value;

        Assert.True(sut.IsSuccess);
        Assert.Equal(value, sut.GetValue(x => x));
    }

    [Theory, Gen]
    public void ImplicitConversionEqualsExplicitSuccess(string value)
    {
        Outcome<string> implicit_ = value;
        var explicit_ = Success.Of(value);

        Assert.Equal(explicit_, implicit_);
    }

    [Fact]
    public void ImplicitConversionWidensToInterfaceType()
    {
        var concreteList = new List<int> { 1, 2, 3 };

        Outcome<IReadOnlyList<int>> sut = concreteList;

        Assert.True(sut.IsSuccess);
        sut.Match(
            x => Assert.Equal(3, x.Count),
            _ => Assert.Fail("should be success"));
    }

    [Fact]
    public void ImplicitConversionWorksInSwitchExpression()
    {
        var flag = true;

        Outcome<IReadOnlyList<int>> result = flag switch
        {
            true => new List<int> { 1, 2, 3 },
            false => new ReadOnlyCollection<int>([4, 5])
        };

        Assert.True(result.IsSuccess);
        result.Match(
            x => Assert.Equal(3, x.Count),
            _ => Assert.Fail("should be success"));
    }

    [Fact]
    public void ImplicitConversionWorksInTernary()
    {
        var useFirst = true;

        Outcome<IEnumerable<int>> result = useFirst
            ? new List<int> { 1 }
            : new HashSet<int> { 2, 3 };

        Assert.True(result.IsSuccess);
    }

    [Theory, Gen]
    public void ImplicitConversionPreservesHashCodeEquality(int value)
    {
        Outcome<int> implicit_ = value;
        var explicit_ = Success.Of(value);

        Assert.Equal(explicit_.GetHashCode(), implicit_.GetHashCode());
    }

    // Implicit conversion tests (Outcome<TSuccess, TFailure>)

    [Theory, Gen]
    public void ImplicitConversionGenericCreatesSuccess(int value)
    {
        Outcome<int, CustomError> sut = value;

        Assert.True(sut.IsSuccess);
        Assert.Equal(value, sut.Match(x => x, _ => throw new Exception("should be success")));
    }

    [Theory, Gen]
    public void ImplicitConversionGenericEqualsExplicitSuccess(string value)
    {
        Outcome<string, CustomError> implicit_ = value;
        var explicit_ = Success.Of<string, CustomError>(value);

        Assert.Equal(explicit_, implicit_);
    }

    [Fact]
    public void ImplicitConversionGenericWidensToInterfaceType()
    {
        var concreteList = new List<string> { "a", "b" };

        Outcome<IReadOnlyList<string>, CustomError> sut = concreteList;

        Assert.True(sut.IsSuccess);
        sut.Match(
            x => Assert.Equal(2, x.Count),
            _ => Assert.Fail("should be success"));
    }

    [Fact]
    public void ImplicitConversionGenericWorksInSwitchExpression()
    {
        var flag = true;

        Outcome<IReadOnlyList<int>, string> result = flag switch
        {
            true => new List<int> { 10, 20 },
            false => new ReadOnlyCollection<int>([30])
        };

        Assert.True(result.IsSuccess);
        result.Match(
            x => Assert.Equal(2, x.Count),
            _ => Assert.Fail("should be success"));
    }

    [Theory, Gen]
    public void FailureStillRequiresExplicitConstruction(string error)
    {
        var success = Success.Of(42);
        var failure = Failure.Nok<int>(error);

        Assert.True(success.IsSuccess);
        Assert.True(failure.IsFailure);
    }
}