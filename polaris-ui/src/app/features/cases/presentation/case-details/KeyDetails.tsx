import {
  CommonDateTimeFormats,
  formatDate,
  getAgeFromIsoDate,
} from "../../../../common/utils/dates";
import {
  CaseDetails,
  DefendantDetails,
} from "../../domain/gateway/CaseDetails";
import { LinkButton } from "../../../../../app/common/presentation/components/LinkButton";
import classes from "./index.module.scss";

export const KeyDetails: React.FC<{
  caseDetails: CaseDetails;
  isMultipleDefendantsOrCharges: boolean;
  handleOpenPdf: () => void;
  dacDocumentId: string;
}> = ({
  caseDetails,
  isMultipleDefendantsOrCharges,
  handleOpenPdf,
  dacDocumentId,
}) => {
  const getOrderedDefendantsList = (caseDetails: CaseDetails) => {
    const { defendants } = caseDetails;
    defendants.sort(
      (a, b) => a.defendantDetails.listOrder - b.defendantDetails.listOrder
    );
    return defendants;
  };

  const getDefendantName = (defendantDetail: DefendantDetails) => {
    if (defendantDetail.type === "Organisation") {
      return defendantDetail.organisationName;
    }
    return `${defendantDetail.surname}, ${defendantDetail.firstNames}`;
  };

  const defendantsList = getOrderedDefendantsList(caseDetails);

  return (
    <div>
      <h1
        className={`govuk-heading-m ${classes.uniqueReferenceNumber}`}
        data-testid="txt-case-urn"
      >
        {caseDetails.uniqueReferenceNumber}
      </h1>

      {isMultipleDefendantsOrCharges && (
        <>
          <ul
            className={classes.defendantsList}
            data-testid="list-defendant-names"
          >
            {defendantsList.map(({ defendantDetails }) => (
              <li key={defendantDetails.id}>
                {getDefendantName(defendantDetails)}
              </li>
            ))}
          </ul>
          {dacDocumentId && (
            <LinkButton
              dataTestId="link-defendant-details"
              className={classes.defendantDetailsLink}
              onClick={handleOpenPdf}
            >
              {`View ${defendantsList.length} ${
                defendantsList.length > 1 ? "defendants" : "defendant"
              } and charges`}
            </LinkButton>
          )}
        </>
      )}
      {!isMultipleDefendantsOrCharges && (
        <div
          className={classes.defendantDetails}
          data-testid="defendant-details"
        >
          <span
            className={`govuk-heading-s ${classes.defendantName}`}
            data-testid="txt-defendant-name"
          >
            {getDefendantName(caseDetails.leadDefendantDetails)}
          </span>
          {caseDetails.leadDefendantDetails.type !== "Organisation" && (
            <span
              className={`${classes.defendantDOB}`}
              data-testid="txt-defendant-DOB"
            >
              DOB:{" "}
              {formatDate(
                caseDetails.leadDefendantDetails.dob,
                CommonDateTimeFormats.ShortDateTextMonth
              )}
              . Age: {getAgeFromIsoDate(caseDetails.leadDefendantDetails.dob)}
            </span>
          )}
          {caseDetails.leadDefendantDetails.youth && (
            <span>
              <b>Youth Offender</b>
            </span>
          )}
        </div>
      )}
    </div>
  );
};
