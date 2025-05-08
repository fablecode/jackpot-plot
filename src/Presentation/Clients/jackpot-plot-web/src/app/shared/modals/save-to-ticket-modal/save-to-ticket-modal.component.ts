import {Component, ElementRef, Inject, OnInit, ViewChild} from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';
import {NgForOf, NgIf} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {TicketService} from '../../../core/services/ticket.service';
import {Ticket} from '../../../core/models/ticket.model';
import {Play} from '../../../core/models/play';
import {Prediction} from '../../../core/models/prediction';
import {TicketInput} from '../../../core/models/input/ticket.input';
import {TicketPlaysService} from '../../../core/services/ticket.plays.service';
import {MatSnackBar, MatSnackBarModule} from '@angular/material/snack-bar';

@Component({
  selector: 'app-save-to-ticket-modal',
  imports: [
    NgForOf,
    NgIf,
    FormsModule,
    MatSnackBarModule
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
  checkedTicketIds = new Set<string>();
  ticketPlayMap = new Map<string, string[]>();

  constructor(
    public dialogRef: MatDialogRef<SaveToTicketModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { plays: Play[]; predictions: Prediction[] },
    private ticketService: TicketService,
    private ticketPlaysService: TicketPlaysService,
    private snackBar: MatSnackBar
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
        name: name
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

  onTicketCheckboxChange(ticket: Ticket, event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    const plays = this.createTicketInputPlays();

    if (checkbox.checked) {
      this.ticketPlaysService.addPlaysToTicket(ticket.id, plays).subscribe({
        next: (createdPlayIds: string[]) => {
          this.checkedTicketIds.add(ticket.id);
          this.ticketPlayMap.set(ticket.id, createdPlayIds);

          this.showUndoSnackbar(`Added to ${ticket.name}`, () => {
            this.removePlays(ticket, createdPlayIds);
          });

          const updatedTicket = this.tickets.find(t => t.id === ticket.id);
          if (updatedTicket) {
            updatedTicket.playCount += createdPlayIds.length;
          }
        },
        error: (err) => console.error('Failed to add plays', err)
      });
    } else {
      const playIds = this.ticketPlayMap.get(ticket.id) || [];

      this.ticketPlaysService.removePlaysFromTicket(ticket.id, playIds).subscribe({
        next: () => {
          this.checkedTicketIds.delete(ticket.id);
          this.ticketPlayMap.delete(ticket.id);

          this.showUndoSnackbar(`Removed from ${ticket.name}`, () => {
            this.addPlays(ticket);
          });

          const updatedTicket = this.tickets.find(t => t.id === ticket.id);
          if (updatedTicket) {
            updatedTicket.playCount -= playIds.length;
          }
        },
        error: (err) => console.error('Failed to remove plays', err)
      });
    }
  }

  isTicketChecked(ticketId: string): boolean {
    return this.checkedTicketIds.has(ticketId);
  }

  getCheckboxTooltip(ticketId: string): string {
    return this.isTicketChecked(ticketId)
      ? 'Remove plays from this ticket'
      : 'Add plays to this ticket';
  }

  showUndoSnackbar(message: string, undoAction: () => void): void {
    const snackBarRef = this.snackBar.open(message, 'Undo', {
      duration: 4000,
      horizontalPosition: 'start',
      verticalPosition: 'bottom',
      panelClass: ['custom-snackbar']
    });

    snackBarRef.onAction().subscribe(() => {
      undoAction();
    });
  }

  addPlays(ticket: Ticket): void {
    const plays = this.createTicketInputPlays();
    this.ticketPlaysService.addPlaysToTicket(ticket.id, plays).subscribe({
      next: (playIds: string[]) => {
        this.checkedTicketIds.add(ticket.id); // ✅ re-check the box
        this.ticketPlayMap.set(ticket.id, playIds);

        const updatedTicket = this.tickets.find(t => t.id === ticket.id);
        if (updatedTicket) {
          updatedTicket.playCount += playIds.length;
        }
      },
      error: (err) => console.error('Failed to re-add plays', err)
    });
  }

  removePlays(ticket: Ticket, playIds: string[]): void {
    this.ticketPlaysService.removePlaysFromTicket(ticket.id, playIds).subscribe({
      next: () => {
        this.checkedTicketIds.delete(ticket.id); // ✅ uncheck the box
        this.ticketPlayMap.delete(ticket.id);

        const updatedTicket = this.tickets.find(t => t.id === ticket.id);
        if (updatedTicket) {
          updatedTicket.playCount -= playIds.length;
        }
      },
      error: (err) => console.error('Failed to undo add', err)
    });
  }
}

