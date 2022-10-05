import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditLoadComponent } from './edit-load.component';

describe('EditLoadComponent', () => {
  let component: EditLoadComponent;
  let fixture: ComponentFixture<EditLoadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditLoadComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditLoadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
