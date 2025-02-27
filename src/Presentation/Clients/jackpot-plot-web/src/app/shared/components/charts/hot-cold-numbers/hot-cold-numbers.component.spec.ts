import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HotColdNumbersComponent } from './hot-cold-numbers.component';

describe('HotColdNumbersComponentComponent', () => {
  let component: HotColdNumbersComponent;
  let fixture: ComponentFixture<HotColdNumbersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HotColdNumbersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HotColdNumbersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
