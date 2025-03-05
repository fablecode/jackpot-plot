import {Injectable} from '@angular/core';
import {Observable, ReplaySubject} from 'rxjs';
import {LuckyPair} from '../../../../core/models/lucky-pair';

@Injectable({
  providedIn: 'root' // ✅ Ensures it is available app-wide (change to 'any' for per-component scope)
})
export class LuckyPairFrequencyService {
  private luckNumberFrequencySubject = new ReplaySubject<LuckyPair[]>(1);

  getLuckNumberFrequencies(): Observable<LuckyPair[]> {
    return this.luckNumberFrequencySubject.asObservable();
  }

  updateLuckNumberFrequencies(luckNumberFrequencies: LuckyPair[]): void {
    this.luckNumberFrequencySubject.next(luckNumberFrequencies);
  }
}
