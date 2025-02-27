import {Injectable} from '@angular/core';
import {Observable, ReplaySubject} from 'rxjs';

@Injectable({
  providedIn: 'root' // ✅ Ensures it is available app-wide (change to 'any' for per-component scope)
})
export class TrendingNumbersService {
  private trendingNumbersSubject = new ReplaySubject<Record<number, number>>(1);

  getTrendingNumbers(): Observable<Record<number, number>> {
    return this.trendingNumbersSubject.asObservable();
  }

  updateTrendingNumbers(trendingNumbers: Record<number, number>): void {
    this.trendingNumbersSubject.next(trendingNumbers);
  }
}
