using PostgreIntegrationTestExample.Models;

public static class TestUsers
{
    public static User Jane => new() { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };
    public static User John => new() { FirstName = "John", LastName = "Smith", Email = "john@example.com" };
    public static User Alice => new() { FirstName = "Alice", LastName = "Wonder", Email = "alice@example.com" };
}
