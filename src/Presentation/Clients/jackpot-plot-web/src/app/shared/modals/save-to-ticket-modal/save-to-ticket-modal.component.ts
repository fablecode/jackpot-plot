import {Component, Inject} from '@angular/core';
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
import {NgForOf} from '@angular/common';

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
    NgForOf
  ],
  templateUrl: './save-to-ticket-modal.component.html',
  styleUrl: './save-to-ticket-modal.component.scss'
})
export class SaveToTicketModalComponent {
  playlists: Playlist[] = [
    { name: 'Watch Later', privacy: 'private', selected: false },
    { name: 'Life Tips', privacy: 'public', selected: true },
    { name: 'Games tracks', privacy: 'private', selected: false },
    { name: 'Funny Clips', privacy: 'private', selected: false },
    { name: 'Czech Songs', privacy: 'private', selected: false },
    { name: 'Food Recipes', privacy: 'private', selected: false },
    { name: 'Anime intros', privacy: 'private', selected: false },
    { name: 'favourites', privacy: 'private', selected: false }
  ];

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
}
