import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WinningNumberFrequencyComponent } from './winning-number-frequency.component';

describe('WinningNumberFrequencyComponent', () => {
  let component: WinningNumberFrequencyComponent;
  let fixture: ComponentFixture<WinningNumberFrequencyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WinningNumberFrequencyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WinningNumberFrequencyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
