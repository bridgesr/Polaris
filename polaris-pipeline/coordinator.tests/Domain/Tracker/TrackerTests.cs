﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain.Tracker;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Common.Wrappers.Contracts;
using Common.Wrappers;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.DurableEntity.Client.Tracker;
using Common.Dto.Tracker;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;

namespace coordinator.tests.Domain.Tracker
{
    public class TrackerTests
    {
        private readonly Fixture _fixture;
        private readonly string _transactionId;
        private readonly List<DocumentDto> _documents;
        private readonly RegisterPdfBlobNameArg _pdfBlobNameArg;
        private readonly SynchroniseDocumentsArg _synchroniseDocumentsArg;
        private readonly List<TrackerDocumentDto> _trackerDocuments;
        private readonly string _caseUrn;
        private readonly long _caseId;
        private readonly Guid _correlationId;
        private readonly EntityStateResponse<TrackerEntity> _entityStateResponse;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        private readonly Mock<IDurableEntityContext> _mockDurableEntityContext;
        private readonly Mock<IDurableEntityClient> _mockDurableEntityClient;
        private readonly Mock<ILogger> _mockLogger;

        private readonly TrackerEntity _tracker;
        private readonly TrackerClient _trackerStatus;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _transactionId = _fixture.Create<string>();
            _documents = _fixture.CreateMany<DocumentDto>(3).ToList();
            _correlationId = _fixture.Create<Guid>();
            _pdfBlobNameArg = _fixture.Build<RegisterPdfBlobNameArg>()
                                .With(a => a.DocumentId, _documents.First().DocumentId)
                                .With(a => a.VersionId, _documents.First().VersionId)
                                .Create();
            _trackerDocuments = _fixture.Create<List<TrackerDocumentDto>>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<long>();
            _synchroniseDocumentsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, _documents)
                .With(a => a.CorrelationId, _correlationId)
                .Create();
            _entityStateResponse = new EntityStateResponse<TrackerEntity>() { EntityExists = true };
            _jsonConvertWrapper = _fixture.Create<JsonConvertWrapper>();

            _mockDurableEntityContext = new Mock<IDurableEntityContext>();
            _mockDurableEntityClient = new Mock<IDurableEntityClient>();
            _mockLogger = new Mock<ILogger>();

            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<TrackerEntity>(
                    It.Is<EntityId>(e => e.EntityName == nameof(TrackerEntity).ToLower() && e.EntityKey == _caseId.ToString()),
                    null, null))
                .ReturnsAsync(_entityStateResponse);

            _tracker = new TrackerEntity();
            _trackerStatus = new TrackerClient(_jsonConvertWrapper);
        }

        [Fact]
        public async Task Initialise_Initialises()
        {
            await _tracker.Reset(_transactionId);

            _tracker.TransactionId.Should().Be(_transactionId);
            _tracker.Documents.Should().NotBeNull();
            _tracker.Logs.Should().NotBeNull();
            _tracker.Status.Should().Be(TrackerStatus.Running);

            _tracker.Logs.Count.Should().Be(1);
        }

        [Fact]
        public async Task RegisterPdfBlobName_RegistersPdfBlobName()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterPdfBlobName(_pdfBlobNameArg);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.PdfBlobName.Should().Be(_pdfBlobNameArg.BlobName);
            document?.Status.Should().Be(TrackerDocumentStatus.PdfUploadedToBlob);

            _tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task RegisterDocumentAsAlreadyProcessed_Registers()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterBlobAlreadyProcessed(new RegisterPdfBlobNameArg(_pdfBlobNameArg.DocumentId, _pdfBlobNameArg.VersionId, _pdfBlobNameArg.BlobName));

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _pdfBlobNameArg.DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.DocumentAlreadyProcessed);

            _tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task RegisterDocumentAsFailedPDFConversion_Registers()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterUnableToConvertDocumentToPdf(_pdfBlobNameArg.DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _pdfBlobNameArg.DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.UnableToConvertToPdf);

            _tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task Initialisation_SetsDocumentStatusToNone()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.New);

            _tracker.Logs.Count.Should().Be(8);
        }

        [Fact]
        public async Task RegisterUnexpectedDocumentFailure_Registers()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterUnexpectedPdfDocumentFailure(_pdfBlobNameArg.DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.UnexpectedFailure);

            _tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersIndexed()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterIndexed(_documents.First().DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.Indexed);

            _tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersOcrAndIndexFailure()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterOcrAndIndexFailure(_documents.First().DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.OcrAndIndexFailure);

            _tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task RegisterCompleted_RegistersCompleted()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.RegisterCompleted();

            _tracker.Status.Should().Be(TrackerStatus.Completed);
            _tracker.ProcessingCompleted.Should().NotBeNull();

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task RegisterFailed_RegistersFailed()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.RegisterFailed();

            _tracker.Status.Should().Be(TrackerStatus.Failed);

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetDocuments_ReturnsDocuments()
        {
            _tracker.Documents = _trackerDocuments;
            var documents = await _tracker.GetDocuments();

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsTrueIfAllDocumentsFailed()
        {
            _tracker.Documents = new List<TrackerDocumentDto> {
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(),  _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnexpectedFailure}
            };

            var output = await _tracker.AllDocumentsFailed();

            output.Should().BeTrue();
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsFalseIfAllDocumentsHaveNotFailed()
        {
            _tracker.Documents = new List<TrackerDocumentDto> {
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnexpectedFailure},
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.PdfUploadedToBlob},
            };

            var output = await _tracker.AllDocumentsFailed();

            output.Should().BeFalse();
        }

        [Fact]
        public async Task Run_Tracker_Dispatches()
        {
            await TrackerEntity.Run(_mockDurableEntityContext.Object);

            _mockDurableEntityContext.Verify(context => context.DispatchAsync<TrackerEntity>());
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsOK()
        {
            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsEntityState()
        {
            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            var okObjectResult = response as OkObjectResult;

            okObjectResult?.Value.Should().Be(_entityStateResponse.EntityState);
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsNotFoundIfEntityNotFound()
        {
            var entityStateResponse = new EntityStateResponse<TrackerEntity>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<TrackerEntity>(
                    It.Is<EntityId>(e => e.EntityName == nameof(TrackerEntity).ToLower() && e.EntityKey == _caseId.ToString()),
                    null, null))
                .ReturnsAsync(entityStateResponse);

            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsBadRequestIfCorrelationIdNotFound()
        {
            var entityStateResponse = new EntityStateResponse<TrackerEntity>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                    client => client.ReadEntityStateAsync<TrackerEntity>(
                        It.Is<EntityId>(e => e.EntityName == nameof(TrackerEntity).ToLower() && e.EntityKey == _caseId.ToString()),
                        null, null))
                .ReturnsAsync(entityStateResponse);

            var message = new HttpRequestMessage();
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        #region SynchroniseDocument

        [Fact]
        public async Task SynchroniseDocument_CreatesNewDocuments()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset(_transactionId);

            // Act
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(1);
            tracker.Documents.Count.Should().Be(_documents.Count);
            deltas.CreatedDocuments.Count.Should().Be(_documents.Count);
            deltas.UpdatedDocuments.Count.Should().Be(0);
            deltas.DeletedDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.Documents[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.Documents[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.Documents[2].PolarisDocumentVersionId.Should().Be(1);
            tracker.Logs.Count.Should().Be(8);
        }

        [Fact]
        public async Task SynchroniseDocument_NoChangesWithExistingDocumentAndVersionIds()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset(_transactionId);
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Act 
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(1);
            tracker.Documents.Count.Should().Be(_documents.Count);
            deltas.CreatedDocuments.Count.Should().Be(0);
            deltas.UpdatedDocuments.Count.Should().Be(0);
            deltas.DeletedDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeFalse();
            tracker.Documents[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.Documents[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.Documents[2].PolarisDocumentVersionId.Should().Be(1);
            tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task SynchroniseDocument_ChangesWithUpdatedDocumentAndVersionIds()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset(_transactionId);
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Act 
            _synchroniseDocumentsArg.Documents[1].VersionId = 111111111;
            _synchroniseDocumentsArg.Documents[2].VersionId = 222222222;
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(2);
            tracker.Documents.Count.Should().Be(_documents.Count);
            deltas.CreatedDocuments.Count.Should().Be(0);
            deltas.UpdatedDocuments.Count.Should().Be(2);
            deltas.DeletedDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.Documents[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.Documents[1].PolarisDocumentVersionId.Should().Be(2);
            tracker.Documents[2].PolarisDocumentVersionId.Should().Be(2);

            tracker.Logs.Count.Should().Be(11);
        }

        [Fact]
        public async Task SynchroniseDocument_ChangesWithDeletedDocuments()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset(_transactionId);
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Act 
            _synchroniseDocumentsArg.Documents.RemoveAt(2);
            _synchroniseDocumentsArg.Documents.RemoveAt(1);
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(2);
            tracker.Documents.Count.Should().Be(1);
            deltas.CreatedDocuments.Count.Should().Be(0);
            deltas.UpdatedDocuments.Count.Should().Be(0);
            deltas.DeletedDocuments.Count.Should().Be(2);
            deltas.Any().Should().BeTrue();
            tracker.Documents[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.Logs.Count.Should().Be(11);
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSame_ReturnsNothingToEvaluate()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            _tracker.Documents.Count.Should().Be(_documents.Count);

            var newDaysDocuments = new DocumentDto[3];
            _documents.CopyTo(newDaysDocuments);
            var newDaysDocumentIdsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, newDaysDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.Documents.Count.Should().Be(_documents.Count);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsNotTheSame_ReturnsRecordsToEvaluate()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            _tracker.Documents.Count.Should().Be(_documents.Count);

            var newDaysDocuments = new List<DocumentDto> { _documents.First() };
            ////only one document in today's run, the next two should be removed from the tracker and in the evaluation results

            var newDaysDocumentIdsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, newDaysDocuments)
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.Documents.Count.Should().Be(1);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSameExceptForANewVersionOfOneDoc_ReturnsOneRecordToEvaluate()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            _tracker.Documents.Count.Should().Be(_documents.Count);

            var newDaysDocuments = new DocumentDto[3];
            _documents.CopyTo(newDaysDocuments);
            var originalVersionId = newDaysDocuments[1].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysDocuments[1].VersionId = newVersionId;
            var modifiedDocumentId = newDaysDocuments[1].DocumentId;
            var newDaysDocumentIdsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, newDaysDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.Documents.Count.Should().Be(_documents.Count);
                var newVersion = _tracker.Documents.Find(x => x.CmsDocumentId == modifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.CmsVersionId.Should().Be(newVersionId);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_OneDocumentRemovedAndOneANewVersion_ReturnsTwoRecordToEvaluate()
        {
            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            _tracker.Documents.Count.Should().Be(_documents.Count);

            var newDaysDocuments = new List<DocumentDto>
            {
                _documents[1],
                _documents[2]
            };

            var documentRemovedFromCmsId = _documents[0].DocumentId;
            var originalVersionId = newDaysDocuments[0].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysDocuments[0].VersionId = newVersionId;
            var modifiedDocumentId = newDaysDocuments[0].DocumentId;

            var unmodifiedDocumentId = newDaysDocuments[1].DocumentId;
            var unmodifiedDocumentVersionId = newDaysDocuments[1].VersionId;

            var newDaysDocumentIdsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, newDaysDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Reset(_transactionId);
            await _tracker.SynchroniseDocuments(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.Documents.Count.Should().Be(2);
                var newVersion = _tracker.Documents.Find(x => x.CmsDocumentId == modifiedDocumentId);
                var unmodifiedDocument = _tracker.Documents.Find(x => x.CmsDocumentId == unmodifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.CmsVersionId.Should().Be(newVersionId);

                unmodifiedDocument.Should().NotBeNull();
                unmodifiedDocument?.CmsVersionId.Should().Be(unmodifiedDocumentVersionId);

                var searchResultForDocumentRemovedFromCms = _tracker.Documents.Find(x => x.CmsDocumentId == documentRemovedFromCmsId);
                searchResultForDocumentRemovedFromCms.Should().BeNull();
            }
        }

        #endregion
    }
}
