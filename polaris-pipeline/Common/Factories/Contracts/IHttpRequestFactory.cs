﻿using System;
using System.Net.Http;

namespace Common.Factories.Contracts
{
    public interface IHttpRequestFactory
    {
        HttpRequestMessage CreateGet(string requestUri, string accessToken, string cmsAuthValues, Guid correlationId);
    }
}
