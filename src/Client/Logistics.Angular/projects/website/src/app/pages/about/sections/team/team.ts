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
      name: "Michael Chen",
      role: "CEO & Co-Founder",
      bio: "20+ years in logistics technology. Previously VP at a Fortune 500 trucking company.",
      initials: "MC",
      linkedIn: "#",
    },
    {
      name: "Sarah Rodriguez",
      role: "CTO & Co-Founder",
      bio: "Former engineering lead at a major SaaS company. Expert in scalable cloud architecture.",
      initials: "SR",
      linkedIn: "#",
    },
    {
      name: "David Thompson",
      role: "VP of Product",
      bio: "Product leader with a passion for solving complex problems in the transportation industry.",
      initials: "DT",
      linkedIn: "#",
    },
    {
      name: "Emily Watson",
      role: "VP of Customer Success",
      bio: "Dedicated to ensuring every customer achieves their goals with our platform.",
      initials: "EW",
      linkedIn: "#",
    },
    {
      name: "James Miller",
      role: "VP of Engineering",
      bio: "Building high-performance teams that deliver reliable, scalable software solutions.",
      initials: "JM",
      linkedIn: "#",
    },
    {
      name: "Lisa Park",
      role: "VP of Sales",
      bio: "Connecting trucking companies with solutions that transform their operations.",
      initials: "LP",
      linkedIn: "#",
    },
  ];
}
