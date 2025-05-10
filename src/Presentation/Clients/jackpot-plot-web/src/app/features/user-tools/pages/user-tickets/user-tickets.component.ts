import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {Ticket} from '../../../../core/models/ticket.model';
import {TicketService} from '../../../../core/services/ticket.service';
import {trigger, transition, style, animate} from '@angular/animations';
import {MatPaginator, MatPaginatorModule} from '@angular/material/paginator';
import { PageEvent } from '@angular/material/paginator';



@Component({
  selector: 'app-user-tickets',
  imports: [CommonModule, FormsModule, MatPaginator, MatPaginatorModule],
  templateUrl: './user-tickets.component.html',
  styleUrl: './user-tickets.component.scss',
  animations: [
    trigger('fadeInOut', [
      transition(':enter', [   // when added to DOM
        style({ opacity: 0 }),
        animate('300ms ease-in', style({ opacity: 1 })),
      ]),
      transition(':leave', [   // when removed from DOM
        animate('300ms ease-out', style({ opacity: 0 })),
      ]),
    ]),
  ]
})
export class UserTicketsComponent implements OnInit {

  tickets: Ticket[] = [];
  totalItems = 0;

  page = 1;
  pageSize = 5;
  pageSizes = [5, 10, 25, 100];

  loading: boolean;

  searchQuery: string = '';

  constructor(private ticketsService: TicketService) {}

  ngOnInit() {
    this.loadTickets();
  }

  onSearchChange(): void {
    // You can debounce or trigger a filtered API call here
    console.log('Search:', this.searchQuery);
  }

  loadTickets() {
    this.loading = true

    this.ticketsService.getTickets(this.page, this.pageSize).subscribe({
      next: (res: { paginatedTickets: Ticket[], total: number })=> {
          this.tickets = res.paginatedTickets;
          this.totalItems = res.total;

        console.log(`Number of tickets in collection: ${this.tickets.length}`);
      },
      error: (error) => {
        console.error('Error fetching user tickets:', error);
      },
      complete: () => {
        this.loading = false;
      }
    })
  }

  onPageChange(page: number): void {
    console.log(`Page changed to: ${page}`);
    this.page = page;
    this.loadTickets();
  }

  onMaterialPageChange(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.page = event.pageIndex + 1;
    this.loadTickets();
  }

  setPageSize(size: number) {
    console.log('Changing page size to', size);
    this.pageSize = size;
    this.page = 1;
    this.loadTickets();
  }

  get ticketRange(): string {
    const start = (this.page - 1) * this.pageSize + 1;
    const end = Math.min(this.page * this.pageSize, this.totalItems);
    return `${start} - ${end}`;
  }

  getBadgeClass(status: string): string {
    switch (status) {
      case 'Active': return 'badge badge-success';
      case 'Paused': return 'badge badge-warning';
      case 'Excluded': return 'badge badge-secondary';
      default: return 'badge badge-light';
    }
  }

  getConfidenceIcon(conf: string): string {
    switch (conf) {
      case 'High': return 'ki-check-circle text-success';
      case 'Medium': return 'ki-minus-circle text-warning';
      case 'Low': return 'ki-information-4 text-danger';
      case 'None': return 'ki-cross-circle text-muted';
      default: return 'ki-question-circle text-muted';
    }
  }
}
