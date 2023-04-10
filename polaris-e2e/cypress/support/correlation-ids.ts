export type CorrelationId = keyof typeof correlationIds

export const correlationIds = {
  BLANK: "E2E00000-0000-0000-0000-000000000000",
  PHASE_1: "E2E00000-0000-0000-0000-000000000001",
  PHASE_2: "E2E00000-0000-0000-0000-000000000002",
}