import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TruckComponent } from './truck.component';

describe('TruckComponent', () => {
  let component: TruckComponent;
  let fixture: ComponentFixture<TruckComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TruckComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TruckComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
