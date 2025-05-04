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
import {Play} from '../../../core/models/play';
import {Prediction} from '../../../core/models/prediction';
import {TicketInput} from '../../../core/models/input/ticket.input';

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
  @ViewChild('ticketInput') ticketInput!: ElementRef<HTMLInputElement>;

  tickets: Ticket[] = [];

  addingNew = false;
  newTicketName = '';

  constructor(
    public dialogRef: MatDialogRef<SaveToTicketModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { plays: Play[]; predictions: Prediction[] },
    private ticketService: TicketService,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
        this.loadTickets();
    }

  close(): void {
    this.dialogRef.close();
  }

  startAddingTicket(): void {
    this.addingNew = true;
    this.newTicketName = '';

    // Scroll to top of the container
    setTimeout(() => {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTo({ top: 0, behavior: 'smooth' });
      }

      if (this.ticketInput) {
        this.ticketInput.nativeElement.focus();
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
      const newTicket: TicketInput = {
        name: name//,
        //plays : this.createTicketInputPlays()
      };

      this.ticketService.addTicket(newTicket).subscribe({
        next: () => {
          this.cancelNewTicket();
          this.loadTickets();
        },
        error: (error) => {
          console.error('Failed to create ticket', error.error.errors);
        }
      });
    }
  }

  createTicketInputPlays(): any[] | { lineIndex: 0; numbers: number[] }[] | { lineIndex: number; numbers: number[] }[] {
    if (this.data.plays && this.data.plays.length > 0) {
      // User selected all plays
      return this.data.plays.map(play => ({
        lineIndex: play.playNumber,
        numbers: play.predictions.map(p => p.number)
      }));
    } else if (this.data.predictions && this.data.predictions.length > 0) {
      // User selected a specific play
      return [
        {
          lineIndex: 0,
          numbers: this.data.predictions.map(p => p.number)
        }
      ];
    }

    return []; // fallback if neither is present
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

