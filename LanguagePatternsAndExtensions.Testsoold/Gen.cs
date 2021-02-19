using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace LanguagePatternsAndExtensions.Tests
{
    public class Gen : AutoDataAttribute
    {
        public Gen()
            : this(connectionString: null)
        { }
        public Gen(string connectionString = null) : base(() => new Fixture().Customize(
            new CompositeCustomization(
                new AutoMoqCustomization())))
        { }
    }
}
