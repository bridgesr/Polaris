﻿using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.Entity;
using Common.Domain.Extensions;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions.Document;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.DurableEntity.Entity.Contract;
using coordinator.Functions.Orchestration.Functions.Document;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestratorTests
    {
        private readonly CaseDocumentOrchestrationPayload _payload;
        private readonly DurableHttpRequest _evaluateDocumentDurableRequest;
        private readonly DurableHttpRequest _generatePdfDurableRequest;
        private readonly string _content;
        private readonly GeneratePdfResponse _pdfResponse;
        private readonly EvaluateDocumentResponse _evaluateDocumentResponse;

        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ICaseDurableEntity> _mockCaseEntity;
        private readonly Mock<ICaseRefreshLogsDurableEntity> _mockCaseRefreshLogsEntity;

        private readonly RefreshDocumentOrchestrator _caseDocumentOrchestrator;

        public CaseDocumentOrchestratorTests()
        {
            var fixture = new Fixture();
            var trackerCmsDocumentDto = fixture.Create<DocumentDto>();
            var trackerPcdRequestDto = fixture.Create<PcdRequestEntity>();
            var defendantsAndChargesListDto = fixture.Create<DefendantsAndChargesEntity>();
            _payload = new CaseDocumentOrchestrationPayload
                (
                    fixture.Create<string>(),
                    Guid.NewGuid(),
                    fixture.Create<string>(),
                    fixture.Create<long>(),
                    JsonSerializer.Serialize(trackerCmsDocumentDto),
                    JsonSerializer.Serialize(trackerPcdRequestDto),
                    JsonSerializer.Serialize(defendantsAndChargesListDto)
                ); ;
            _evaluateDocumentDurableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk/evaluateDocument"));
            _generatePdfDurableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk/generatePdf"));
            var updateSearchIndexDurableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk/updateSearchIndex"));
            var textExtractorDurableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk/textExtractor"));
            _content = fixture.Create<string>();
            var durableResponse = new DurableHttpResponse(HttpStatusCode.OK, content: _content);
            _pdfResponse = fixture.Create<GeneratePdfResponse>();
            
            var mockLogger = new Mock<ILogger<RefreshDocumentOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockCaseEntity = new Mock<ICaseDurableEntity>();
            _mockCaseEntity.Setup(entity => entity.GetVersion()).ReturnsAsync(1);
            _mockCaseRefreshLogsEntity = new Mock<ICaseRefreshLogsDurableEntity>();

            _evaluateDocumentResponse = fixture.Create<EvaluateDocumentResponse>();

            _mockDurableOrchestrationContext
                .Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>())
                .Returns(_payload);
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<DurableHttpRequest>(
                        nameof(CreateGeneratePdfHttpRequest),
                        It.Is<GeneratePdfHttpRequestActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.DocumentId == _payload.CmsDocumentTracker.CmsDocumentId && p.FileName == _payload.CmsDocumentTracker.CmsOriginalFileName)))
                .ReturnsAsync(_generatePdfDurableRequest);
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<DurableHttpRequest>(
                        nameof(CreateTextExtractorHttpRequest),
                        It.Is<TextExtractorHttpRequestActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.DocumentId == _payload.CmsDocumentTracker.CmsDocumentId && p.BlobName == _pdfResponse.BlobName)))
                .ReturnsAsync(textExtractorDurableRequest);
            _mockDurableOrchestrationContext
                .Setup(context => context.CallHttpAsync(_generatePdfDurableRequest))
                .ReturnsAsync(durableResponse);
            _mockDurableOrchestrationContext
                .Setup(context => context.CallHttpAsync(textExtractorDurableRequest))
                .ReturnsAsync(durableResponse);
            
            //set default activity responses
            _evaluateDocumentResponse.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
            _mockDurableOrchestrationContext
                .Setup(context => context.CallHttpAsync(_evaluateDocumentDurableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _evaluateDocumentResponse.ToJson()));
            _mockDurableOrchestrationContext
                .Setup(context => context.CallHttpAsync(updateSearchIndexDurableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _content));
            _mockDurableOrchestrationContext
                .Setup(context => context.CallHttpAsync(_generatePdfDurableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _pdfResponse.ToJson()));
            _mockDurableOrchestrationContext
                .Setup(context => context.CallHttpAsync(textExtractorDurableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _content));

            _mockDurableOrchestrationContext
                .Setup(context => context.CreateEntityProxy<ICaseDurableEntity>(It.Is<EntityId>(e => e.EntityName == nameof(CaseDurableEntity).ToLower() && e.EntityKey == $"[{_payload.CmsCaseId}]")))
                .Returns(_mockCaseEntity.Object);
            _mockDurableOrchestrationContext
                .Setup(context => context.CreateEntityProxy<ICaseRefreshLogsDurableEntity>(It.Is<EntityId>(e => e.EntityName == nameof(CaseRefreshLogsDurableEntity).ToLower() && e.EntityKey.Contains(_payload.CmsCaseId.ToString()))))
                .Returns(_mockCaseRefreshLogsEntity.Object);

            _caseDocumentOrchestrator = new RefreshDocumentOrchestrator(new JsonConvertWrapper(), mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<GeneratePdfResponse>(It.IsAny<string>(), null))
                .ReturnsAsync(default(GeneratePdfResponse));

            _mockDurableOrchestrationContext
                .Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>())
                .Returns((CaseDocumentOrchestrationPayload)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersPdfBlobName()
        {
            // Arrange
            _pdfResponse.AlreadyProcessed = false;
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<GeneratePdfResponse>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(_pdfResponse);

            // Act
            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            // Assert
            _mockCaseEntity.Verify
                (
                    tracker => 
                    tracker.SetDocumentStatus
                    (
                        It.Is<(string, DocumentStatus, string)>
                        (
                            a => 
                                a.Item1 == _payload.CmsDocumentTracker.PolarisDocumentId.ToString() && 
                                a.Item2 == DocumentStatus.PdfUploadedToBlob && 
                                a.Item3 == _pdfResponse.BlobName
                        )
                    )
                );
        }

        [Fact]
        public async Task Run_Tracker_RegistersIndexed_WhenNotAlreadyProcessed()
        {
            _pdfResponse.AlreadyProcessed = false;

            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<GeneratePdfResponse>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(_pdfResponse);

            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockCaseEntity.Verify
                (
                    tracker =>
                    tracker.SetDocumentStatus
                    (
                        It.Is<(string, DocumentStatus, string)>
                        (
                            a =>
                                a.Item1 == _payload.PolarisDocumentId.ToString() &&
                                a.Item2 == DocumentStatus.Indexed &&
                                a.Item3 == _pdfResponse.BlobName
                        )
                    )
                );
        }

        [Fact]
        public async Task Run_WhenDocumentEvaluation_EqualsAcquireDocument_AndSearchIndexUpdated_RegistersUnexpectedDocumentFailureWhenCallToGeneratePdfReturnsNonOkResponse()
        {
            // Arrange
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<GeneratePdfResponse>(It.IsAny<string>(), It.IsAny<CaseDocumentOrchestrationPayload>()))
                .ReturnsAsync((GeneratePdfResponse)null);

            try
            {
                // Act
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                // Assert
                _mockCaseEntity.Verify
                    (
                        tracker =>
                        tracker.SetDocumentStatus
                        (
                            It.Is<(string, DocumentStatus, string)>
                            (
                                a =>
                                    a.Item1 == _payload.PolarisDocumentId.ToString() &&
                                    a.Item2 == DocumentStatus.UnableToConvertToPdf
                            )
                        )
                    );
            }
        }

        [Fact]
        public async Task Run_RegistersAsIndexed_WhenDocumentEvaluation_EqualsDocumentUnchanged()
        {
            var pdfResponse = new GeneratePdfResponse(new Fixture().Create<string>());
            pdfResponse.AlreadyProcessed = true;

            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<GeneratePdfResponse>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(pdfResponse);

            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockCaseEntity.Verify
                    (
                        tracker =>
                        tracker.SetDocumentStatus
                        (
                            It.Is<(string, DocumentStatus, string)>
                            (
                                a =>
                                    a.Item1 == _payload.PolarisDocumentId.ToString() &&
                                    a.Item2 == DocumentStatus.DocumentAlreadyProcessed
                            )
                        )
                    );
            }
        }
    }
}
