import { useEffect, useCallback, useRef } from "react";
/**
 * This hook will take care of custom focus control on unsaved redaction buttons and remove redaction button in a document,
 * by ignoring all other tab-able elements within the document when user navigates through the unsaved redaction,
 * using keyboard Tab or (shift+tab) key.
 */
export const useControlledRedactionFocus = (
  tabId: string,
  activeTabId: string | undefined,
  tabIndex: number
) => {
  const getTabbableElements = useCallback(() => {
    const pdfHighlighters = document.querySelectorAll(".PdfHighlighter");
    const pageHighlightElements = pdfHighlighters[tabIndex].querySelectorAll(
      ".highlight-layer-wrapper"
    );
    const tabbableElements = Array.from(pageHighlightElements).reduce(
      (acc, current) => {
        const elements = current.querySelectorAll("button:not([disabled])");
        acc = [...acc, elements[0]];
        return acc;
      },
      [] as Element[]
    );
    return tabbableElements;
  }, [tabIndex]);

  const getRemoveRedactionBtn = useCallback(() => {
    const pdfHighlighters = document.querySelectorAll(".PdfHighlighter");
    const removeBtn = pdfHighlighters[tabIndex].querySelector("#remove-btn");
    return removeBtn;
  }, [tabIndex]);

  const getRemoveAllRedactionBtn = useCallback(() => {
    const pdfHighlighters = document.querySelectorAll(".govuk-tabs__panel");
    const removeAllRedactionBtn = pdfHighlighters[tabIndex].querySelector(
      `#btn-link-removeAll-${tabIndex}`
    );
    return removeAllRedactionBtn;
  }, [tabIndex]);

  const getEntryElement = useCallback(() => {
    const pdfHighlighters = document.querySelectorAll(".govuk-tabs__panel");
    const reportIssueBtn = pdfHighlighters[tabIndex].querySelector(
      `#btn-report-issue-${tabIndex}`
    );
    const openPdfBtn = pdfHighlighters[tabIndex].querySelector("#btn-open-pdf");
    const tabPanel = pdfHighlighters[tabIndex];
    const entryElement = (reportIssueBtn as HTMLButtonElement).disabled
      ? openPdfBtn ?? tabPanel
      : reportIssueBtn;
    return entryElement;
  }, [tabIndex]);

  const activeButtonIndex = useRef(0);

  const handleTabKeyPress = (
    tabbableElements: Element[],
    removeBtn: Element | null,
    removeAllRedactionBtn: Element | null,
    entryElement: Element | null,
    e: KeyboardEvent
  ) => {
    if (document.activeElement === entryElement) {
      (tabbableElements[0] as HTMLElement).focus();
      (tabbableElements[0] as HTMLElement).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      activeButtonIndex.current = 0;
      if (removeBtn) {
        (removeBtn as HTMLElement).style.visibility = "visible";
      }
      e.preventDefault();
      return;
    }
    if (
      document.activeElement === removeBtn &&
      activeButtonIndex.current < tabbableElements.length - 1
    ) {
      activeButtonIndex.current = activeButtonIndex.current + 1;
      (tabbableElements[activeButtonIndex.current] as HTMLElement).focus();
      (
        tabbableElements[activeButtonIndex.current] as HTMLElement
      ).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      e.preventDefault();
      return;
    }
    if (
      Array.from(tabbableElements).includes(document.activeElement!) &&
      removeBtn
    ) {
      activeButtonIndex.current = Array.from(tabbableElements).indexOf(
        document.activeElement!
      );
      (removeBtn as HTMLElement).focus();
      (removeBtn as HTMLElement).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      e.preventDefault();
      return;
    }
    if (
      document.activeElement === removeBtn &&
      activeButtonIndex.current === tabbableElements.length - 1
    ) {
      (removeAllRedactionBtn as HTMLElement).focus();
      (removeAllRedactionBtn as HTMLElement).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      (removeBtn as HTMLElement).style.visibility = "hidden";
      e.preventDefault();
    }
  };

  const handleShiftTabKeyPress = (
    tabbableElements: Element[],
    removeBtn: Element | null,
    removeAllRedactionBtn: Element | null,
    entryElement: Element | null,
    e: KeyboardEvent
  ) => {
    if (document.activeElement === removeAllRedactionBtn) {
      (tabbableElements[tabbableElements.length - 1] as HTMLElement).focus();
      (
        tabbableElements[tabbableElements.length - 1] as HTMLElement
      ).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      activeButtonIndex.current = tabbableElements.length - 1;
      if (removeBtn) {
        (removeBtn as HTMLElement).style.visibility = "visible";
      }
      e.preventDefault();
      return;
    }
    if (document.activeElement === removeBtn) {
      (tabbableElements[activeButtonIndex.current] as HTMLElement).focus();
      (
        tabbableElements[activeButtonIndex.current] as HTMLElement
      ).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      e.preventDefault();
      return;
    }
    if (
      Array.from(tabbableElements).includes(document.activeElement!) &&
      activeButtonIndex.current > 0
    ) {
      activeButtonIndex.current = activeButtonIndex.current - 1;
      (tabbableElements[activeButtonIndex.current] as HTMLElement).focus();
      (
        tabbableElements[activeButtonIndex.current] as HTMLElement
      ).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      e.preventDefault();
      return;
    }
    if (document.activeElement === tabbableElements[0]) {
      (entryElement as HTMLElement).focus();
      (entryElement as HTMLElement).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      if (removeBtn) {
        (removeBtn as HTMLElement).style.visibility = "hidden";
      }
      e.preventDefault();
    }
  };

  const keyDownHandler = useCallback(
    (e: KeyboardEvent) => {
      if (
        tabId === activeTabId &&
        document.activeElement &&
        (e.code === "Tab" || e.key === "Tab")
      ) {
        const tabbableElements = getTabbableElements();
        if (!tabbableElements.length) {
          return;
        }
        const removeBtn = getRemoveRedactionBtn();
        const removeAllRedactionBtn = getRemoveAllRedactionBtn();
        const entryElement = getEntryElement();
        if (!e.shiftKey) {
          handleTabKeyPress(
            tabbableElements,
            removeBtn,
            removeAllRedactionBtn,
            entryElement,
            e
          );
        }
        if (e.shiftKey) {
          handleShiftTabKeyPress(
            tabbableElements,
            removeBtn,
            removeAllRedactionBtn,
            entryElement,
            e
          );
        }
      }
    },
    [
      activeTabId,
      getRemoveRedactionBtn,
      getEntryElement,
      getRemoveAllRedactionBtn,
      getTabbableElements,
      tabId,
    ]
  );

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
