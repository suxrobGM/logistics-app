import type { Routes } from "@angular/router";
import { ContactSubmissionsList } from "./contact-submissions-list/contact-submissions-list";

export const contactSubmissionsRoutes: Routes = [
  {
    path: "",
    component: ContactSubmissionsList,
    data: {
      breadcrumb: "Contact Submissions",
    },
  },
];
