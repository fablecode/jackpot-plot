import {Injectable} from '@angular/core';
import {ConfigService} from './config.service';
import {HttpClient} from '@angular/common/http';
import {Ticket} from '../models/ticket.model';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root' // Makes this service available throughout the app
})
export class TicketService {
  private readonly BASE_URL: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.BASE_URL = `${this.configService.apiBaseUrl}/api/tickets`; // Use config service
  }

  /**
   * Retrieve all user tickets
   */
  getUserTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(this.BASE_URL);
  }
}
