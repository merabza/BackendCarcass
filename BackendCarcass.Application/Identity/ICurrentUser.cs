using System.Collections.Generic;

namespace BackendCarcass.Application.Identity;

public interface ICurrentUser
{
    int Id { get; }
    int SerialNumber { get; }
    string Name { get; }
    List<string> Roles { get; }
}
