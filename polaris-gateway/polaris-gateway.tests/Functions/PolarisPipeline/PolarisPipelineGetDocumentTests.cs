﻿using System.Security.AccessControl;
using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Common.Validators.Contracts;
using Common.ValueObjects;
using FluentAssertions;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Functions.PolarisPipeline.Document;
using Common.Telemetry.Wrappers.Contracts;
using Xunit;

namespace PolarisGateway.Tests.Functions.PolarisPipeline
{
    public class PolarisPipelineGetDocumentTests : SharedMethods.SharedMethods
    {
        private readonly string _blobName;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly PolarisDocumentId _polarisDocumentId;
        private readonly Stream _blobStream;

        private readonly Mock<IPipelineClient> _mockPipelineClient;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly Mock<ITelemetryAugmentationWrapper> _mockTelemetryAugmentationWrapper;

        private readonly PolarisPipelineGetDocument _polarisPipelineGetPdf;

        public PolarisPipelineGetDocumentTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            _polarisDocumentId = fixture.Create<PolarisDocumentId>();

            _blobName = fixture.Create<string>();
            _blobStream = new MemoryStream();

            var mockLogger = new Mock<ILogger<PolarisPipelineGetDocument>>();

            _mockTokenValidator = new Mock<IAuthorizationValidator>();
            _mockTokenValidator
                .Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValidateTokenResult { IsValid = true, UserName = "user-name" });

            _mockPipelineClient = new Mock<IPipelineClient>();
            _mockPipelineClient
                .Setup(client => client.GetDocumentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PolarisDocumentId>(), It.IsAny<Guid>()))
                .ReturnsAsync(_blobStream);

            _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.RegisterUserName(It.IsAny<string>()));
            _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.RegisterCorrelationId(It.IsAny<Guid>()));

            _polarisPipelineGetPdf = new PolarisPipelineGetDocument(_mockPipelineClient.Object, mockLogger.Object, _mockTokenValidator.Object, _mockTelemetryAugmentationWrapper.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
        {
            var response = await _polarisPipelineGetPdf.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
            var response = await _polarisPipelineGetPdf.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = false });
            var response = await _polarisPipelineGetPdf.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_ReturnsBadRequestWhenUrnIsInvalid(string caseUrn)
        {
            var response = await _polarisPipelineGetPdf.Run(CreateHttpRequest(), caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsNotFoundWhenPipelineClientReturnsNull()
        {
            _mockPipelineClient
                .Setup(client => client.GetDocumentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PolarisDocumentId>(), It.IsAny<Guid>()))
                .ReturnsAsync(default(Stream));

            var response = await _polarisPipelineGetPdf.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var response = await _polarisPipelineGetPdf.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBlobStream()
        {
            var response = await _polarisPipelineGetPdf.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value) as OkObjectResult;

            response?.Value.Should().Be(_blobStream);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockPipelineClient
                .Setup(client => client.GetDocumentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PolarisDocumentId>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var response = await _polarisPipelineGetPdf.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }
    }
}

