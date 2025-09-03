using System.Net.Http.Json;
using PostgreIntegrationTestExample.Models;
using FluentAssertions;

public class UsersApiTest : IClassFixture<UsersApiTestFixture>
{
    private readonly UsersApiTestFixture _fixture;

    public UsersApiTest(UsersApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanCreateAndGetUser()
    {
        // Create a user
        var newUser = new User { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };
        var postResponse = await _fixture.Client!.PostAsJsonAsync("/api/users", newUser);
        postResponse.IsSuccessStatusCode.Should().BeTrue();
        var createdUser = await postResponse.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull();
        createdUser!.FirstName.Should().Be("Jane");

        // Get the user
        var getResponse = await _fixture.Client!.GetAsync($"/api/users/{createdUser.Id}");
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<User>();
        fetchedUser.Should().NotBeNull();
        fetchedUser!.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task CanUpdateUser()
    {
        // Create a new user
        var newUser = new User { FirstName = "John", LastName = "Smith", Email = "john@example.com" };
        var postResponse = await _fixture.Client!.PostAsJsonAsync("/api/users", newUser);
        postResponse.IsSuccessStatusCode.Should().BeTrue();
        var createdUser = await postResponse.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull();

        // Uodate a user
        createdUser!.LastName.Should().Be("Smith");
        createdUser.LastName = "Doe";
        var putResponse = await _fixture.Client!.PutAsJsonAsync($"/api/users/{createdUser.Id}", createdUser);
        putResponse.IsSuccessStatusCode.Should().BeTrue();

        // Read the yser
        var getResponse = await _fixture.Client!.GetAsync($"/api/users/{createdUser.Id}");
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        var updatedUser = await getResponse.Content.ReadFromJsonAsync<User>();
        updatedUser!.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task CanDeleteUser()
    {
        // Create a new user
        var newUser = new User { FirstName = "Alice", LastName = "Wonder", Email = "alice@example.com" };
        var postResponse = await _fixture.Client!.PostAsJsonAsync("/api/users", newUser);
        postResponse.IsSuccessStatusCode.Should().BeTrue();
        var createdUser = await postResponse.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull();

        // Delete the user
        var deleteResponse = await _fixture.Client!.DeleteAsync($"/api/users/{createdUser!.Id}");
        deleteResponse.IsSuccessStatusCode.Should().BeTrue();

        // Read the user and check is does not exist
        var getResponse = await _fixture.Client.GetAsync($"/api/users/{createdUser.Id}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
