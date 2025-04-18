import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GeneratedNumbersMenuComponent } from './generated-numbers-menu.component';

describe('GeneratedNumbersMenuComponent', () => {
  let component: GeneratedNumbersMenuComponent;
  let fixture: ComponentFixture<GeneratedNumbersMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GeneratedNumbersMenuComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GeneratedNumbersMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
