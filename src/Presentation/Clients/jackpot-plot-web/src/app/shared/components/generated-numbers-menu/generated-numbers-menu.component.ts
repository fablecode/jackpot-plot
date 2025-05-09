import {Component, Input, OnInit} from '@angular/core';
import {NgClass, NgForOf} from "@angular/common";
import {Play} from '../../../core/models/play';
import {Prediction} from '../../../core/models/prediction';
import { MatDialog } from '@angular/material/dialog';
import {SaveToTicketModalComponent} from '../../modals/save-to-ticket-modal/save-to-ticket-modal.component';
import {AuthService} from '../../../core/services/auth.service';

@Component({
  selector: 'app-generated-numbers-menu',
  imports: [
    NgForOf,
    NgClass
  ],
  templateUrl: './generated-numbers-menu.component.html',
  styleUrl: './generated-numbers-menu.component.scss'
})
export class GeneratedNumbersMenuComponent implements OnInit {
  @Input() lotteryId: number;
  @Input() plays: Play[] | null = null;
  @Input() predictions: Prediction[] | null = null;

  constructor(private dialog: MatDialog, private authService: AuthService) {}

  menuItems = [
    {
      title: 'Save to ticket',
      icon: 'ki-bookmark',
      link: '/metronic/tailwind/demo1/account/activity',
      modalToggle: false
    },
    {
      title: 'Add to kanban board',
      icon: 'ki-burger-menu-4',
      link: '#',
      modalToggle: '#share_profile_modal'
    }
  ];

  onMenuClick(item: any, event: MouseEvent): void {
    event.preventDefault(); // Prevent default anchor behavior

    if(!this.authService.isAuthenticated()) {
      this.authService.login().then(() => {
        // Redirect will happen in Keycloak
      });
    } else {
      if (item.title === 'Save to ticket') {
        this.openSaveModal();
      }
      // Handle other actions here
    }
  }

  openSaveModal(): void {
    this.dialog.open(SaveToTicketModalComponent, {
      width: '400px',
      data: {
        lotteryId: this.lotteryId,
        plays: this.plays,
        predictions: this.predictions
      }
    });
  }

  ngOnInit() {
    this.plays = this.plays ?? [];
    this.predictions = this.predictions ?? [];
  }
}
