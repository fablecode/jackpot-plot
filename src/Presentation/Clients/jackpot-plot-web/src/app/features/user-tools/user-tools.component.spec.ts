import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserToolsComponent } from './user-tools.component';

describe('UserToolsComponent', () => {
  let component: UserToolsComponent;
  let fixture: ComponentFixture<UserToolsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserToolsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserToolsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
