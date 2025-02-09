import {Injectable} from '@angular/core';
import { MenuItem } from './menu.model';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private menuItems: MenuItem[] = [
    {
      title: 'Dashboard',
      icon: 'ki-element-11',
      link: '/dashboard',
      children: [
        { title: 'Overview', icon: 'ki-home', link: '/overview'},
        { title: 'Trends', icon: 'ki-chart-line-up-2', link: '/trends'},
        { title: 'Hot & Cold Numbers', icon: 'ki-fire', link: '/hot-and-cold-numbers'},
        { title: 'Winning Patterns', icon: 'ki-grid', link: '/winning-patterns'}
      ]
    },
    {
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
      title: 'Predictions',
      icon: 'ki-route  ',
      link: '/predictions',
      children: [
        { title: 'Number Generator', link: '/overview'},
        { title: 'Statistical Probability', link: '/overview'},
        { title: 'Custom Number Selection', link: '/overview'},
      ]
    },
    {
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
      title: 'Winning Strategies',
      icon: 'ki-data',
      link: '/winning-strategies',
      children: [
        { title: 'Mathematical Strategies', link: '/overview'},
        { title: 'User Guides', link: '/overview'},
        { title: 'Past Wins', link: '/overview'},
      ]
    },
    {
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
