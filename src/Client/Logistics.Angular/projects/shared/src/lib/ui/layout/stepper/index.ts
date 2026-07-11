import { UiStep } from "./step";
import { UiStepPanel } from "./step-panel";
import { UiStepContent, UiStepList, UiStepPanels, UiStepper } from "./stepper";

export * from "./step";
export * from "./step-panel";
export * from "./stepper";

/** All five stepper elements plus the `uiStepContent` template directive. */
export const UiStepperImports = [
  UiStepper,
  UiStepList,
  UiStep,
  UiStepPanels,
  UiStepPanel,
  UiStepContent,
] as const;
