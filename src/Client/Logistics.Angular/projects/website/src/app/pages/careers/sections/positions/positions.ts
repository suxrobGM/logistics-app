import { Component, signal } from "@angular/core";
import { FilterTabs, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { ButtonModule } from "primeng/button";

interface JobPosition {
  id: string;
  title: string;
  department: string;
  location: string;
  type: string;
}

@Component({
  selector: "web-positions",
  templateUrl: "./positions.html",
  imports: [SectionContainer, SectionHeader, ScrollAnimateDirective, ButtonModule, FilterTabs],
})
export class Positions {
  protected readonly departments = [
    "All Departments",
    "Engineering",
    "Product",
    "Sales",
    "Customer Success",
    "Operations",
  ];

  protected readonly selectedDepartment = signal("All Departments");

  protected readonly positions: JobPosition[] = [
    {
      id: "1",
      title: "Senior Full Stack Engineer",
      department: "Engineering",
      location: "Remote (US)",
      type: "Full-time",
    },
    {
      id: "2",
      title: "Staff Backend Engineer",
      department: "Engineering",
      location: "Houston, TX",
      type: "Full-time",
    },
    {
      id: "3",
      title: "Mobile Engineer (React Native)",
      department: "Engineering",
      location: "Remote (US)",
      type: "Full-time",
    },
    {
      id: "4",
      title: "Product Manager",
      department: "Product",
      location: "Houston, TX",
      type: "Full-time",
    },
    {
      id: "5",
      title: "UX Designer",
      department: "Product",
      location: "Remote (US)",
      type: "Full-time",
    },
    {
      id: "6",
      title: "Account Executive",
      department: "Sales",
      location: "Houston, TX",
      type: "Full-time",
    },
    {
      id: "7",
      title: "Customer Success Manager",
      department: "Customer Success",
      location: "Remote (US)",
      type: "Full-time",
    },
    {
      id: "8",
      title: "Implementation Specialist",
      department: "Operations",
      location: "Houston, TX",
      type: "Full-time",
    },
  ];

  protected get filteredPositions(): JobPosition[] {
    if (this.selectedDepartment() === "All Departments") {
      return this.positions;
    }
    return this.positions.filter((p) => p.department === this.selectedDepartment());
  }

  protected selectDepartment(dept: string): void {
    this.selectedDepartment.set(dept);
  }
}
