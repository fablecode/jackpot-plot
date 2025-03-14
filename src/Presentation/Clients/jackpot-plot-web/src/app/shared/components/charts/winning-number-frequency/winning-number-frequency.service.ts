import {Injectable} from '@angular/core';
import {Observable, ReplaySubject} from 'rxjs';
import {WinningNumberFrequency} from '../../../../core/models/winning-number-frequency.model';

@Injectable({
  providedIn: 'root' // ✅ Ensures it is available app-wide (change to 'any' for per-component scope)
})
export class WinningNumberFrequencyService {
  private winningNumberFrequencies = new ReplaySubject<WinningNumberFrequency[]>(1);

  getWinningNumberFrequencies(): Observable<WinningNumberFrequency[]> {
    return this.winningNumberFrequencies.asObservable();
  }

  updateWinningNumberFrequencies(winningNumberFrequencies: WinningNumberFrequency[]): void {
    this.winningNumberFrequencies.next(winningNumberFrequencies);
  }
}
