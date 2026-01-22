import { Component, DestroyRef, ElementRef, afterNextRender, inject, input, signal } from "@angular/core";

@Component({
  selector: "web-animated-counter",
  template: `{{ displayValue() }}`,
})
export class AnimatedCounter {
  private readonly el = inject(ElementRef);
  private readonly destroyRef = inject(DestroyRef);

  public readonly value = input.required<string>();
  public readonly duration = input(1500);

  protected readonly displayValue = signal("0");

  constructor() {
    afterNextRender(() => {
      const observer = new IntersectionObserver(
        ([entry]) => {
          if (entry.isIntersecting) {
            this.animate();
            observer.disconnect();
          }
        },
        { threshold: 0.5 },
      );

      observer.observe(this.el.nativeElement);
      this.destroyRef.onDestroy(() => observer.disconnect());
    });
  }

  private animate(): void {
    const raw = this.value();
    const match = raw.match(/^([\d.]+)(.*)$/);

    if (!match) {
      this.displayValue.set(raw);
      return;
    }

    const target = parseFloat(match[1]);
    const suffix = match[2] || "";
    const decimals = match[1].includes(".") ? (match[1].split(".")[1]?.length ?? 0) : 0;
    const start = performance.now();
    const duration = this.duration();

    const step = (now: number) => {
      const progress = Math.min((now - start) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3); // ease-out cubic
      const current = target * eased;

      this.displayValue.set(
        decimals ? current.toFixed(decimals) + suffix : Math.floor(current).toLocaleString() + suffix,
      );

      if (progress < 1) {
        requestAnimationFrame(step);
      } else {
        this.displayValue.set(raw);
      }
    };

    requestAnimationFrame(step);
  }
}
