import { Component } from "@angular/core";
import { SectionContainer, SectionHeader } from "@/shared/components";

interface Testimonial {
  quote: string;
  author: string;
  role: string;
  company: string;
}

@Component({
  selector: "web-testimonials",
  templateUrl: "./testimonials.html",
  imports: [SectionContainer, SectionHeader],
})
export class Testimonials {
  protected readonly testimonials: Testimonial[] = [
    {
      quote:
        "Logistics TMS has transformed how we manage our fleet. We've reduced empty miles by 25% and our dispatchers can handle twice the workload.",
      author: "Mike Johnson",
      role: "Operations Director",
      company: "Midwest Freight Solutions",
    },
    {
      quote:
        "The driver app is a game-changer. POD capture, navigation, and communication all in one place. Our drivers love it.",
      author: "Sarah Martinez",
      role: "Fleet Manager",
      company: "Pacific Coast Trucking",
    },
    {
      quote:
        "We saw ROI within the first month. Automated invoicing alone saved us 20 hours per week in administrative work.",
      author: "David Chen",
      role: "CEO",
      company: "Summit Logistics Group",
    },
  ];
}
