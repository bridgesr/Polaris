import { CASE_ROUTE } from "../../../src/mock-api/routes";

describe("case details page", () => {
  describe("case page navigation", () => {
    it("can navigate back from case page, having not previously visited results page, and land on search page", () => {
      cy.visit("/case-details/12AB1111111/13401");

      cy.findAllByTestId("link-back-link").should("have.attr", "href", "/");

      cy.findAllByTestId("link-back-link").click();
      cy.location("pathname").should("eq", "/case-search");
    });

    it("can navigate back from case page, having previously visited results page, and land on results page", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.findByTestId("link-12AB1111111").click();

      cy.findAllByTestId("link-back-link").should(
        "have.attr",
        "href",
        "/case-search-results?urn=12AB1111111"
      );

      cy.findAllByTestId("link-back-link").click();

      cy.location("pathname").should("eq", "/case-search-results");
      cy.location("search").should("eq", "?urn=12AB1111111");
    });

    it("shows the unhandled error page if an unexpected error occurrs with the api", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.overrideRoute(CASE_ROUTE, {
        type: "break",
        httpStatusCode: 500,
      });

      cy.findByTestId("link-12AB1111111").click();

      // we are showing the error page
      cy.findByTestId("txt-error-page-heading");
    });
  });

  describe("case details", () => {
    it("Should show the feedback from link", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("feedback-banner")
        .should("exist")
        .contains(
          "Your feedback (opens in a new tab) will help us to improve this service."
        );
    });
    it("For Single defendant and single charge, should show defendant details, charge details and custody time limits and Youth Offender if applicable", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("defendant-details").then(($details) => {
        cy.wrap($details).contains("Walsh, Steve");
        cy.wrap($details).contains("DOB: 28 Nov 1977. Age: 45");
        cy.wrap($details).contains("Youth Offender");
      });

      cy.findByTestId("div-charges").then(($charges) => {
        cy.wrap($charges).findByTestId("charges-title").contains("Charges");
        cy.wrap($charges).contains("Custody time limit: 20 Days");
        cy.wrap($charges).contains("Custody end: 20 Nov 2022");
      });
    });

    it("For Single defendant and single charge,should read name from organisationName and shouldn't show date of birth in defendant details, if the defendant is an organisation ", () => {
      cy.visit("/case-search-results?urn=12AB1111122");
      cy.visit("/case-details/12AB1111122/13501");
      cy.findByTestId("txt-case-urn").contains("12AB1111122");
      cy.findByTestId("defendant-details").then(($details) => {
        cy.wrap($details)
          .findByTestId("txt-defendant-name")
          .should("have.text", "GUZZLERS BREWERY");
        cy.wrap($details).findByTestId("txt-defendant-DOB").should("not.exist");
      });
    });

    it("For multiple defendants, should show list of defendant names in the ascending order of listOrder and shouldn't show charge details", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13301");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("list-defendant-names")
        .get("li")
        .first()
        .should("have.text", "Walsh, Steve")
        .next()
        .should("have.text", "Taylor, Scott")
        .next()
        .should("have.text", "Victor, Peter");
      cy.findByTestId("link-defendant-details").contains(
        "View 3 defendants and charges"
      );
    });

    it("For multiple defendant, should read name from organisationName, if the defendant is an organisation", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13601");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("list-defendant-names")
        .get("li")
        .first()
        .should("have.text", "GUZZLERS BREWERY")
        .next()
        .should("have.text", "Victor, Peter");
      cy.findByTestId("link-defendant-details").contains(
        "View 2 defendants and charges"
      );
    });

    it("For multiple charges, should show list of defendant name and shouldn't show charge details", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13201");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("list-defendant-names")
        .get("li")
        .first()
        .should("have.text", "Walsh, Steve");
      cy.findByTestId("link-defendant-details").contains(
        "View 1 defendant and charges"
      );
    });

    it("For multiple charges / defendants, it can open the defendant and charges pdf and user should not be able to redact that document", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13201");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("list-defendant-names")
        .get("li")
        .first()
        .should("have.text", "Walsh, Steve");
      cy.findByTestId("link-defendant-details").contains(
        "View 1 defendant and charges"
      );
      cy.findByTestId("link-defendant-details").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("CASE OUTLINE");
      cy.findByTestId("tab-active").contains("Test DAC");
      cy.selectPDFTextElement("This is a DV case.");
      cy.findByTestId("btn-redact").should("have.length", 0);
      cy.findByTestId("redaction-warning").should("have.length", 1);
      cy.findByTestId("redaction-warning").contains(
        "This document can only be redacted in CMS."
      );
    });
  });

  describe("pdf viewing", () => {
    it("can open a pdf", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.findByTestId("div-pdfviewer-0").should("not.exist");

      cy.findByTestId("link-document-1").click();

      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    });

    it("can open a pdf in a new tab", () => {
      cy.visit("/case-details/12AB1111111/13401", {
        onBeforeLoad(window) {
          cy.stub(window, "open");
        },
      });

      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.findByTestId("link-document-1").click();

      cy.findByTestId("btn-open-pdf").click();

      cy.window()
        .its("open")
        .should(
          "be.calledWith",
          "https://mocked-out-api/api/some-complicated-sas-url/MCLOVEMG3",
          "_blank"
        );
    });
  });

  describe("Document navigation away alert modal", () => {
    it("Should show an alert modal when closing a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click({ force: true });
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have unsaved redactions");
      // click on return to case file btn
      cy.findByTestId("btn-nav-return").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have unsaved redactions");
      // click on ignore btn
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
    });

    it("Should not show an alert modal when closing a document when there are no active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
    });
  });

  describe("Navigating away from case file", () => {
    /* 
    We are mocking the onbeforeunload event callback function, to avoid triggering the alert in the cypress test,
    this will Prevent Test Runner hanging when the application uses confirmation dialog in window.onbeforeunload callback.
    Adding of stub callback to the beforeunload event is triggered by the logic in the application.
    This way we make sure teh call back is been fired.
    */
    beforeEach(() => {
      const beforeunloadCallbackStub = cy.stub().as("beforeunloadCallback");
      cy.on("window:before:load", (win) => {
        Object.defineProperty(win, "onbeforeunload", {
          set(cb) {
            if (cb !== null) {
              win.addEventListener("beforeunload", beforeunloadCallbackStub);
            }
          },
        });
      });
    });

    it("Should show browser confirm modal when navigating away by refreshing the page, from a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click();
      cy.reload();
      cy.get("@beforeunloadCallback").should("have.been.calledOnce");
    });

    it("Should not show browser confirm modal when navigating away by refreshing the page, from a document with no active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.reload();
      cy.get("@beforeunloadCallback").should("not.have.been.called");
    });

    it("Should show browser confirm modal when navigating away using Header Crown Prosecution Service link, from a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("link-homepage").click();
      cy.get("@beforeunloadCallback").should("have.been.calledOnce");
    });

    it("Should show custom alert modal when navigating away using 'find a case' link from a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("link-back-link").click();
      cy.get("@beforeunloadCallback").should("not.have.been.called");
      cy.findByTestId("div-modal").then(($modal) => {
        cy.wrap($modal).contains("You have 1 document with unsaved redactions");
        //checks for the pdf link in the modal and clicking on the link
        cy.wrap($modal)
          .findByTestId("link-document-1")
          .contains("MCLOVEMG3")
          .click();
      });
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("link-back-link").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have 1 document with unsaved redactions");
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.location("pathname").should("eq", "/case-search");
    });

    it("Should show custom alert modal when navigating away using browser back button from a document with active redactions", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.findByTestId("link-12AB1111111").click();
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click();
      cy.go(-1);
      //two times back button to reach the search page , first one was for the hash urls change with the pdf safeid
      cy.go(-1);
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have 1 document with unsaved redactions");
      cy.findByTestId("btn-nav-return").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.go(-1);
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have 1 document with unsaved redactions");
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.location("pathname").should("eq", "/case-search-results");
    });
  });

  describe("feature toggle", () => {
    it("Redaction shouldn't be allowed and User should show warning message when selecting a text,if presentation redact status is not 'Ok'", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("CASE OUTLINE");
      cy.selectPDFTextElement("This is a DV case.");
      cy.findByTestId("btn-redact").should("have.length", 0);
      cy.findByTestId("redaction-warning").should("have.length", 1);
      cy.findByTestId("redaction-warning").contains(
        "This document can only be redacted in CMS."
      );
    });

    it("User shouldn't be allowed to view document and there should be document view warnings, if presentation view status is not 'Ok'", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-3").should("have.length", 0);
      cy.findByTestId("view-warning-court-preparation").should(
        "have.length",
        1
      );
      cy.findByTestId("view-warning-court-preparation").contains(
        "Some documents for this case are only available in CMS"
      );
      cy.findByTestId("name-text-document-3").should("have.length", 1);
      cy.findByTestId("name-text-document-3").contains("MG05MCLOVE");
      cy.findByTestId("view-warning-document-3").should("have.length", 1);
      cy.findByTestId("view-warning-document-3").contains(
        "Document only available on CMS"
      );
    });
  });

  describe("Report an Issue", () => {
    it("Should show the `Report an issue` button with correct text and show the confirmation modal when user has reported an issue", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.findByTestId("div-pdfviewer-0").should("not.exist");

      cy.findByTestId("link-document-1").click();

      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("btn-report-issue").should("exist");
      cy.findByTestId("btn-report-issue").contains("Report an issue");
      cy.findByTestId("btn-report-issue").click();
      cy.findByTestId("btn-report-issue").contains("Issue reported");
      cy.findByTestId("btn-report-issue").should("be.disabled");
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("Thanks for reporting an issue with this document.");
      cy.findByTestId("btn-modal-close").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("btn-report-issue").contains("Issue reported");
      cy.findByTestId("btn-report-issue").should("be.disabled");
    });
  });
});

export {};
