using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BackendCarcassApi.Test.Endpoints.V1;

public class MasterDataEndpointsFixture
{
    public Mock<IMediator> MediatorMock { get; }
    public Mock<HttpRequest> RequestMock { get; }

    public MasterDataEndpointsFixture()
    {
        MediatorMock = new Mock<IMediator>();
        RequestMock = new Mock<HttpRequest>();
    }
}