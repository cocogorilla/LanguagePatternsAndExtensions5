using Moq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace LanguagePatternsAndExtensions.Tests;
public class ExampleTests
{
    [Theory, Gen]
    public async Task OptionsInsteadOfNullChecking()
    {
        var potentialValues = new[] { "This is a nice string", null };

        // This is a simple example of using Option to avoid null checking
        // .ToOption is SAFE
        var values = potentialValues.Select(x => x.ToOption());

        // From here on out, you don't have to null check or wonder what the type of the value is
        foreach (var value in values)
        {
            var actual = value.Match(
                "I got nothing",
                x => $"I got {x}");
            Assert.Contains(actual, new[] { "I got nothing", "I got This is a nice string" });
        }
    }

    [Theory, Gen]
    public async Task TwoOptionsNamedParameters()
    {
        var answer = 42.ToOption();

        string actualWithStaticValue = answer.Match(
            some: x => $"The answer is: {x}",
            nothing: "I don't know the answer");

        string actualWithFunction = answer.Match(
            some: x => $"The answer is: {x}",
            none: () => "I don't know the answer");

        Assert.Equal(actualWithStaticValue, actualWithFunction);
    }

    [Theory, Gen]
    public async Task TheUtilityOfOptionTypes_AndNamedParametersForExpressiveCode(
        string inputString,
        string expected,
        Mock<IDemoInterface> mock)
    {
        // mock to randomly present a value or not
        mock.Setup(x => x.Translate(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
            new Random().Next() % 2 == 0 ? Option<string>.Some(expected) : Option<string>.None());

        var result = mock.Object.Translate(inputString, "en-US");

        var actual = result.Match(
            some: translation => $"I got a translation: {translation}",
            none: () => "I wasn't able to translate");

        if (result.IsSome)
            Assert.Contains(expected, actual);
        else
            Assert.Contains("I wasn't able to translate", actual);
    }

    [Theory, Gen]
    public async Task BasicExampleOfOutcomeSuccess(
        int theCount,
        Mock<IDemoInterface> mock)
    {
        mock.Setup(x => x.GetCountOfUsers()).ReturnsAsync(Success.Of(theCount));

        var result = await mock.Object.GetCountOfUsers();

        Assert.True(result.Succeeded);
        var actual = result.GetValue(x => $"I counted {theCount} users");
        Assert.Contains(theCount.ToString(), actual);
    }

    [Theory, Gen]
    public async Task BasicExampleOfOutcomeFailure(
        int theCount,
        string error,
        Mock<IDemoInterface> mock)
    {
        mock.Setup(x => x.GetCountOfUsers()).ReturnsAsync(Failure.Nok<int>(error));

        var result = await mock.Object.GetCountOfUsers();

        Assert.False(result.Succeeded);
        var actual = result.ErrorMessage;
        Assert.Equal(error, actual);
    }

    [Theory, Gen]
    public async Task BasicExampleUsingTraverse(
        int theCount,
        string error,
        Mock<IDemoInterface> mock)
    {
        var random = new Random().Next();
        mock.Setup(x => x.GetCountOfUsers()).ReturnsAsync(random % 2 == 0
            ? Success.Of(theCount)
            : Failure.Nok<int>(error));

        var result = await mock.Object.GetCountOfUsers();

        var actual = result.Traverse(
            x => $"I counted {theCount} users",
            x => $"I failed with {error}");

        if (result.Succeeded)
            Assert.Contains(theCount.ToString(), actual);
        else
            Assert.Contains(error, actual);
    }

    [Theory, Gen]
    public async Task UtilityOfComposingOperations(
        int count1,
        int count2,
        int count3,
        Mock<IDemoInterface> mock1,
        Mock<IDemoInterface> mock2,
        Mock<IDemoInterface> mock3)
    {
        mock1.Setup(x => x.GetCountOfUsers()).ReturnsAsync(Success.Of(count1));
        mock2.Setup(x => x.GetCountOfUsers()).ReturnsAsync(Success.Of(count2));
        mock3.Setup(x => x.GetCountOfUsers()).ReturnsAsync(Success.Of(count3));

        var result = await (from a in mock1.Object.GetCountOfUsers()
                            from b in mock2.Object.GetCountOfUsers()
                            from c in mock3.Object.GetCountOfUsers()
                            select (long)a + (long)b + (long)c);

        Assert.True(result.Succeeded);
        var actual = result.GetValue(x => $"I counted {x} users");
        Assert.Contains((count1 + count2 + count3).ToString(), actual);
    }

    [Theory, Gen]
    public async Task UtilityOfGettingErrorsFromComposedOperations(
        int count1,
        int count2,
        int count3,
        string errorMessage,
        Mock<IDemoInterface> mock1,
        Mock<IDemoInterface> mock2,
        Mock<IDemoInterface> mock3)
    {
        mock1.Setup(x => x.GetCountOfUsers()).ReturnsAsync(Success.Of(count1));
        mock2.Setup(x => x.GetCountOfUsers()).ReturnsAsync(Success.Of(count2));
        mock3.Setup(x => x.GetCountOfUsers()).ReturnsAsync(Failure.Nok<int>(errorMessage));

        var result = await (from a in mock1.Object.GetCountOfUsers()
                            from b in mock2.Object.GetCountOfUsers()
                            from c in mock3.Object.GetCountOfUsers()
                            select (long)a + (long)b + (long)c);

        Assert.False(result.Succeeded);
        var actual = result.ErrorMessage;
        Assert.Equal(errorMessage, actual);
    }

    [Theory, Gen]
    // If the user is not found we return a 404
    // If the user is not an admin we return a 401
    // If the user is an admin we return a 200 with the user count
    // If any operation fails we return a 500 with that error
    public async Task AdvancedPuttingItAllTogetherUsingExpressionSyntax(
        int userId,
        int count,
        string userName,
        Mock<IDemoInterface> mock)
    {
        SetupMocks(userId, count, userName, mock);

        var actual = await (
            from user in mock.Object.GetUserName(userId)
            from isAdmin in mock.Object.UserIsAdmin(userName)
            from userCount in mock.Object.GetCountOfUsers()
            select user.IsNone
                ? new ExampleWebResult(HttpStatusCode.NotFound, "unable to locate user")
                : !isAdmin
                    ? new ExampleWebResult(HttpStatusCode.Unauthorized, "user was not authorized to count system users")
                    : new ExampleWebResult(HttpStatusCode.OK, $"the total users in system are {userCount}"));

        var finalReturn =
            actual.Traverse(
                result =>
                    result,
                error =>
                    new ExampleWebResult(HttpStatusCode.InternalServerError, error));

        CheckResults(count, finalReturn);
    }

    [Theory, Gen]
    // If the user is not found we return a 404
    // If the user is not an admin we return a 401
    // If the user is an admin we return a 200 with the user count
    // If any operation fails we return a 500 with that error
    public async Task AdvancedPuttingItAllTogetherWithPatternMatching(
        int userId,
        int count,
        string userName,
        Mock<IDemoInterface> mock)
    {
        SetupMocks(userId, count, userName, mock);

        var actual = await (
            from user in mock.Object.GetUserName(userId)
            from isAdmin in mock.Object.UserIsAdmin(userName)
            from userCount in mock.Object.GetCountOfUsers()
            select (user, isAdmin) switch
            {
                ((true, _), true) =>
                    new ExampleWebResult(HttpStatusCode.OK, $"the total users in system are {userCount}"),
                ((true, _), false) =>
                    new ExampleWebResult(HttpStatusCode.Unauthorized, "user was not authorized to count system users"),
                ((false, _), _) => new ExampleWebResult(HttpStatusCode.NotFound, "unable to locate user")
            });

        var finalReturn =
            actual.Traverse(
                result =>
                    result,
                error =>
                    new ExampleWebResult(HttpStatusCode.InternalServerError, error));

        CheckResults(count, finalReturn);
    }

    private static void CheckResults(int count, ExampleWebResult finalReturn)
    {
        if (finalReturn.code == HttpStatusCode.NotFound)
        {
            Assert.Equal("unable to locate user", finalReturn.result);
        }
        else if (finalReturn.code == HttpStatusCode.Unauthorized)
        {
            Assert.Equal("user was not authorized to count system users", finalReturn.result);
        }
        else if (finalReturn.code == HttpStatusCode.OK)
        {
            Assert.Contains($"the total users in system are {count}", finalReturn.result);
        }
        else
        {
            Assert.Equal(HttpStatusCode.InternalServerError, finalReturn.code);
            Assert.Contains("database failure to", finalReturn.result);
        }
    }

    private static void SetupMocks(int userId, int count, string userName, Mock<IDemoInterface> mock)
    {
        var random = () => new Random().Next() % 2 == 0;
        mock.Setup(x => x.GetUserName(userId)).ReturnsAsync(random() ? Success.Of(random() ? userName : Option<string>.None()) : Failure.Nok<Option<string>>("database failure to get user"));
        mock.Setup(x => x.UserIsAdmin(userName)).ReturnsAsync(random() ? Success.Of(true) : Failure.Nok<bool>("database failure to determine rights"));
        mock.Setup(x => x.GetCountOfUsers()).ReturnsAsync(random() ? Success.Of(count) : Failure.Nok<int>("database failure to count users"));
    }
    public interface IDemoInterface
    {
        Option<string> Translate(string inputString, string toLocale);
        Task<Outcome<int>> GetCountOfUsers();
        Task<Outcome<bool>> UserIsAdmin(string userName);
        Task<Outcome<Option<string>>> GetUserName(int userId);
    }

    public record ExampleWebResult(HttpStatusCode code, string result);
}