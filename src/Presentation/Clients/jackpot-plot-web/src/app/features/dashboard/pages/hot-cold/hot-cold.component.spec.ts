import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HotColdComponent } from './hot-cold.component';

describe('HotColdComponent', () => {
  let component: HotColdComponent;
  let fixture: ComponentFixture<HotColdComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HotColdComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HotColdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
