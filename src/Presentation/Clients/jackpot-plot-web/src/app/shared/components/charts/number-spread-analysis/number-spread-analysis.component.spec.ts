import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NumberSpreadAnalysisComponent } from './number-spread-analysis.component';

describe('NumberSpreadAnalysisComponent', () => {
  let component: NumberSpreadAnalysisComponent;
  let fixture: ComponentFixture<NumberSpreadAnalysisComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NumberSpreadAnalysisComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NumberSpreadAnalysisComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
