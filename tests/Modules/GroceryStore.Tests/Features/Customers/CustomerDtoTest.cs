using System.Text.Json;

using GroceryStore.Features.Customers;

namespace GroceryStore.Tests.Features.Customers;

[Trait("Category", "Dto")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Customers")]
public class CustomerDtoTest
{
    [Fact]
    [Trait("Action", "Update")]
    public void CustomerUpdateDto_ShouldRejectUnknownJsonProperty()
    {
        const string json = "{\"street123\":\"Updated - Street\"}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CustomerUpdateDto>(json));
    }
}