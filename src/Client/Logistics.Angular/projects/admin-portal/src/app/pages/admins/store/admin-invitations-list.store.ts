import { getAdminInvitations, type InvitationDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the pending admin invitations list.
 */
export const AdminInvitationsListStore = createListStore<InvitationDto>(getAdminInvitations, {
  defaultPageSize: 10,
});
