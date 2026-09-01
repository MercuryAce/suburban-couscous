using VisitManagement.Domain.Visits;
using Xunit;

public class LicenceNumberTests
{
    [Theory]
    [InlineData("AB12 DTF")]
    [InlineData("Ab12 dtf")]
    [InlineData("  ab12 dtf  ")]
    public void Parse_TrimsAndUppercases_WhenValid(string raw)
    {
        var licenceNumber = LicenceNumber.Parse(raw);

        Assert.Equal(raw.Trim().ToUpperInvariant(), licenceNumber.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Parse_NullOrWhitespace_ThrowsArgumentException(string? raw)
    {
        var action = () => LicenceNumber.Parse(raw);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Equality_By_NormalisedValue_WhenValid()
    {
        var left = LicenceNumber.Parse("ab12");
        var right = LicenceNumber.Parse(" AB12 ");

        Assert.Equal(left, right);
    }

    [Fact]
    public void ToString_ReturnsNormalisedValue()
    {
        var licenceNumber = LicenceNumber.Parse("  ab12 dtf ");

        Assert.Equal("AB12 DTF", licenceNumber.ToString());
    }
}
