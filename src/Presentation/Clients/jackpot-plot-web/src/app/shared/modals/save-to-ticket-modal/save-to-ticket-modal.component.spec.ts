import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SaveToTicketModalComponent } from './save-to-ticket-modal.component';

describe('SaveToTicketModalComponent', () => {
  let component: SaveToTicketModalComponent;
  let fixture: ComponentFixture<SaveToTicketModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SaveToTicketModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SaveToTicketModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
