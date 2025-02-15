import {Injectable} from '@angular/core';
import { MenuItem } from './menu.model';
import {ROUTE_PATHS} from '../constants/routes.constants';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private menuItems: MenuItem[] = [
    {
      id: ROUTE_PATHS.DASHBOARD,
      title: 'Dashboard',
      icon: 'ki-element-11',
      link: `/${ROUTE_PATHS.DASHBOARD}`,
      children: [
        { title: 'Overview', icon: 'ki-home', link: `/${ROUTE_PATHS.DASHBOARD}/${ROUTE_PATHS.OVERVIEW}`},
        { title: 'Trends',  icon: 'ki-chart-line-up-2', link: `/${ROUTE_PATHS.DASHBOARD}/${ROUTE_PATHS.TRENDS}`},
        { title: 'Hot & Cold Numbers', icon: 'ki-fire', link: `/${ROUTE_PATHS.DASHBOARD}/${ROUTE_PATHS.HOT_COLD_NUMBERS}`},
        { title: 'Winning Patterns', icon: 'ki-grid', link: `/${ROUTE_PATHS.DASHBOARD}/${ROUTE_PATHS.WINNING_PATTERNS}`}
      ]
    },
    {
      id: 'lottery',
      title: 'Lottery',
      icon: 'ki-scroll',
      link: '/lottery',
      children: [
        { title: 'Historical Results', link: '/overview'},
        { title: 'Number Frequency', link: '/overview'},
        { title: 'Draw History', link: '/overview'},
      ]
    },
    {
      id: 'predictions',
      title: 'Predictions',
      icon: 'ki-data  ',
      link: '/predictions',
      children: [
        { title: 'Number Generator', link: '/overview'},
        { title: 'Statistical Probability', link: '/overview'},
        { title: 'Custom Number Selection', link: '/overview'},
      ]
    },
    {
      id: 'draw insights',
      title: 'Draw Insights',
      icon: 'ki-chart-line-up',
      link: '/draw-insights',
      children: [
        { title: 'Past Draw Analysis', link: '/overview'},
        { title: 'Common Number Pairs', link: '/overview'},
        { title: 'Trends', link: '/overview'},
      ]
    },
    {
      id: 'winning strategies',
      title: 'Winning Strategies',
      icon: 'ki-route',
      link: '/winning-strategies',
      children: [
        { title: 'Mathematical Strategies', link: '/overview'},
        { title: 'User Guides', link: '/overview'},
        { title: 'Past Wins', link: '/overview'},
      ]
    },
    {
      id: 'comparison tool',
      title: 'Comparison Tool',
      icon: 'ki-parcel-tracking',
      link: '/comparison-tool',
      children: [
        { title: 'Compare Lotteries', link: '/overview'},
        { title: 'Prize Structures', link: '/overview'},
        { title: 'Best Odd', link: '/overview'},
      ]
    },
    {
      id: 'user tools',
      title: 'User Tools',
      icon: 'ki-setting-2',
      link: '/winning-strategies',
      children: [
        { title: 'Saved Predictions', link: '/overview'},
        { title: 'Custom Number Picker', link: '/overview'},
        { title: 'Favorite Numbers, link: ', link: '/overview'},
      ]
    },
    {
      id: 'community',
      title: 'Community',
      icon: 'ki-people',
      link: '/community',
      children: [
        { title: 'Discussion Forum', link: '/community'},
        { title: 'User Predictions', link: '/community'},
        { title: 'Winning Stories', link: '/community'},
      ]
    }
  ];

  getMenuItems(): MenuItem[] {
    return this.menuItems;
  }
}
