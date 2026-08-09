/**
 * Swallow-and-report-nothing wrappers shared by the copilot and dispatch chat API services: their
 * stores stay pure state orchestration and never touch try/catch, and the global
 * errorHandlerInterceptor has already toasted whatever went wrong.
 */
export async function orNull<T>(call: Promise<T>): Promise<T | null> {
  try {
    return await call;
  } catch {
    return null;
  }
}

export async function succeeded(call: Promise<unknown>): Promise<boolean> {
  try {
    await call;
    return true;
  } catch {
    return false;
  }
}
