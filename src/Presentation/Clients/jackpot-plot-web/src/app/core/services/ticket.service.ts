import {Injectable} from '@angular/core';
import {ConfigService} from './config.service';
import {HttpClient} from '@angular/common/http';
import {PagedTickets, Ticket} from '../models/ticket.model';
import {delay, Observable, of} from 'rxjs';
import {TicketInput} from '../models/input/ticket.input';

@Injectable({
  providedIn: 'root' // Makes this service available throughout the app
})
export class TicketService {
  private readonly BASE_URL: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.BASE_URL = `${this.configService.apiBaseUrl}/api/tickets`; // Use config service
  }

  private generateMockTickets(): Ticket[] {
    const tickets: Ticket[] = [];

    const lotteries = ['Eurojackpot', 'Powerball', 'MegaMillions'];
    const statuses = ['Active', 'Paused', 'Excluded'];
    const confidences = ['High', 'Medium', 'Low', 'None'];

    const resultTiers = [
      null,
      { tier: 1, description: '🎯 Jackpot! Matched 5 + 2' },
      { tier: 2, description: '🥈 2nd Prize – Matched 5 + 1' },
      { tier: 3, description: '🥉 3rd Prize – Matched 5' },
      { tier: 4, description: '💵 Small Win – Matched 4 + 2' },
      { tier: 5, description: '🪙 Token Win – Matched 2 + 1' },
      null, // higher chance of miss
      null,
      null
    ];

    for (let i = 1; i <= 120; i++) {
      const drawDate = new Date(Date.now() + Math.random() * 7 * 86400000);

      const lastResult = resultTiers[Math.floor(Math.random() * resultTiers.length)];

      tickets.push({
        id: this.generateUUIDv4(),
        name: `Ticket ${i}`,
        lottery: lotteries[i % lotteries.length],
        status: statuses[i % statuses.length] as Ticket['status'],
        nextDraw: i % 3 === 0 ? null : drawDate,
        entries: Math.floor(Math.random() * 20),
        lastResult: lastResult,
        confidence: confidences[i % confidences.length] as Ticket['confidence'],
        isPublic: false,
        playCount: 0
      });
    }

    return tickets;
  }

  private generateUUIDv4(): string {
    // Generate a random UUID (version 4)
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  /**
   * Retrieve all user tickets
   */
  getUserTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(this.BASE_URL);
  }

  /**
   * Create user ticket
   */
  addTicket(ticket: TicketInput) {
    return this.http.post(this.BASE_URL, ticket, { observe: 'response' });
  }

  /**
   * Fetches paginated tickets from the backend via /api/tickets/search.
   *
   * @param page - The current page number (1-based).
   * @param pageSize - The number of tickets to retrieve per page.
   * @param searchTerm - (Optional) A search keyword to filter tickets.
   * @param sortColumn - (Optional) Column to sort by (default: 'ticket_id').
   * @param sortDirection - (Optional) Sort direction: 'asc' or 'desc' (default: 'asc').
   * @returns An observable containing tickets and pagination metadata.
   */
  getTickets(page: number, pageSize: number, searchTerm: string = '', sortColumn: string = 'ticket_id', sortDirection: string = 'asc'): Observable<PagedTickets> {
    const params = {
      pageNumber: page.toString(),
      pageSize: pageSize.toString(),
      searchTerm,
      sortColumn,
      sortDirection
    };

    return this.http.get<PagedTickets>(`${this.BASE_URL}/search`, { params });
  }
}
