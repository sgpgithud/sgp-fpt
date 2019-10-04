import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SgpmailerComponent } from './sgpmailer.component';

describe('SgpmailerComponent', () => {
  let component: SgpmailerComponent;
  let fixture: ComponentFixture<SgpmailerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SgpmailerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SgpmailerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
