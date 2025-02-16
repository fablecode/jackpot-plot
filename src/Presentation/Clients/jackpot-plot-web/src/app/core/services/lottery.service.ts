import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {ConfigService} from './config.service';
import {Lottery} from '../models/lottery.model';

@Injectable({
  providedIn: 'root' // Makes this service available throughout the app
})
export class LotteryService {
  private readonly BASE_URL: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.BASE_URL = `${this.configService.apiBaseUrl}/api/lotteries`; // Use config service
  }

  getAllLotteries(): Observable<Lottery[]> {
    return this.http.get<Lottery[]>(`${this.BASE_URL}`);
  }
}
