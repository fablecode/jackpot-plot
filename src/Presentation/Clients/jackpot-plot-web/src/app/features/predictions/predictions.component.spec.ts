import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PredictionsComponent } from './predictions.component';

describe('PredictionsComponent', () => {
  let component: PredictionsComponent;
  let fixture: ComponentFixture<PredictionsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PredictionsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PredictionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
