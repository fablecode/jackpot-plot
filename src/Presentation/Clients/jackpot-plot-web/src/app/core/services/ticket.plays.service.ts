import {HttpClient} from '@angular/common/http';
import {ConfigService} from './config.service';
import {TicketPlayInput} from '../models/input/ticket.play.input';
import {firstValueFrom, Observable} from 'rxjs';
import {Injectable} from '@angular/core';

@Injectable({
  providedIn: 'root' // Makes this service available throughout the app
})
export class TicketPlaysService {
  private readonly BASE_URL: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.BASE_URL = `${this.configService.apiBaseUrl}/api/tickets`; // Use config service
  }

  /**
   * Add ploys to ticket
   */
  addPlaysToTicket(ticketId: string, plays: TicketPlayInput[]): Observable<string[]> {
    return this.http.post<string[]>(`${this.BASE_URL}/${ticketId}/plays`, plays);
  }

  /**
   * Delete ploys from ticket
   */
  removePlaysFromTicket(ticketId: string, playIds: string[]): Observable<any> {
    return this.http.request('DELETE', `${this.BASE_URL}/${ticketId}/plays`, { body: playIds });
  }
}
