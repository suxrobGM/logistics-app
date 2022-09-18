import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TruckStatsComponent } from './truck-stats.component';

describe('TruckComponent', () => {
  let component: TruckStatsComponent;
  let fixture: ComponentFixture<TruckStatsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TruckStatsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TruckStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
