using Lisa.Domain.Entities;
using Lisa.Infrastructure.Identity;

namespace Lisa.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<User>();
    }
}
