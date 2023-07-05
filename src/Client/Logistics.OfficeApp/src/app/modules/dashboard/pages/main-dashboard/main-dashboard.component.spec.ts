import {ComponentFixture, TestBed} from '@angular/core/testing';

import {MainDashboardComponent} from './main-dashboardcomponent';

describe('ReportPageComponent', () => {
  let component: MainDashboardComponent;
  let fixture: ComponentFixture<MainDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MainDashboardComponent],
    })
        .compileComponents();

    fixture = TestBed.createComponent(MainDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
