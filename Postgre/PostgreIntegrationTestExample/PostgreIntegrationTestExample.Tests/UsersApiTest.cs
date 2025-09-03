using System.Net.Http.Json;
using PostgreIntegrationTestExample.Models;
using FluentAssertions;

public class UsersApiTest(UsersApiTestFixture fixture) : IClassFixture<UsersApiTestFixture>
{
    private readonly UsersApiTestFixture _fixture = fixture;

    [Fact]
    public async Task CanCreateAndGetUser()
    {
        await _fixture.SeedUsersAsync(); // Ensure clean DB
        var postResponse = await _fixture.Client!.PostAsJsonAsync("/api/users", TestUsers.Jane);
        postResponse.IsSuccessStatusCode.Should().BeTrue();
        var createdUser = await postResponse.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull();
        createdUser!.FirstName.Should().Be(TestUsers.Jane.FirstName);

        var getResponse = await _fixture.Client!.GetAsync($"/api/users/{createdUser.Id}");
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<User>();
        fetchedUser.Should().NotBeNull();
        fetchedUser!.FirstName.Should().Be(TestUsers.Jane.FirstName);
    }

    [Fact]
    public async Task CanUpdateUser()
    {
        var seededUser = TestUsers.John;
        await _fixture.SeedUsersAsync(seededUser);

        // Update the user via API
        seededUser.LastName = "Doe";
        var putResponse = await _fixture.Client!.PutAsJsonAsync($"/api/users/{seededUser.Id}", seededUser);
        putResponse.IsSuccessStatusCode.Should().BeTrue();

        var getResponse = await _fixture.Client!.GetAsync($"/api/users/{seededUser.Id}");
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        var updatedUser = await getResponse.Content.ReadFromJsonAsync<User>();
        updatedUser!.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task CanDeleteUser()
    {
        var seededUser = TestUsers.Alice;
        await _fixture.SeedUsersAsync(seededUser);

        // Delete the user via API
        var deleteResponse = await _fixture.Client!.DeleteAsync($"/api/users/{seededUser.Id}");
        deleteResponse.IsSuccessStatusCode.Should().BeTrue();

        var getResponse = await _fixture.Client.GetAsync($"/api/users/{seededUser.Id}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
