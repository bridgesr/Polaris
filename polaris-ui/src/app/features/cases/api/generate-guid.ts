import { v4 as uuidv4, validate } from "uuid";

declare global {
  var __POLARIS_INSTRUMENTATION_GUID__: string;
}

export const generateGuid = () => {
  if (window.__POLARIS_INSTRUMENTATION_GUID__) {
    if (!validate(window.__POLARIS_INSTRUMENTATION_GUID__)) {
      throw new Error(
        "The __POLARIS_INSTRUMENTATION_GUID__ window variable has been set but is not a valid UUID"
      );
    }
    return window.__POLARIS_INSTRUMENTATION_GUID__;
  }
  return uuidv4();
};
