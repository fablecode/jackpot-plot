import {Injectable} from '@angular/core';
import {ConfigService} from './config.service';
import {HttpClient} from '@angular/common/http';
import {Ticket} from '../models/ticket.model';
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
    const allTickets: Ticket[] = [];

    const lotteries = ['Eurojackpot', 'Powerball', 'MegaMillions'];
    const statuses = ['Active', 'Paused', 'Excluded'];
    const confidences = ['High', 'Medium', 'Low', 'None'];
    const results = ['Win', 'Miss', 'Awaiting', '—'];

    for (let i = 1; i <= 120; i++) {
      const drawDate = new Date(Date.now() + Math.random() * 7 * 86400000);
      allTickets.push({
        isPublic: false,
        playCount: 0,
        id: this.generateUUIDv4(),
        name: `Ticket ${i}`,
        lottery: lotteries[i % lotteries.length],
        status: statuses[i % statuses.length] as Ticket['status'],
        nextDraw: i % 3 === 0 ? null : drawDate,
        entries: Math.floor(Math.random() * 20),
        lastResult: results[i % results.length],
        confidence: confidences[i % confidences.length] as Ticket['confidence']
      });
    }

    return allTickets;
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

  getTickets(page: number, pageSize: number): Observable<{ paginatedTickets: Ticket[], total: number }> {
    console.log(`Fetching tickets - Page: ${page}, Page Size: ${pageSize}`);

    // Always generate fresh tickets
    const allTickets = this.generateMockTickets();

    const start = (page - 1) * pageSize;
    const end = start + pageSize;
    return of({
      paginatedTickets: allTickets.slice(start, end),
      total: allTickets.length
    })
      .pipe(delay(1000));
  }
}
