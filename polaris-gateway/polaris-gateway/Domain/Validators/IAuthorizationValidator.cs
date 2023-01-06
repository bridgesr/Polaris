﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace PolarisGateway.Domain.Validators
{
    public interface IAuthorizationValidator
    {
        Task<bool> ValidateTokenAsync(StringValues token, Guid correlationId, string requiredScopes = null, string requiredRoles = null);
    }
}