using System.Text.Json;

using GroceryStore.Features.Suppliers;

namespace GroceryStore.Tests.Features.Suppliers;

[Trait("Category", "Dto")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Suppliers")]
public class SupplierDtoTest
{
    [Fact]
    [Trait("Action", "Update")]
    public void SupplierUpdateDto_ShouldRejectUnknownJsonProperty()
    {
        const string json = "{\"street123\":\"Updated - Street\"}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SupplierUpdateDto>(json));
    }
}