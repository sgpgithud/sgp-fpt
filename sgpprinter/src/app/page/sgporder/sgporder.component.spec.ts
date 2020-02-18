import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SgporderComponent } from './sgporder.component';

describe('SgporderComponent', () => {
  let component: SgporderComponent;
  let fixture: ComponentFixture<SgporderComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SgporderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SgporderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
