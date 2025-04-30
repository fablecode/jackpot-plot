import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {ConfigService} from './config.service';
import {map, Observable} from 'rxjs';
import {Strategy} from '../models/strategy.model';


import {LotterySearchResult} from '../models/lotterySearchResult';
import {HotColdNumbers} from '../models/hot-cold-numbers';
import {NumberSpread} from '../models/numberSpread';
import {LuckyPair} from '../models/lucky-pair';
import {WinningNumberFrequency} from '../models/winning-number-frequency.model';

@Injectable({
  providedIn: 'root' // Makes this service available throughout the app
})
export class PredictionService {
  private readonly BASE_URL: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.BASE_URL = `${this.configService.apiBaseUrl}/api/predictions`; // Use config service
  }

  getAllStrategies(): Observable<Strategy[]> {
    return this.http.get<Strategy[]>(`${this.BASE_URL}/strategies`);
  }

  searchLottery(lotteryId: number, numberOfPlays: number, strategy: string): Observable<LotterySearchResult> {
    const searchParams = {
      lotteryId: lotteryId,
      numberOfPlays: numberOfPlays,
      strategy: strategy
    };
    return this.http.post<LotterySearchResult>(`${this.BASE_URL}`, searchParams);
  }

  getHotColdNumbers(lotteryId: number): Observable<HotColdNumbers> {
    return this.http.get<HotColdNumbers>(`${this.BASE_URL}/hot-cold-numbers?lotteryId=${lotteryId}`);
  }

  getTrendingNumbers(): Observable<Record<number, number>> {
    return this.http.get<Record<number, number>>(`${this.BASE_URL}/trending-numbers`);
  }

  getPredictionSuccessRate(): Observable<{ matchCount: number; frequency: number }[]> {
    return this.http.get<Record<number, number>>(`${this.BASE_URL}/success-rate`).pipe(
      map(response => Object.entries(response).map(([matchCount, frequency]) => ({
        matchCount: Number(matchCount),
        frequency: frequency
      })))
    );
  }

  getNumberSpread() : Observable<NumberSpread> {
    return this.http.get<NumberSpread>(`${this.BASE_URL}/number-spread`);
  }

  getLuckyPairFrequency(): Observable<LuckyPair[]> {
    return this.http.get<LuckyPair[]>(`${this.BASE_URL}/lucky-pair-frequency`);
  }

  getWinningNumberFrequency(): Observable<WinningNumberFrequency[]> {
    return this.http.get<WinningNumberFrequency[]>(`${this.BASE_URL}/winning-number-frequency`);
  }
}
