import {Component, ElementRef, Inject, ViewChild} from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle
} from '@angular/material/dialog';
import {MatListOption, MatSelectionList} from '@angular/material/list';
import {MatIcon} from '@angular/material/icon';
import {MatButton} from '@angular/material/button';
import {NgClass, NgForOf, NgIf} from '@angular/common';
import {FormsModule} from '@angular/forms';

interface Playlist {
  name: string;
  privacy: 'private' | 'public';
  selected: boolean;
}

@Component({
  selector: 'app-save-to-ticket-modal',
  imports: [
    MatSelectionList,
    MatListOption,
    MatIcon,
    MatDialogActions,
    MatDialogContent,
    MatButton,
    MatDialogTitle,
    NgForOf,
    NgIf,
    NgClass,
    FormsModule
  ],
  templateUrl: './save-to-ticket-modal.component.html',
  styleUrl: './save-to-ticket-modal.component.scss'
})
export class SaveToTicketModalComponent {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  playlists: Playlist[] = [];

  addingNew = false;
  newTicketName = '';

  constructor(
    public dialogRef: MatDialogRef<SaveToTicketModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

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
      this.playlists.unshift({
        name,
        privacy: 'private',
        selected: true
      });
    }
    this.cancelNewTicket();
  }
}
