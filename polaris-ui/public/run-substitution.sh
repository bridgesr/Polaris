#
PUBLIC_URL=/polaris-ui \
REACT_APP_CLIENT_ID=3649c1c8-00cf-4b8f-a671-304bc074937c \
REACT_APP_TENANT_ID=00dd0d1d-d7e6-4338-ac51-565339c7088c \
REACT_APP_GATEWAY_SCOPE=https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway/user_impersonation \
REACT_APP_GATEWAY_BASE_URL=https://polaris-dev-cmsproxy.azurewebsites.net \
REACT_APP_REDACTION_LOG_SCOPE=https://CPSGOVUK.onmicrosoft.com/fa-redaction-log-dev-reporting/user_impersonation \
REACT_APP_REDACTION_LOG_BASE_URL=https://fa-redaction-log-dev-reporting.azurewebsites.net \
REACT_APP_REAUTH_REDIRECT_URL=https://polaris-dev-cmsproxy.azurewebsites.net/polaris?polaris-ui-url= \
REACT_APP_MOCK_AUTH=false \
REACT_APP_MOCK_API_SOURCE= \
REACT_APP_MOCK_API_MAX_DELAY=1000 \
REACT_APP_AI_KEY=46e7f124-6e75-4f47-8512-e357d285bb43 \
REACT_APP_SURVEY_LINK="/abc" \
REACT_APP_REPORT_ISSUE=true \
REACT_APP_PRIVATE_BETA_USER_GROUP=1a9b08e8-5839-4953-a053-c1bc6dd02233 \
REACT_APP_PRIVATE_BETA_SIGN_UP_URL=https://forms.office.com/e/Af374akw0Q \
REACT_APP_PRIVATE_BETA_CHECK_IGNORE_USER=AutomationUser.ServiceTeam2@cps.gov.uk \
REACT_APP_IS_REDACTION_SERVICE_OFFLINE="true" \
node polaris-ui/subsititute-config.js