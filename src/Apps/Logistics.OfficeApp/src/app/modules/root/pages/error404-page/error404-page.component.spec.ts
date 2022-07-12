import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Error404PageComponent } from './error404-page.component';

describe('Error404PageComponent', () => {
  let component: Error404PageComponent;
  let fixture: ComponentFixture<Error404PageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ Error404PageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Error404PageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
