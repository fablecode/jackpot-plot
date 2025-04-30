import {Component, ElementRef, Inject, OnInit, ViewChild} from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';
import {NgForOf, NgIf} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {TicketService} from '../../../core/services/ticket.service';
import {Ticket} from '../../../core/models/ticket.model';
import {AuthService} from '../../../core/services/auth.service';

@Component({
  selector: 'app-save-to-ticket-modal',
  imports: [
    NgForOf,
    NgIf,
    FormsModule
  ],
  templateUrl: './save-to-ticket-modal.component.html',
  styleUrl: './save-to-ticket-modal.component.scss'
})
export class SaveToTicketModalComponent implements OnInit {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  tickets: Ticket[] = [];

  addingNew = false;
  newTicketName = '';

  constructor(
    public dialogRef: MatDialogRef<SaveToTicketModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private ticketService: TicketService,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
        this.loadTickets();
    }

  close(): void {
    this.dialogRef.close();
  }

  createNewPlaylist(): void {
    // Handle new playlist logic
    alert('Create new playlist clicked!');
  }

  startAddingTicket(): void {
    this.addingNew = true;
    this.newTicketName = '';

    // Scroll to top of the container
    setTimeout(() => {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTo({ top: 0, behavior: 'smooth' });
      }
    }, 0);
  }

  cancelNewTicket(): void {
    this.addingNew = false;
    this.newTicketName = '';
  }

  addNewTicket(): void {
    const name = this.newTicketName.trim();
    if (name) {
      this.tickets.unshift({
        id: "werwrw",
        name,
        isPublic: false
      });
    }
    this.cancelNewTicket();
  }

  loadTickets(): void {
    // Load user tickets
    this.ticketService.getUserTickets().subscribe({
      next: (data : Ticket[]) => {
        this.tickets = data;
      },
      error: (error) => {
        console.error('Error fetching user tickets data:', error);
      }
    });
  }
}
