import {effect, inject, Injectable} from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import {KEYCLOAK_EVENT_SIGNAL, KeycloakEventType, ReadyArgs, typeEventArgs} from 'keycloak-angular';
import Keycloak from 'keycloak-js';
import {Router} from '@angular/router';
import {AUTH_CONSTANTS} from '../constants/auth.constants';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private keycloak = inject(Keycloak);
  private readonly keycloakSignal = inject(KEYCLOAK_EVENT_SIGNAL);

  private authState$ = new BehaviorSubject<boolean>(false);

  constructor(private router: Router) {

    effect(() => {
      const keycloakEvent = this.keycloakSignal();

      if ([KeycloakEventType.Ready, KeycloakEventType.AuthSuccess].includes(keycloakEvent.type)) {
        this.authState$.next(typeEventArgs<ReadyArgs>(keycloakEvent.args));
      }

      if (keycloakEvent.type === KeycloakEventType.AuthLogout) {
        this.authState$.next(false);
      }
    });
  }

  async login() {
    // Save the current URL
    const currentUrl = this.router.url;

    sessionStorage.removeItem(AUTH_CONSTANTS.REDIRECT_AFTER_LOGIN);
    await this.keycloak.login({ redirectUri: window.location.origin + currentUrl });
  }

  async logout() {
    await this.keycloak.logout();
  }

  loggedInStatus$() {
    return this.authState$.asObservable();
  }

  isAuthenticated() {
    return this.authState$.getValue();
  }

  async hasRole(role: string): Promise<boolean> {
    return this.keycloak.hasRealmRole(role);
  }

  getUsername(): string | undefined {
    return this.keycloak.tokenParsed?.['preferred_username'];
  }

  getEmail(): string | undefined {
    return this.keycloak.tokenParsed?.['email'];
  }
}
