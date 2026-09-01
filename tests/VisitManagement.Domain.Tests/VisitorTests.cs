using VisitManagement.Domain.Visits;
using Xunit;

public class VisitorTests
{
    [Fact]
    public void Create_trims_id_and_names()
    {
        var visitor = Visitor.Create("  P123456  ", " Jane ", " Doe ");

        Assert.Equal("P123456", visitor.Id);
        Assert.Equal("Jane", visitor.FirstName);
        Assert.Equal("Doe", visitor.LastName);
    }

    [Fact]
    public void Create_stores_id_and_names()
    {
        var visitor = Visitor.Create("P123456", "Jane", "Doe");

        Assert.Equal("P123456", visitor.Id);
        Assert.Equal("Jane", visitor.FirstName);
        Assert.Equal("Doe", visitor.LastName);
    }

    [Fact]
    public void Create_rejects_blank_id()
    {
        Assert.Throws<ArgumentException>(() => Visitor.Create("", "Jane", "Doe"));
    }

    [Fact]
    public void Create_rejects_blank_names()
    {
        Assert.Throws<ArgumentException>(() => Visitor.Create("P1", "", "Doe"));
        Assert.Throws<ArgumentException>(() => Visitor.Create("P1", "Jane", "  "));
    }
}
