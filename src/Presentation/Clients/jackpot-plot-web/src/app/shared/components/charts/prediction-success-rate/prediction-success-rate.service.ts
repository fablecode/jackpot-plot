import {Injectable} from '@angular/core';
import {Observable, ReplaySubject} from 'rxjs';

@Injectable({
  providedIn: 'root' // ✅ Ensures it is available app-wide (change to 'any' for per-component scope)
})
export class PredictionSuccessRateService {
  private predictionSuccessRateSubject = new ReplaySubject<{ matchCount: number; frequency: number }[]>(1);

  getPredictionSuccessRate(): Observable<{ matchCount: number; frequency: number }[]> {
    return this.predictionSuccessRateSubject.asObservable();
  }

  updatePredictionSuccessRate(predictionSuccessRates: { matchCount: number; frequency: number }[]): void {
    this.predictionSuccessRateSubject.next(predictionSuccessRates);
  }
}
