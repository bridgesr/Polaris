import { CmsAuthError } from "../../../common/errors/CmsAuthError";
import { CmsAuthRedirectingError } from "../../../common/errors/CmsAuthRedirectingError";
import { REAUTH_REDIRECT_URL } from "../../../config";

const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";

const isCmsAuthFail = (response: Response) => response.status === 403;

const isAuthPageLoad = (window: Window) =>
  window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM);

const tryCleanRefreshInidcator = (window: Window) => {
  // clean the indicator from the browser address bar
  if (window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM)) {
    const nextUrl = window.location.href.replace(
      new RegExp(`[?|&]${REAUTHENTICATION_INDICATOR_QUERY_PARAM}`),
      ""
    );

    window.history.replaceState(null, "", nextUrl);
  }
};

const tryHandleFirstAuthFail = (response: Response, window: Window) => {
  if (isCmsAuthFail(response) && !isAuthPageLoad(window)) {
    const delimiter = window.location.href.includes("?") ? "&" : "?";

    const nextUrl = `${REAUTH_REDIRECT_URL}${encodeURIComponent(
      window.location.href + delimiter + REAUTHENTICATION_INDICATOR_QUERY_PARAM
    )}`;

    // Cypress tests are unhappy with the window navigation during the failure flow.
    //  For the time being, we let the test env file disable this step by setting
    //  REAUTH_REDIRECT_URL to blank. Not optimal but this flow is tested by e2e tests.
    if (REAUTH_REDIRECT_URL) {
      window.location.assign(nextUrl);
    }
    // stop any follow-on logic occurring
    throw new CmsAuthRedirectingError();
  }
  return null;
};

const tryHandleSecondAuthFail = (response: Response, window: Window) => {
  if (isCmsAuthFail(response) && isAuthPageLoad(window)) {
    tryCleanRefreshInidcator(window);
    throw new CmsAuthError("We think you are not logged in to CMS");
  }
  return null;
};

const handleNonAuthCall = (response: Response, window: Window) => {
  tryCleanRefreshInidcator(window);
  return response;
};

export const reauthenticationFilter = (response: Response, window: Window) =>
  tryHandleFirstAuthFail(response, window) ||
  tryHandleSecondAuthFail(response, window) ||
  handleNonAuthCall(response, window);