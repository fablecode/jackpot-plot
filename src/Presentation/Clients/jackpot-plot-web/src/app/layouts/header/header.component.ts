import {Component, HostBinding, OnDestroy, OnInit} from '@angular/core';
import {AuthService} from '../../core/services/auth.service';
import {Router} from '@angular/router';
import {ROUTE_PATHS} from '../../core/constants/routes.constants';
import {Subscription} from 'rxjs';

@Component({
	selector: 'app-header',
	standalone: true,
	imports: [],
	templateUrl: './header.component.html',
	styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit, OnDestroy {
	@HostBinding('class') hostClass = 'header fixed top-0 z-10 left-0 right-0 flex items-stretch shrink-0 bg-[#fefefe] dark:bg-coal-500 shadow-sm dark:border-b dark:border-b-coal-100';
	@HostBinding('attr.role') hostRole = 'banner';
	@HostBinding('attr.data-sticky') dataSticky = 'true';
	@HostBinding('attr.data-sticky-name') dataStickyName = 'header';
	@HostBinding('id') hostId = 'header';

  isAuthenticated = false;
  private authSub!: Subscription;

  constructor(private router: Router, private authService : AuthService) {
  }

  async onLogoutClick(event: Event): Promise<void> {
    event.preventDefault(); // prevent the default href behavior
    // Perform logout logic here, like calling an auth service
    await this.authService.logout()
    // After logout, navigate to the sign-in page
    await this.router.navigate([`/${ROUTE_PATHS.DASHBOARD}/${ROUTE_PATHS.OVERVIEW}`]);
  }

  ngOnInit(): void {
    this.authSub = this.authService.isLoggedIn().subscribe(
      (authStatus) => {
        this.isAuthenticated = authStatus;
      }
    );
  }

  ngOnDestroy(): void {
    this.authSub.unsubscribe(); // clean up to avoid memory leaks
  }
}
