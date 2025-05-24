import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {PagedTicket, PagedTickets, Ticket} from '../../../../core/models/ticket.model';
import {TicketService} from '../../../../core/services/ticket.service';
import {trigger, transition, style, animate} from '@angular/animations';
import {MatPaginator, MatPaginatorModule} from '@angular/material/paginator';
import { PageEvent } from '@angular/material/paginator';
import {debounceTime, distinctUntilChanged, Subject} from 'rxjs';



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

  tickets: PagedTicket[] = [];
  totalItems = 0;

  page = 1;
  pageSize = 10;
  pageSizes = [5, 10, 25, 100];

  loading: boolean;

  searchQuery: string = '';
  private searchChanged = new Subject<string>();

  constructor(private ticketsService: TicketService) {}

  ngOnInit() {
    this.loadTickets();

    this.searchChanged
      .pipe(
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe((searchTerm) => {
        this.page = 1; // reset to first page when filtering
        this.loadTickets();
      });
  }

  onSearchChange(): void {
    // You can debounce or trigger a filtered API call here
    this.searchChanged.next(this.searchQuery);
  }

  loadTickets() {
    this.loading = true

    this.ticketsService.getTickets(this.page, this.pageSize, this.searchQuery).subscribe({
      next: (res: PagedTickets)=> {
          this.tickets = res.tickets;
          this.totalItems = res.totalFilteredItems;

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
      case 'active': return 'badge badge-pill badge-success';
      case 'paused': return 'badge badge-pill badge-warning';
      case 'excluded': return 'badge badge-pill badge-secondary';
      default: return 'badge badge-pill badge-light';
    }
  }

  getConfidenceIcon(conf: string): string {
    switch (conf) {
      case 'high': return 'ki-check-circle text-success';
      case 'medium': return 'ki-minus-circle text-warning';
      case 'low': return 'ki-information-4 text-danger';
      case 'none': return 'ki-cross-circle text-muted';
      default: return 'ki-question-circle text-muted';
    }
  }

  formatLastResult(result: string | null): string {
    switch (result) {
      case 'Win':
        return '🎉 You matched a few numbers — small win!';
      case 'Miss':
        return '😞 No luck this time';
      case 'Awaiting':
        return '⏳ Waiting for draw results';
      case '—':
      case null:
      default:
        return '—';
    }
  }
}
