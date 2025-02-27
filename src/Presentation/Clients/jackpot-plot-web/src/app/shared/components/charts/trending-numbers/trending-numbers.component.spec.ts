import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrendingNumbersComponent } from './trending-numbers.component';

describe('TrendingNumbersComponent', () => {
  let component: TrendingNumbersComponent;
  let fixture: ComponentFixture<TrendingNumbersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrendingNumbersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TrendingNumbersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
