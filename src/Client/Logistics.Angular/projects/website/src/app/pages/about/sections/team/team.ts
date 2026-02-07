import { Component } from "@angular/core";
import { Avatar, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface TeamMember {
  name: string;
  role: string;
  bio: string;
  initials: string;
  linkedIn?: string;
}

@Component({
  selector: "web-team",
  templateUrl: "./team.html",
  imports: [SectionContainer, SectionHeader, ScrollAnimateDirective, Avatar],
})
export class Team {
  protected readonly members: TeamMember[] = [
    {
      name: "Sukhrob Ilyosbekov",
      role: "CEO & Founder",
      bio: "Software engineer and entrepreneur passionate about building modern tools for the logistics industry.",
      initials: "SI",
      linkedIn: "https://www.linkedin.com/in/suxrobgm",
    },
    {
      name: "Co-Founder",
      role: "Co-Founder",
      bio: "Details coming soon.",
      initials: "CF",
    },
  ];
}
