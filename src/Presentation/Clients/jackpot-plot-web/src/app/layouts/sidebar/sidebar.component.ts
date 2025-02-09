import {Component, HostBinding, OnInit} from '@angular/core';
import {MenuItem} from '../../core/menu/menu.model';
import {MenuService} from '../../core/menu/menu.service';
import {NgForOf, NgIf} from '@angular/common';
import {Router} from '@angular/router';

@Component({
	selector: 'app-sidebar',
	standalone: true,
  imports: [
    NgForOf,
    NgIf
  ],
	templateUrl: './sidebar.component.html',
	styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit {
	@HostBinding('class') hostClass = 'sidebar dark:bg-coal-600 bg-light border-r border-r-gray-200 dark:border-r-coal-100 fixed z-20 hidden lg:flex flex-col items-stretch shrink-0';
	@HostBinding('attr.data-drawer') drawer = 'true';
	@HostBinding('attr.data-drawer-class') drawerClass = 'drawer drawer-start top-0 bottom-0';
	@HostBinding('attr.data-drawer-enable') drawerEnable = 'true|lg:false';
	@HostBinding('attr.id') id = 'sidebar';

  menuItems: MenuItem[] = [];
  selectedItem: string = '';

  constructor(private router: Router, private menuService: MenuService) {}

  selectItem(item: any) {
    this.selectedItem = item.route;
    this.router.navigate([item.route]);
  }

  isSelected(item: any): boolean {
    return this.selectedItem === item.route;
  }

  ngOnInit(): void {
    this.menuItems = this.menuService.getMenuItems();
  }
}
