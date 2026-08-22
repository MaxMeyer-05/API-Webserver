using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ModuleCatalog.Controllers;
using ModuleCatalog.Contracts;
using ModuleCatalog.Services;
using ModuleCatalog.Tests.TestData;

namespace ModuleCatalog.Tests.Controllers;

[Trait("Category", "Controller")]
[Trait("Module", "ModuleCatalog")]
public class ModuleCatalogControllerTest
{
    #region GetRegisteredEndpoints Tests

    [Fact]
    [Trait("Action", "GetRegisteredEndpoints")]
    public void GetRegisteredEndpoints_ShouldReturnOkWithRegisteredEndpoints()
    {
        // Arrange
        var expectedEndpoints = ModuleCatalogTestData.RegisteredEndpointDtos;
        var serviceMock = new Mock<IModuleCatalogService>(MockBehavior.Strict);
        serviceMock
            .Setup(s => s.GetRegisteredEndpoints())
            .Returns(expectedEndpoints);

        var controller = new ModuleCatalogController(serviceMock.Object);

        // Act
        var result = controller.GetRegisteredEndpoints();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var returnedData = Assert.IsAssignableFrom<IReadOnlyList<EndpointDto>>(okResult.Value);
        Assert.Same(expectedEndpoints, returnedData);

        serviceMock.Verify(s => s.GetRegisteredEndpoints(), Times.Once);
        serviceMock.VerifyNoOtherCalls();
    }

    #endregion

    #region GetInstalledModules Tests

    [Fact]
    [Trait("Action", "GetInstalledModules")]
    public void GetInstalledModules_ShouldReturnOkWithInstalledModules()
    {
        // Arrange
        var expectedModules = ModuleCatalogTestData.InstalledModuleDtos;
        var serviceMock = new Mock<IModuleCatalogService>(MockBehavior.Strict);
        serviceMock
            .Setup(s => s.GetInstalledModules())
            .Returns(expectedModules);

        var controller = new ModuleCatalogController(serviceMock.Object);

        // Act
        var result = controller.GetInstalledModules();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var returnedData = Assert.IsAssignableFrom<IReadOnlyList<ModuleDto>>(okResult.Value);
        Assert.Same(expectedModules, returnedData);

        serviceMock.Verify(s => s.GetInstalledModules(), Times.Once);
        serviceMock.VerifyNoOtherCalls();
    }

    #endregion
}