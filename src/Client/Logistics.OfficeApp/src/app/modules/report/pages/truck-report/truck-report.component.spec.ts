import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TruckReportComponent } from './truck-report.component';

describe('TruckComponent', () => {
  let component: TruckReportComponent;
  let fixture: ComponentFixture<TruckReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TruckReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TruckReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
