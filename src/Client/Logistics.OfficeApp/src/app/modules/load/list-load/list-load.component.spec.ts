import {ComponentFixture, TestBed} from '@angular/core/testing';

import {ListLoadComponent} from './list-load.component';

describe('ListLoadComponent', () => {
  let component: ListLoadComponent;
  let fixture: ComponentFixture<ListLoadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ListLoadComponent],
    })
        .compileComponents();

    fixture = TestBed.createComponent(ListLoadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
