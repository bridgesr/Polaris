import { PipelineResults } from "../../app/features/cases/domain/gateway/PipelineResults";
import { PipelinePdfResultsDataSource } from "./types/PipelinePdfResultsDataSource";
//the result count is set to 8 based on the maximum number of call tracker api call in a test suit, increase it when needed.
const dataSource: PipelinePdfResultsDataSource = () => getPipelinePdfResults(8);

export default dataSource;

const pipelinePdfResult: PipelineResults = {
  transactionId: "121",
  status: "Completed",
  processingCompleted: new Date().toISOString(),
  documentsRetrieved: new Date().toISOString(),
  documents: [
    {
      documentId: "1",
      cmsDocumentId: "1",
      pdfBlobName: "MCLOVEMG3",
      status: "Indexed",
      cmsOriginalFileName: "MCLOVEMG3",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-01",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 1,
        documentType: "MG11",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "2",
      cmsDocumentId: "2",
      pdfBlobName: "CM01",
      status: "Indexed",
      cmsOriginalFileName: "CM01",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 2,
        documentType: "MG12",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "DocTypeNotAllowed",
      },
    },
    {
      documentId: "3",
      cmsDocumentId: "3",
      pdfBlobName: "MG05MCLOVE",
      status: "Indexed",
      cmsOriginalFileName: "MG05MCLOVE",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG13",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "OnlyAvailableInCms",
        write: "Ok",
      },
    },
    {
      documentId: "4",
      cmsDocumentId: "4",
      pdfBlobName: "MG06_3June",
      status: "Indexed",
      cmsOriginalFileName: "MG06_3June",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-04",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 4,
        documentType: "MG14",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "5",
      cmsDocumentId: "5",
      pdfBlobName: "MG06_10june",
      status: "Indexed",
      cmsOriginalFileName: "MG06_10june",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 5,
        documentType: "MG15",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "6",
      cmsDocumentId: "null",
      pdfBlobName: "CM01",
      status: "UnexpectedFailure",
      cmsOriginalFileName: "Test DAC.pdf",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2023-05-11",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 1029,
        documentType: "DAC",
        documentCategory: "Review",
      },
      isPdfAvailable: false,
      presentationFlags: { read: "Ok", write: "DocTypeNotAllowed" },
    },
  ],
};
// this will return updated tracker data with updated polarisDocumentVersionId, processingCompleted and documentsRetrieved needed for te redaction refresh flow
const getPipelinePdfResults = (resultsCount: number) => {
  let resultsArray = Array(resultsCount)
    .fill({})
    .map((value, index) => ({
      ...pipelinePdfResult,

      processingCompleted: new Date(
        new Date().getTime() + index * 1000
      ).toISOString(),
      documentsRetrieved: new Date(
        new Date().getTime() + index * 1000
      ).toISOString(),
      documents: pipelinePdfResult.documents.map((document) => ({
        ...document,
        polarisDocumentVersionId: document.polarisDocumentVersionId + index,
      })),
    }));

  return resultsArray;
};
// This will create two results one with document id 2 and the second with document id 2 deleted.
const getRefreshDeletedDocuments = () => {
  const resultsArray = getPipelinePdfResults(2);

  return [
    resultsArray[0],
    {
      ...resultsArray[1],
      documents: resultsArray[1].documents.filter(
        ({ documentId }) => documentId !== "2"
      ),
    },
  ];
};

export const missingDocsPipelinePdfResults: PipelineResults = {
  transactionId: "121",
  status: "Completed",
  processingCompleted: new Date().toISOString(),
  documentsRetrieved: new Date().toISOString(),
  documents: [
    {
      documentId: "1",
      cmsDocumentId: "1",
      pdfBlobName: "MCLOVEMG3",
      status: "Indexed",
      cmsOriginalFileName: "MCLOVEMG3  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "2",
      cmsDocumentId: "2",
      pdfBlobName: "CM01",
      status: "Indexed",
      cmsOriginalFileName: "CM01  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 11,
        documentType: "MG11",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "3",
      cmsDocumentId: "3",
      pdfBlobName: "MG05MCLOVE",
      status: "Indexed",
      cmsOriginalFileName: "MG05MCLOVE very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 5,
        documentType: "MG5",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "4",
      cmsDocumentId: "4",
      pdfBlobName: "MG06_3June",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "MG06_3June  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 6,
        documentType: "MG6",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "5",
      cmsDocumentId: "5",
      pdfBlobName: "MG06_10june",
      status: "UnableToConvertToPdf",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
  ],
};

export const allMissingDocsPipelinePdfResults: PipelineResults = {
  transactionId: "121",
  status: "Completed",
  processingCompleted: new Date().toISOString(),
  documentsRetrieved: new Date().toISOString(),
  documents: [
    {
      documentId: "1",
      cmsDocumentId: "1",
      pdfBlobName: "MCLOVEMG3",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "MCLOVEMG3  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "2",
      cmsDocumentId: "2",
      pdfBlobName: "CM01",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "CM01  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 11,
        documentType: "MG11",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "3",
      cmsDocumentId: "3",
      pdfBlobName: "MG05MCLOVE",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "MG05MCLOVE very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 5,
        documentType: "MG5",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "4",
      cmsDocumentId: "4",
      pdfBlobName: "MG06_3June",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "MG06_3June  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 6,
        documentType: "MG6",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "5",
      cmsDocumentId: "5",
      pdfBlobName: "MG06_10june",
      status: "UnableToConvertToPdf",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
  ],
};

export const refreshPipelineDeletedDocuments: PipelinePdfResultsDataSource =
  () => getRefreshDeletedDocuments();
