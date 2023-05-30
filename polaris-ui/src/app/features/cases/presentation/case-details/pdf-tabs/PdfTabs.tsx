import { Tabs } from "../../../../../common/presentation/components/tabs";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { PdfTab } from "./PdfTab";

type PdfTabsProps = {
  tabsState: {
    items: CaseDocumentViewModel[];
    headers: HeadersInit;
    activeTabId: string | undefined;
  };
  pipelineState: CaseDetailsState["pipelineState"];
  savedDocumentDetails: {
    documentId: string;
    polarisDocumentVersionId: number;
  }[];
  contextData: {
    correlationId: string;
    urn: string;
    caseId: string;
  };
  handleTabSelection: (documentId: string) => void;
  handleClosePdf: (caseDocument: { documentId: string }) => void;
  handleLaunchSearchResults: () => void;
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: CaseDetailsState["handleRemoveRedaction"];
  handleRemoveAllRedactions: CaseDetailsState["handleRemoveAllRedactions"];
  handleSavedRedactions: CaseDetailsState["handleSavedRedactions"];
  handleOpenPdfInNewTab: CaseDetailsState["handleOpenPdfInNewTab"];
  handleUnLockDocuments: CaseDetailsState["handleUnLockDocuments"];
};

export const PdfTabs: React.FC<PdfTabsProps> = ({
  tabsState: { items, headers, activeTabId },
  contextData,
  savedDocumentDetails,
  handleTabSelection,
  pipelineState,
  handleClosePdf,
  handleLaunchSearchResults,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
  handleOpenPdfInNewTab,
  handleUnLockDocuments,
}) => {
  return (
    <Tabs
      idPrefix="pdf"
      items={items.map((item, index) => ({
        isDirty: item.redactionHighlights.length > 0,
        id: item.documentId,
        label: item.presentationFileName,
        panel: {
          children: (
            <PdfTab
              tabIndex={index}
              caseDocumentViewModel={item}
              savedDocumentDetails={savedDocumentDetails}
              redactStatus={item.presentationFlags.write}
              headers={headers}
              handleLaunchSearchResults={handleLaunchSearchResults}
              handleAddRedaction={handleAddRedaction}
              handleRemoveRedaction={handleRemoveRedaction}
              handleRemoveAllRedactions={handleRemoveAllRedactions}
              handleSavedRedactions={handleSavedRedactions}
              handleOpenPdfInNewTab={handleOpenPdfInNewTab}
              contextData={contextData}
            />
          ),
        },
      }))}
      title="Contents"
      activeTabId={activeTabId}
      handleClosePdf={handleClosePdf}
      handleTabSelection={handleTabSelection}
      handleUnLockDocuments={handleUnLockDocuments}
    />
  );
};
