﻿using System;

namespace CarcassShared;

public sealed class ProjectException : Exception
{
    public ProjectException()
    {
    }

    public ProjectException(string message) : base(message)
    {
    }

    public ProjectException(string message, Exception inner) : base(message, inner)
    {
    }
}