import { Injectable, inject } from "@angular/core";
import {
  Api,
  approveDispatchDecision,
  cancelDispatchSession,
  getDispatchSessionById,
  getDispatchSessions,
  getPendingDecisions,
  rejectDispatchDecision,
  runDispatchAgent,
} from "@logistics/shared/api";
import type {
  DispatchAgentMode,
  DispatchDecisionDto,
  DispatchSessionDto,
  DispatchSessionDtoPagedResponse,
  GuidResult,
} from "@logistics/shared/api";

@Injectable({ providedIn: "root" })
export class DispatchAgentService {
  private readonly api = inject(Api);

  async getSessions(page = 1, pageSize = 10): Promise<DispatchSessionDtoPagedResponse> {
    return this.api.invoke(getDispatchSessions, { Page: page, PageSize: pageSize });
  }

  async getSession(id: string): Promise<DispatchSessionDto> {
    return this.api.invoke(getDispatchSessionById, { sessionId: id });
  }

  async getPendingDecisions(): Promise<DispatchDecisionDto[]> {
    return this.api.invoke(getPendingDecisions);
  }

  async run(mode: DispatchAgentMode): Promise<GuidResult> {
    return this.api.invoke(runDispatchAgent, { body: { mode } });
  }

  async approveDecision(id: string): Promise<void> {
    return this.api.invoke(approveDispatchDecision, { decisionId: id });
  }

  async rejectDecision(id: string, reason?: string): Promise<void> {
    return this.api.invoke(rejectDispatchDecision, {
      decisionId: id,
      body: { decisionId: id, reason },
    });
  }

  async cancelSession(id: string): Promise<void> {
    return this.api.invoke(cancelDispatchSession, { sessionId: id });
  }
}
