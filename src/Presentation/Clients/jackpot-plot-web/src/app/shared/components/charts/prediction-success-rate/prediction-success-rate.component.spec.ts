import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PredictionSuccessRateComponent } from './prediction-success-rate.component';

describe('PredictionSuccessRateComponent', () => {
  let component: PredictionSuccessRateComponent;
  let fixture: ComponentFixture<PredictionSuccessRateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PredictionSuccessRateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PredictionSuccessRateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
