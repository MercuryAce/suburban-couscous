using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using VisitManagement.Application.DTOs;
using VisitManagement.Domain.Visits;

namespace VisitManagement.Api.Tests;

public class VisitsApiTests : IClassFixture<VisitManagementWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient _client;

    public VisitsApiTests(VisitManagementWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateVisit_ValidBody_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/visits", ValidRequest(" ab12 xyz "), JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var visit = await response.Content.ReadFromJsonAsync<VisitResponse>(JsonOptions);
        Assert.NotNull(visit);
        Assert.Equal("AB12 XYZ", visit.VehicleLicenceNumber);
        Assert.Equal("Jane", visit.Visitor.FirstName);
        Assert.Contains(visit.Id.ToString(), response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetVisitById_ValidId_Returns200()
    {
        var created = await CreateVisitAsync();

        var response = await _client.GetAsync($"/api/v1/visits/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var visit = await response.Content.ReadFromJsonAsync<VisitResponse>(JsonOptions);
        Assert.Equal(created.Id, visit?.Id);
        Assert.Equal("AB12 XYZ", visit?.VehicleLicenceNumber);
    }

    [Fact]
    public async Task GetAllVisits_Returns200()
    {
        await CreateVisitAsync();

        var response = await _client.GetAsync("/api/v1/visits");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedVisitsResponse>(JsonOptions);
        Assert.NotNull(page);
        Assert.NotEmpty(page.Items);
    }

    [Fact]
    public async Task GetVisitById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/visits/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateVisit_EmptyActivities_Returns400()
    {
        var body = new CreateVisitRequest(
            VisitStatus.Active,
            "AB12 XYZ",
            new VisitorRequest("P123456", "Jane", "Doe"),
            []);

        var response = await _client.PostAsJsonAsync("/api/v1/visits", body, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateVisit_EndAtLessThanStartAt_Returns400()
    {
        var start = DateTimeOffset.Parse("2026-09-20T00:00:00Z");
        var end = DateTimeOffset.Parse("2026-09-10T00:00:00Z");
        var body = new CreateVisitRequest(
            VisitStatus.Active,
            "AB12 XYZ",
            new VisitorRequest("P123456", "Jane", "Doe"),
            [new ActivityRequest(ActivityType.Business, "TN-001", start, end)]);

        var response = await _client.PostAsJsonAsync("/api/v1/visits", body, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<VisitResponse> CreateVisitAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/visits", ValidRequest("AB12 XYZ"), JsonOptions);
        response.EnsureSuccessStatusCode();
        var visit = await response.Content.ReadFromJsonAsync<VisitResponse>(JsonOptions);
        Assert.NotNull(visit);
        return visit;
    }

    private static CreateVisitRequest ValidRequest(string licence) =>
        new(
            VisitStatus.Active,
            licence,
            new VisitorRequest("P123456", "Jane", "Doe"),
            [
                new ActivityRequest(
                    ActivityType.Business,
                    "TN-001",
                    DateTimeOffset.Parse("2026-09-10T00:00:00Z"),
                    DateTimeOffset.Parse("2026-09-20T00:00:00Z"))
            ]);

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
