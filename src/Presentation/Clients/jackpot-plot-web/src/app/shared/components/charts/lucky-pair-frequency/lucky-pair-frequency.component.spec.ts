import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LuckyPairFrequencyComponent } from './lucky-pair-frequency.component';

describe('LuckyPairFrequencyComponent', () => {
  let component: LuckyPairFrequencyComponent;
  let fixture: ComponentFixture<LuckyPairFrequencyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LuckyPairFrequencyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LuckyPairFrequencyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
