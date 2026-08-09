import { TestBed } from "@angular/core/testing";
import {
  Api,
  approveAIDispatchDecision,
  approveCopilotDecision,
  rejectAIDispatchDecision,
  rejectCopilotDecision,
  type AgentDecisionDto,
} from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import { DecisionActionsService } from "./decision-actions.service";

function decision(overrides: Partial<AgentDecisionDto> = {}): AgentDecisionDto {
  return { id: "decision-1", toolName: "assign_load_to_truck", ...overrides };
}

describe("DecisionActionsService - the configure() endpoint switch", () => {
  let apiInvoke: ReturnType<typeof vi.fn>;
  let service: DecisionActionsService;

  beforeEach(() => {
    apiInvoke = vi.fn().mockResolvedValue(undefined);
    // Immediately accept, as if the user clicked through the confirm dialog - keeps these tests
    // focused on the endpoint switch, not the confirm gesture itself (covered below).
    const confirm = vi.fn((options: { accept: () => void }) => options.accept());

    TestBed.configureTestingModule({
      providers: [
        DecisionActionsService,
        { provide: Api, useValue: { invoke: apiInvoke } },
        { provide: ToastService, useValue: { confirm, showSuccess: vi.fn() } },
      ],
    });
    service = TestBed.inject(DecisionActionsService);
  });

  it("by default (no configure() call) approve() hits the dispatch endpoint", () => {
    service.approve(decision());

    expect(apiInvoke).toHaveBeenCalledTimes(1);
    expect(apiInvoke).toHaveBeenCalledWith(approveAIDispatchDecision, { decisionId: "decision-1" });
  });

  it("by default (no configure() call) confirmReject() hits the dispatch endpoint", async () => {
    service.reject(decision());
    await service.confirmReject("not needed");

    expect(apiInvoke).toHaveBeenCalledTimes(1);
    expect(apiInvoke).toHaveBeenCalledWith(rejectAIDispatchDecision, {
      decisionId: "decision-1",
      body: { reason: "not needed" },
    });
  });

  it("after configure('copilot'), approve() hits the copilot endpoint", () => {
    service.configure("copilot");
    service.approve(decision());

    expect(apiInvoke).toHaveBeenCalledTimes(1);
    expect(apiInvoke).toHaveBeenCalledWith(approveCopilotDecision, { decisionId: "decision-1" });
  });

  it("after configure('copilot'), confirmReject() hits the copilot endpoint", async () => {
    service.configure("copilot");
    service.reject(decision());
    await service.confirmReject("wrong truck");

    expect(apiInvoke).toHaveBeenCalledTimes(1);
    expect(apiInvoke).toHaveBeenCalledWith(rejectCopilotDecision, {
      decisionId: "decision-1",
      body: { reason: "wrong truck" },
    });
  });

  it("reject() defers the API call until confirmReject() is invoked", () => {
    service.reject(decision());

    expect(apiInvoke).not.toHaveBeenCalled();
    expect(service.showRejectDialog()).toBe(true);
    expect(service.pendingDecision()).toEqual(decision());
  });

  it("confirmReject() with no pending decision is a no-op", async () => {
    await service.confirmReject("reason");

    expect(apiInvoke).not.toHaveBeenCalled();
  });
});

describe("DecisionActionsService.approve() - the confirm gesture guards the API call", () => {
  it("does not call the API merely because approve() ran - only once the user accepts", () => {
    const apiInvoke = vi.fn().mockResolvedValue(undefined);
    const confirm = vi.fn(); // never calls accept - the user hasn't decided yet

    TestBed.configureTestingModule({
      providers: [
        DecisionActionsService,
        { provide: Api, useValue: { invoke: apiInvoke } },
        { provide: ToastService, useValue: { confirm, showSuccess: vi.fn() } },
      ],
    });
    const service = TestBed.inject(DecisionActionsService);

    service.approve(decision());

    expect(confirm).toHaveBeenCalledTimes(1);
    expect(apiInvoke).not.toHaveBeenCalled();
  });
});
