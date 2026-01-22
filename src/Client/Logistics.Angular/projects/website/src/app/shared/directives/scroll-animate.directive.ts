import { DestroyRef, Directive, ElementRef, afterNextRender, inject, input } from "@angular/core";

type AnimationType = "fade-up" | "fade-in" | "fade-left" | "fade-right" | "scale-up";

const INITIAL_TRANSFORMS: Record<AnimationType, string> = {
  "fade-up": "translateY(30px)",
  "fade-in": "none",
  "fade-left": "translateX(-30px)",
  "fade-right": "translateX(30px)",
  "scale-up": "scale(0.95)",
};

@Directive({
  selector: "[webScrollAnimate]",
})
export class ScrollAnimateDirective {
  private readonly el = inject(ElementRef);
  private readonly destroyRef = inject(DestroyRef);

  public readonly animation = input<AnimationType>("fade-up", { alias: "webScrollAnimate" });
  public readonly delay = input(0);
  public readonly threshold = input(0.1);

  constructor() {
    afterNextRender(() => {
      const element = this.el.nativeElement as HTMLElement;

      // Set initial hidden state
      element.style.opacity = "0";
      element.style.transition = "opacity 0.6s ease-out, transform 0.6s ease-out";
      element.style.transform = INITIAL_TRANSFORMS[this.animation()];

      // Observe for intersection
      const observer = new IntersectionObserver(
        ([entry]) => {
          if (entry.isIntersecting) {
            setTimeout(() => {
              element.style.opacity = "1";
              element.style.transform = "none";
            }, this.delay());
            observer.disconnect();
          }
        },
        { threshold: this.threshold() },
      );

      observer.observe(element);
      this.destroyRef.onDestroy(() => observer.disconnect());
    });
  }
}
