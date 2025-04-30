import { Injectable } from "@angular/core";
import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {catchError, Observable, throwError} from "rxjs";
import {AuthService} from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private loginTriggered = false;

  constructor(private authService: AuthService) {}

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
      return next.handle(req).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401 && !this.loginTriggered) {
            this.loginTriggered = true;

            this.authService.login().then(() => {
              // Redirect will happen in Keycloak
              this.loginTriggered = false;
            });

            // Return empty observable to prevent further handling
            return new Observable<HttpEvent<any>>();
          }

          return throwError(() => error);
        })
      );
    }
}
