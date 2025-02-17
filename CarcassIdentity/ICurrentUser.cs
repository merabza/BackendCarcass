﻿using System.Collections.Generic;

namespace CarcassIdentity;

public interface ICurrentUser
{
    int Id { get; }
    int SerialNumber { get; }
    string Name { get; }
    List<string> Roles { get; }
}