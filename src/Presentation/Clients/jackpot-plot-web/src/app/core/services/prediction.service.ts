import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {ConfigService} from './config.service';
import {Observable} from 'rxjs';
import {Strategy} from '../models/strategy.model';

@Injectable({
  providedIn: 'root' // Makes this service available throughout the app
})
export class PredictionService {
  private readonly BASE_URL: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.BASE_URL = `${this.configService.apiBaseUrl}/api/predictions`; // Use config service
  }

  getAllStrategies(): Observable<Strategy[]> {
    return this.http.get<Strategy[]>(`${this.BASE_URL}`);
  }
}
