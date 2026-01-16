import type { Routes } from "@angular/router";
import { Permission } from "@logistics/shared/models";
import { authGuard } from "@/core/auth";
import { ConversationComponent } from "./conversation/conversation";
import { MessagesListComponent } from "./messages-list/messages-list";

export const messagesRoutes: Routes = [
  {
    path: "",
    component: MessagesListComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permission.Message.View,
    },
  },
  {
    path: ":id",
    component: ConversationComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Conversation",
      permission: Permission.Message.View,
    },
  },
];
