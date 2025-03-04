import {Observable, ReplaySubject} from 'rxjs';
import {NumberSpread} from '../../../../core/models/numberSpread';
import {Injectable} from '@angular/core';

@Injectable({
  providedIn: 'root' // ✅ Ensures it is available app-wide (change to 'any' for per-component scope)
})
export class NumberSpreadAnalysisService {
  private numberSpreadAnalysisSubject = new ReplaySubject<NumberSpread>(1);

  getNumberSpreadAnalysis(): Observable<NumberSpread> {
    return this.numberSpreadAnalysisSubject.asObservable();
  }

  updateNumberSpreadAnalysis(numberSpread: NumberSpread): void {
    this.numberSpreadAnalysisSubject.next(numberSpread);
  }
}
