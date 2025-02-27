import {Injectable} from '@angular/core';
import {Observable, ReplaySubject} from 'rxjs';
import {HotColdNumbers} from '../../../../core/models/hot-cold-numbers';

@Injectable({
  providedIn: 'root' // ✅ Ensures it is available app-wide (change to 'any' for per-component scope)
})
export class HotColdNumbersService {
  private numbersSubject = new ReplaySubject<HotColdNumbers>(1);

  getNumbers(): Observable<HotColdNumbers> {
    return this.numbersSubject.asObservable();
  }

  updateNumbers(hotNumbers: Record<number, number>, coldNumbers: Record<number, number>): void {
    this.numbersSubject.next({ hotNumbers, coldNumbers });
  }
}
