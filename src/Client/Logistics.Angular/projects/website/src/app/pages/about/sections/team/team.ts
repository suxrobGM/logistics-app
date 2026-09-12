import { Component } from "@angular/core";
import { Avatar, Icon } from "@logistics/shared/ui";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface TeamMember {
  name: string;
  role: string;
  bio: string;
  initials: string;
  /** Square headshot under `public/images/team/`. Null falls back to the initials avatar. */
  photo: string | null;
  linkedIn: string | null;
}

@Component({
  selector: "web-team",
  templateUrl: "./team.html",
  imports: [Avatar, Icon, ScrollAnimateDirective, SectionContainer, SectionHeader],
})
export class Team {
  protected readonly members: TeamMember[] = [
    {
      name: "Sukhrob Ilyosbekov",
      role: "CEO & Founder",
      bio: "Software engineer and founder. Spends his time building tools for the logistics industry.",
      initials: "SI",
      photo: "images/team/sukhrob-ilyosbekov.jpg",
      linkedIn: "https://www.linkedin.com/in/suxrobgm",
    },
    {
      name: "Olim Gulomov",
      role: "Co-Founder",
      bio: "Leads sales and growth. Works with carriers to bring LogisticsX to more fleets.",
      initials: "OG",
      photo: "images/team/olim-gulomov.jpg",
      linkedIn: "https://www.linkedin.com/in/olimgulomov",
    },
  ];
}
