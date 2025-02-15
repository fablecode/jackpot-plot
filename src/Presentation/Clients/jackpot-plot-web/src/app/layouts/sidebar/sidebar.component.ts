import {AfterViewInit, Component, HostBinding, OnInit} from '@angular/core';
import {MenuItem} from '../../core/menu/menu.model';
import {MenuService} from '../../core/menu/menu.service';
import {NgForOf, NgIf} from '@angular/common';
import {NavigationEnd, Router, RouterLink} from '@angular/router';
import {filter} from 'rxjs';

@Component({
	selector: 'app-sidebar',
	standalone: true,
  imports: [
    NgForOf,
    NgIf,
    RouterLink
  ],
	templateUrl: './sidebar.component.html',
	styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit, AfterViewInit {
	@HostBinding('class') hostClass = 'sidebar dark:bg-coal-600 bg-light border-r border-r-gray-200 dark:border-r-coal-100 fixed z-20 hidden lg:flex flex-col items-stretch shrink-0';
	@HostBinding('attr.data-drawer') drawer = 'true';
	@HostBinding('attr.data-drawer-class') drawerClass = 'drawer drawer-start top-0 bottom-0';
	@HostBinding('attr.data-drawer-enable') drawerEnable = 'true|lg:false';
	@HostBinding('attr.id') id = 'sidebar';

  menuItems: MenuItem[] = [];
  expandedMenus: { [key: string]: boolean } = {}; // Stores expanded state

  constructor(private router: Router, private menuService: MenuService) {}

  isActive(path: string): boolean {
    return this.router.isActive(path, { paths: 'subset', queryParams: 'ignored', fragment: 'ignored', matrixParams: 'ignored' });
  }

  isExpanded(menuId: string): boolean {
    return !!this.expandedMenus[menuId];
  }

  checkActiveRoutes() {
    // Expand accordions that have an active child route
    this.menuItems.forEach(menu => {
      this.expandedMenus[menu.id] = menu.children.some(item => this.router.isActive(item.link, { paths: 'subset', queryParams: 'ignored', fragment: 'ignored', matrixParams: 'ignored' }));
    });
  }

  initializeMetronicMenu() {
    // Ensure Metronic detects changes after Angular updates the state
    if (typeof window !== 'undefined' && (window as any).KTMenu) {
      (window as any).KTMenu.init();
    }
  }

  ngOnInit(): void {
    this.menuItems = this.menuService.getMenuItems();
    this.checkActiveRoutes();

    // Re-check active routes on every navigation
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(() => {
      this.checkActiveRoutes();
    });
  }

  ngAfterViewInit() {
    this.initializeMetronicMenu(); // Ensure Metronic initializes correctly
  }
}
