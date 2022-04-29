using System;
using AutoFixture;
using AutoFixture.Idioms;
using Xunit;

namespace LanguagePatternsAndExtensions.Tests
{
    public class OutcomeTests
    {
        [Theory, Gen]
        public void SuccessContainsExpectedValueForQuery(
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

        [Theory, Gen]
        public void FailureMessageIsExpectedForQuery(
            string customError)
        {
            var sut = Failure.Nok<int>(customError);
            var result = sut.Traverse(x => throw new Exception("should not be in this case"),
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
            sut.Traverse(x => throw new Exception("should not be in this case"),
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
            var a = new Outcome<int>(fail);

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

            Assert.IsType<OutcomeWasNotSuccessException>(actual);
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

            Assert.IsType<OutcomeWasNotFailureException>(actual);
        }

        [Theory, Gen]
        public void SuccessIsGuarded(
            GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(Success).GetMethod(nameof(Success.Of)));
        }

        [Theory, Gen]
        public void FailureIsGuarded(
            GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(Failure).GetMethod(nameof(Failure.Nok)));
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
            expected.Traverse(x => { }, x =>
            {
                actual.Traverse(y => { }, y =>
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
        public void HashCodeEqualityComparisonForSuccessIsCorrect(string value)
        {
            var a = Success.Of(value);
            var b = Success.Of(value);
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
        public void HashCodeEqualityComparisonForFailureIsCorrect(string error)
        {
            var a = Failure.Nok<string>(error);
            var b = Failure.Nok<string>(error);
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
        public void HashCodeEqualityComparisonOnErrorMessageForFailureIsCorrect(string error, string error2)
        {
            var a = Failure.Nok<decimal>(error);
            var b = Failure.Nok<decimal>(error2);
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
        public void OutcomeEmptyEqualitySuccessIsCorrect()
        {
            var oa = Success.Ok();
            var ob = Success.Ok();
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

            actual.Traverse(
                x => Assert.Equal(one + two, x.Whatever),
                x => throw new Exception("should not get here"));
        }
    }
}
