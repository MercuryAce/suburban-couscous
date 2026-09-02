using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using VisitManagement.Api.Auth;
using VisitManagement.Application.DTOs;
using VisitManagement.Domain.Visits;

namespace VisitManagement.Api.Tests;

public class VisitsApiTests : IClassFixture<VisitManagementWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public VisitsApiTests(VisitManagementWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken(ScopeClaims.Read, ScopeClaims.Write));

        _anonymous = factory.CreateClient();
    }

    [Fact]
    public async Task GetVisit_WithoutToken_Returns401()
    {
        var response = await _anonymous.GetAsync($"/api/v1/visits/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateVisit_ReadOnlyToken_Returns403()
    {
        _anonymous.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken(ScopeClaims.Read));

        var response = await _anonymous.PostAsJsonAsync("/api/v1/visits", ValidRequest("AB12 XYZ"), JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetVisit_WriteOnlyToken_Returns403()
    {
        _anonymous.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken(ScopeClaims.Write));

        var response = await _anonymous.GetAsync($"/api/v1/visits/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Token_ValidClient_ReturnsBearerThatCanCreate()
    {
        var tokenResponse = await _anonymous.PostAsJsonAsync(
            "/api/v1/auth/token",
            new TokenRequest(TestJwt.ClientId, TestJwt.ClientSecret));

        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);
        Assert.False(string.IsNullOrWhiteSpace(token?.AccessToken));

        _anonymous.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        var create = await _anonymous.PostAsJsonAsync("/api/v1/visits", ValidRequest("AB12 XYZ"), JsonOptions);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
    }

    [Fact]
    public async Task Token_InvalidSecret_Returns401()
    {
        var response = await _anonymous.PostAsJsonAsync(
            "/api/v1/auth/token",
            new TokenRequest(TestJwt.ClientId, "wrong-secret"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        Assert.Equal(TestJwt.ClientId, visit.CreatedBy);
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
    public async Task GetAllVisits_PageAndPageSize_AreEchoed()
    {
        await CreateVisitAsync();
        await CreateVisitAsync();

        var response = await _client.GetAsync("/api/v1/visits?page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedVisitsResponse>(JsonOptions);
        Assert.NotNull(page);
        Assert.Equal(1, page.Page);
        Assert.Equal(1, page.PageSize);
        Assert.Single(page.Items);
        Assert.True(page.TotalCount >= 2);
    }

    [Fact]
    public async Task GetAllVisits_PageSizeOverMax_Returns400ProblemDetails()
    {
        var response = await _client.GetAsync("/api/v1/visits?page=1&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.Equal(400, problem?.Status);
        Assert.Equal("Bad Request", problem?.Title);
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
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.Equal(400, problem?.Status);
        Assert.Equal("Bad Request", problem?.Title);
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

    [Fact]
    public async Task UpdateVisit_ValidBody_Returns200()
    {
        var created = await CreateVisitAsync();
        var body = ValidRequest(" xy99 zzz ") with { Status = VisitStatus.Completed };

        var response = await _client.PutAsJsonAsync($"/api/v1/visits/{created.Id}", body, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var visit = await response.Content.ReadFromJsonAsync<VisitResponse>(JsonOptions);
        Assert.Equal(created.Id, visit?.Id);
        Assert.Equal("XY99 ZZZ", visit?.VehicleLicenceNumber);
        Assert.Equal(VisitStatus.Completed, visit?.Status);
        Assert.Equal(TestJwt.ClientId, visit?.UpdatedBy);
        Assert.Equal(created.CreatedBy, visit?.CreatedBy);
    }

    [Fact]
    public async Task UpdateVisit_UnknownId_Returns404()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/visits/{Guid.NewGuid()}",
            ValidRequest("AB12 XYZ"),
            JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateVisit_EmptyActivities_Returns400()
    {
        var created = await CreateVisitAsync();
        var body = new CreateVisitRequest(
            VisitStatus.Active,
            "AB12 XYZ",
            new VisitorRequest("P123456", "Jane", "Doe"),
            []);

        var response = await _client.PutAsJsonAsync($"/api/v1/visits/{created.Id}", body, JsonOptions);

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
