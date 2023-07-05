import {ComponentFixture, TestBed} from '@angular/core/testing';

import {TruckDashboardComponent} from './truck-dashboardcomponent';

describe('TruckComponent', () => {
  let component: TruckDashboardComponent;
  let fixture: ComponentFixture<TruckDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TruckDashboardComponent],
    })
        .compileComponents();

    fixture = TestBed.createComponent(TruckDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
