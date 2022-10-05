import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangeRoleDialogComponent } from './change-role-dialog.component';

describe('ChangeRoleDialogComponent', () => {
  let component: ChangeRoleDialogComponent;
  let fixture: ComponentFixture<ChangeRoleDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ChangeRoleDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChangeRoleDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
