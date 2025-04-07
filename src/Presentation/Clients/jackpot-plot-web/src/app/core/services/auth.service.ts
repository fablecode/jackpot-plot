import {effect, inject, Injectable} from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import {KEYCLOAK_EVENT_SIGNAL, KeycloakEventType, ReadyArgs, typeEventArgs} from 'keycloak-angular';
import Keycloak from 'keycloak-js';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private keycloak = inject(Keycloak);
  private readonly keycloakSignal = inject(KEYCLOAK_EVENT_SIGNAL);

  public authState$ = new BehaviorSubject<boolean>(false);

  constructor() {

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
    await this.keycloak.login();
  }

  async logout() {
    await this.keycloak.logout();
  }

  isLoggedIn() {
    return this.authState$.asObservable();
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
