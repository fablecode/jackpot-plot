import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NumberGeneratorComponent } from './number-generator.component';

describe('NumberGeneratorComponent', () => {
  let component: NumberGeneratorComponent;
  let fixture: ComponentFixture<NumberGeneratorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NumberGeneratorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NumberGeneratorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
