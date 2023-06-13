﻿using CarcassContracts.V1.Responses;
using MessagingAbstractions;

namespace ServerCarcassMini.CommandRequests.Authentication;

public sealed class RegistrationCommandRequest : ICommand<LoginResponse>
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}