import { correlationIds } from "../../support/correlation-ids"

const { REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID } = Cypress.env()

describe("Refresh via guid-controlled ", () => {
  it("can update a document", () => {
    cy.fullLogin()

    cy.clearCaseTracker(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID)

    cy.visit("/")
    cy.setPolarisInstrumentationGuid("PHASE_1")
    cy.findByTestId("input-search-urn").type(`${REFRESH_TARGET_URN}{enter}`)
    cy.findByTestId(`link-${REFRESH_TARGET_URN}`).click()

    cy.findByTestId("btn-accordion-open-close-all").click()
    cy.findByText("e2e-numbers-pre").click()

    cy.selectPDFTextElement("Three")

    cy.setPolarisInstrumentationGuid("PHASE_2")
    cy.findByTestId("btn-redact").click({ force: true })
    cy.findByTestId("btn-save-redaction-0").click()
    cy.findByTestId("pdfTab-spinner-0").should("exist")
    cy.findByTestId("pdfTab-spinner-0").should("not.exist")
    cy.findByTestId("div-pdfviewer-0").should("exist")
    cy.selectPDFTextElement("Four")
  })
})

export {}